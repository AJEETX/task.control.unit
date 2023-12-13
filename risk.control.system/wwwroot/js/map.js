var map;
function haversine_distance(mk1, mk2) {
    var R = 3958.8; // Radius of the Earth in miles
    var rlat1 = mk1.position.lat() * (Math.PI / 180); // Convert degrees to radians
    var rlat2 = mk2.position.lat() * (Math.PI / 180); // Convert degrees to radians
    var difflat = rlat2 - rlat1; // Radian difference (latitudes)
    var difflon = (mk2.position.lng() - mk1.position.lng()) * (Math.PI / 180); // Radian difference (longitudes)

    var d = 2 * R * Math.asin(Math.sqrt(Math.sin(difflat / 2) * Math.sin(difflat / 2) + Math.cos(rlat1) * Math.cos(rlat2) * Math.sin(difflon / 2) * Math.sin(difflon / 2)));
    return d * 1.609;
}
function initReportMap() {
    var claimId = document.getElementById('claimId').value;
    var response = $.ajax({
        type: "GET",
        url: "/api/ClaimsInvestigation/GetFaceDetail?claimId=" + claimId,
        async: false
    }).responseText;
    var center = {
        lat: 40.774102,
        lng: -73.971734
    };
    var dakota = {
        lat: 40.7767644,
        lng: -73.9761399
    };
    var frick = {
        lat: 40.771209,
        lng: -73.9673991
    };
    if (response) {
        var data = JSON.parse(response);
        if (data && data.center && data.dakota && data.frick) {
            center = data.center;
            dakota = data.dakota;
            frick = data.frick
        }
    }

    initFaceMap(center, dakota, frick);

    var ocrResponse = $.ajax({
        type: "GET",
        url: "/api/ClaimsInvestigation/GetOcrDetail?claimId=" + claimId,
        async: false
    }).responseText;

    var ocenter = {
        lat: 40.774102,
        lng: -73.971734
    };
    var odakota = {
        lat: 40.7767644,
        lng: -73.9761399
    };
    var ofrick = {
        lat: 40.771209,
        lng: -73.9673991
    };

    if (ocrResponse) {
        var odata = JSON.parse(ocrResponse);
        if (odata && odata.center && odata.dakota && odata.frick) {
            ocenter = odata.center;
            odakota = odata.dakota;
            ofrick = odata.frick
        }
    }

    initOcrMap(ocenter, odakota, ofrick);
}

function initFaceMap(center, dakota, frick) {
    const options = {
        scaleControl: true,
        center: center,
        mapId: "4504f8b37365c3d0",
        mapTypeId: google.maps.MapTypeId.ROADMAP,
    };
    map = new google.maps.Map(
        document.getElementById('face-map'),
        options);
    // Locations of landmarks

    // The markers for The Dakota and The Frick Collection
    var mk1 = new
        google.maps.Marker({
            position: dakota,
            map: map
        });
    var mk2 = new
        google.maps.Marker({
            position: frick,
            map: map
        });

    var bounds = new google.maps.LatLngBounds();
    bounds.extend(dakota);
    bounds.extend(frick);

    // Draw a line showing the straight distance between the markers
    var line = new google.maps.Polyline({ path: [dakota, frick], map: map });

    // Calculate and display the distance between markers
    var distance = haversine_distance(mk1, mk2);
    document.getElementById('face-msg').innerHTML = "Distance between expected vs visited location: " + distance.toFixed(2) + " km.";
    let directionsService = new google.maps.DirectionsService();
    let directionsRenderer = new google.maps.DirectionsRenderer();
    directionsRenderer.setMap(map); // Existing map object displays directions
    // Create route from existing points used for markers
    const route = {
        origin: dakota,
        destination: frick,
        travelMode: 'DRIVING'
    }

    directionsService.route(route,
        function (response, status) { // anonymous function to capture directions
            if (status !== 'OK') {
                console.log('Directions request failed due to ' + status);
                return;
            } else {
                directionsRenderer.setDirections(response); // Add route to the map
                var directionsData = response.routes[0].legs[0]; // Get data about the mapped route
                if (!directionsData) {
                    window.alert('Directions request failed');
                    return;
                }
                else {
                    document.getElementById('face-msg').innerHTML += "<br>";
                    document.getElementById('face-msg').innerHTML += " Driving distance is " + directionsData.distance.text + " (" + directionsData.duration.text + ").";
                }
            }
        });
    map.fitBounds(bounds);
    map.setCenter(bounds.getCenter());
    map.setZoom(map.getZoom() - 1);
    if (map.getZoom() > 18) {
        map.setZoom(18);
    }
}
function initOcrMap(center, dakota, frick) {
    const options = {
        scaleControl: true,
        center: center,
        mapId: "4504f8b37365c3d0",
        mapTypeId: google.maps.MapTypeId.ROADMAP,
    };
    map = new google.maps.Map(
        document.getElementById('ocr-map'),
        options);

    // The markers for The Dakota and The Frick Collection
    var mk1 = new
        google.maps.Marker({
            position: dakota,
            map: map
        });
    var mk2 = new
        google.maps.Marker({
            position: frick,
            map: map
        });

    var bounds = new google.maps.LatLngBounds();
    bounds.extend(dakota);
    bounds.extend(frick);

    // Draw a line showing the straight distance between the markers
    var line = new google.maps.Polyline({ path: [dakota, frick], map: map });

    // Calculate and display the distance between markers
    var distance = haversine_distance(mk1, mk2);
    document.getElementById('ocr-msg').innerHTML = "Distance between expected vs visited location: " + distance.toFixed(2) + " km.";
    let directionsService = new google.maps.DirectionsService();
    let directionsRenderer = new google.maps.DirectionsRenderer();
    directionsRenderer.setMap(map); // Existing map object displays directions
    // Create route from existing points used for markers
    const route = {
        origin: dakota,
        destination: frick,
        travelMode: 'DRIVING'
    }

    directionsService.route(route,
        function (response, status) { // anonymous function to capture directions
            if (status !== 'OK') {
                console.log('Directions request failed due to ' + status);
                return;
            } else {
                directionsRenderer.setDirections(response); // Add route to the map
                var directionsData = response.routes[0].legs[0]; // Get data about the mapped route
                if (!directionsData) {
                    window.alert('Directions request failed');
                    return;
                }
                else {
                    document.getElementById('ocr-msg').innerHTML += "<br>";
                    document.getElementById('ocr-msg').innerHTML += " Driving distance is " + directionsData.distance.text + " (" + directionsData.duration.text + ").";
                }
            }
        });
    map.fitBounds(bounds);
    map.setCenter(bounds.getCenter());
    map.setZoom(map.getZoom() - 1);
    if (map.getZoom() > 18) {
        map.setZoom(18);
    }
}