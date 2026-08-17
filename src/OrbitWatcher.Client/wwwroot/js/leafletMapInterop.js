const mapStates = new Map();

const constellationColors = {
    "GPS":     "#4285F4",
    "GLONASS": "#EA4335",
    "Galileo": "#FBBC04",
    "BeiDou":  "#34A853",
    "QZSS":    "#FF6D01",
    "NavIC":   "#9334E6",
    "SBAS":    "#E91E8F",
    "Other":   "#9AA0A6"
};

function getPropertyValue(source, camelName, pascalName) {
    return source[camelName] ?? source[pascalName];
}

function formatNumber(value, digits) {
    if (typeof value !== "number") {
        return "—";
    }

    return value.toFixed(digits);
}

function buildPopupContent(satellite) {
    const name = getPropertyValue(satellite, "name", "Name") ?? "Unknown";
    const noradCatId = getPropertyValue(satellite, "noradCatId", "NoradCatId") ?? "—";
    const constellation = getPropertyValue(satellite, "constellation", "Constellation") ?? "—";
    const lat = getPropertyValue(satellite, "lat", "Lat");
    const lon = getPropertyValue(satellite, "lon", "Lon");
    const altKm = getPropertyValue(satellite, "altKm", "AltKm");
    const vx = getPropertyValue(satellite, "velocityXKmS", "VelocityXKmS");
    const vy = getPropertyValue(satellite, "velocityYKmS", "VelocityYKmS");
    const vz = getPropertyValue(satellite, "velocityZKmS", "VelocityZKmS");
    const speed = getPropertyValue(satellite, "speedKmS", "SpeedKmS");
    const period = getPropertyValue(satellite, "orbitalPeriodMin", "OrbitalPeriodMin");
    const inclination = getPropertyValue(satellite, "inclinationDeg", "InclinationDeg");
    const eccentricity = getPropertyValue(satellite, "eccentricity", "Eccentricity");
    const perigee = getPropertyValue(satellite, "perigeeKm", "PerigeeKm");
    const apogee = getPropertyValue(satellite, "apogeeKm", "ApogeeKm");
    const timestampUtc = getPropertyValue(satellite, "timestampUtc", "TimestampUtc") ?? "—";

    return `<strong>${name}</strong><br/>NORAD: ${noradCatId} · ${constellation}` +
        `<hr style="margin:4px 0"/>` +
        `Lat: ${formatNumber(lat, 6)}<br/>Lon: ${formatNumber(lon, 6)}<br/>Alt: ${formatNumber(altKm, 2)} km` +
        `<hr style="margin:4px 0"/>` +
        `Vx: ${formatNumber(vx, 3)} km/s<br/>Vy: ${formatNumber(vy, 3)} km/s<br/>Vz: ${formatNumber(vz, 3)} km/s<br/>Speed: ${formatNumber(speed, 3)} km/s` +
        `<hr style="margin:4px 0"/>` +
        `Period: ${formatNumber(period, 2)} min<br/>Inclination: ${formatNumber(inclination, 2)}°<br/>Eccentricity: ${formatNumber(eccentricity, 6)}<br/>Perigee: ${formatNumber(perigee, 2)} km<br/>Apogee: ${formatNumber(apogee, 2)} km` +
        `<hr style="margin:4px 0"/>` +
        `UTC: ${timestampUtc}`;
}

function getMarkerColor(satellite) {
    const constellation = getPropertyValue(satellite, "constellation", "Constellation") ?? "Other";
    return constellationColors[constellation] || constellationColors["Other"];
}

function attachClickHandler(mapState, markerKey, marker) {
    if (!mapState.clickCallback) {
        return;
    }

    marker.on("click", () => {
        const noradCatId = parseInt(markerKey, 10);
        if (!isNaN(noradCatId)) {
            mapState.clickCallback.dotNetRef.invokeMethodAsync(
                mapState.clickCallback.methodName,
                noradCatId
            );
        }
    });
}

export function initializeMap(mapId, element, centerLat, centerLon, zoom) {
    if (mapStates.has(mapId)) {
        return;
    }

    const map = L.map(element, {
        worldCopyJump: true
    }).setView([centerLat, centerLon], zoom);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: "&copy; OpenStreetMap contributors"
    }).addTo(map);

    mapStates.set(mapId, {
        map,
        markers: new Map(),
        groundTrackLayers: [],
        clickCallback: null
    });

    setTimeout(() => map.invalidateSize(), 0);
}

export function upsertMarkers(mapId, satellites) {
    const mapState = mapStates.get(mapId);
    if (!mapState || !Array.isArray(satellites)) {
        return;
    }

    for (const satellite of satellites) {
        const noradCatId = getPropertyValue(satellite, "noradCatId", "NoradCatId");
        const lat = getPropertyValue(satellite, "lat", "Lat");
        const lon = getPropertyValue(satellite, "lon", "Lon");

        if (noradCatId === undefined || typeof lat !== "number" || typeof lon !== "number") {
            continue;
        }

        const markerKey = noradCatId.toString();
        const color = getMarkerColor(satellite);
        let marker = mapState.markers.get(markerKey);

        if (!marker) {
            marker = L.circleMarker([lat, lon], {
                radius: 5,
                fillColor: color,
                color: "#fff",
                weight: 1,
                opacity: 1,
                fillOpacity: 0.9
            });
            marker.addTo(mapState.map);
            mapState.markers.set(markerKey, marker);
            attachClickHandler(mapState, markerKey, marker);
        } else {
            marker.setLatLng([lat, lon]);
            marker.setStyle({ fillColor: color });
        }

        marker.bindPopup(buildPopupContent(satellite));
    }
}

export function removeMarkersExcept(mapId, noradIds) {
    const mapState = mapStates.get(mapId);
    if (!mapState || !Array.isArray(noradIds)) {
        return;
    }

    const validIds = new Set(noradIds.map(id => id.toString()));
    for (const [markerKey, marker] of mapState.markers.entries()) {
        if (validIds.has(markerKey)) {
            continue;
        }

        mapState.map.removeLayer(marker);
        mapState.markers.delete(markerKey);
    }
}

export function drawGroundTrack(mapId, segments) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    // Clear existing ground track layers
    clearGroundTrackLayers(mapState);

    if (!Array.isArray(segments)) {
        return;
    }

    for (const segment of segments) {
        if (!Array.isArray(segment) || segment.length < 2) {
            continue;
        }

        const latlngs = segment.map(point => {
            const lat = getPropertyValue(point, "lat", "Lat");
            const lon = getPropertyValue(point, "lon", "Lon");
            return [lat, lon];
        });

        const polyline = L.polyline(latlngs, {
            color: "#00ccff",
            weight: 2,
            opacity: 0.8,
            dashArray: null
        });

        polyline.addTo(mapState.map);
        mapState.groundTrackLayers.push(polyline);
    }
}

export function clearGroundTrack(mapId) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    clearGroundTrackLayers(mapState);
}

function clearGroundTrackLayers(mapState) {
    for (const layer of mapState.groundTrackLayers) {
        mapState.map.removeLayer(layer);
    }
    mapState.groundTrackLayers = [];
}

export function setMarkerClickCallback(mapId, dotNetRef, methodName) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    mapState.clickCallback = { dotNetRef, methodName };

    // Attach click handlers to all existing markers
    for (const [markerKey, marker] of mapState.markers.entries()) {
        attachClickHandler(mapState, markerKey, marker);
    }
}

export function disposeMap(mapId) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    clearGroundTrackLayers(mapState);
    mapState.map.remove();
    mapStates.delete(mapId);
}
