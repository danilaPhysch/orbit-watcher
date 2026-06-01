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
        markers: new Map()
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

export function disposeMap(mapId) {
    const mapState = mapStates.get(mapId);
    if (!mapState) {
        return;
    }

    mapState.map.remove();
    mapStates.delete(mapId);
}
