const mapStates = new Map();

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
    const lat = getPropertyValue(satellite, "lat", "Lat");
    const lon = getPropertyValue(satellite, "lon", "Lon");
    const altKm = getPropertyValue(satellite, "altKm", "AltKm");
    const timestampUtc = getPropertyValue(satellite, "timestampUtc", "TimestampUtc") ?? "—";

    return `<strong>${name}</strong><br/>NORAD: ${noradCatId}<br/>Lat: ${formatNumber(lat, 6)}<br/>Lon: ${formatNumber(lon, 6)}<br/>Alt (km): ${formatNumber(altKm, 2)}<br/>UTC: ${timestampUtc}`;
}

export function initializeMap(mapId, element, centerLat, centerLon, zoom, dotNetRef) {
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
        dotNetRef,
        pastTrackLayer: null,
        futureTrackLayer: null
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
        let marker = mapState.markers.get(markerKey);

        if (!marker) {
            marker = L.marker([lat, lon]);
            marker.addTo(mapState.map);
            mapState.markers.set(markerKey, marker);

            marker.on("click", () => {
                if (mapState.dotNetRef) {
                    mapState.dotNetRef.invokeMethodAsync("OnSatelliteSelected", noradCatId);
                }
            });
        } else {
            marker.setLatLng([lat, lon]);
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

export function drawGroundTrack(mapId, groundTrack) {
    const mapState = mapStates.get(mapId);
    if (!mapState || !groundTrack) {
        return;
    }

    clearGroundTrack(mapId);

    const mapSegments = (segments) => {
        if (!segments || !Array.isArray(segments)) return [];
        return segments.map(segment =>
            segment.map(pt => [
                getPropertyValue(pt, "lat", "Lat"),
                getPropertyValue(pt, "lon", "Lon")
            ])
        );
    };

    const pastSegments = mapSegments(getPropertyValue(groundTrack, "pastTrack", "PastTrack"));
    const futureSegments = mapSegments(getPropertyValue(groundTrack, "futureTrack", "FutureTrack"));

    if (pastSegments.length > 0) {
        mapState.pastTrackLayer = L.polyline(pastSegments, {
            color: '#1b6ec2',
            weight: 3,
            dashArray: '5, 8',
            opacity: 0.6
        }).addTo(mapState.map);
    }

    if (futureSegments.length > 0) {
        mapState.futureTrackLayer = L.polyline(futureSegments, {
            color: '#ff4d4f',
            weight: 3,
            opacity: 0.8
        }).addTo(mapState.map);
    }
}

export function clearGroundTrack(mapId) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    if (mapState.pastTrackLayer) {
        mapState.map.removeLayer(mapState.pastTrackLayer);
        mapState.pastTrackLayer = null;
    }

    if (mapState.futureTrackLayer) {
        mapState.map.removeLayer(mapState.futureTrackLayer);
        mapState.futureTrackLayer = null;
    }
}

export function disposeMap(mapId) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    clearGroundTrack(mapId);
    mapState.map.remove();
    mapStates.delete(mapId);
}
