var data;
var current_data;
function initMap(url) {
    var response = $.ajax({
        type: "GET",
        url: url,
        async: false
    }).responseText;
    data = JSON.parse(response);

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(success);
    } else {
        alert("There is Some Problem on your current browser to get Geo Location!");
    }
}

async function success(position) {
    var hexData = 'AIzaSyDH8T9FvJ8n2LNwxkppRAeOq3Mx7I3qi1E';
    const { Map, InfoWindow } = await google.maps.importLibrary("maps");
    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
    var bounds = new google.maps.LatLngBounds();
    var lat = position.coords.latitude;
    var long = position.coords.longitude;
    var center = {
        lat: position.coords.latitude,
        lng: position.coords.longitude
    };
    var locresponse = $.ajax({
        type: "GET",
        url: `https://maps.googleapis.com/maps/api/geocode/json?latlng=${lat},${long}&sensor=false&key=${hexData}`,
        async: false
    }).responseText;
    current_data = JSON.parse(locresponse);

    var LatLng = new google.maps.LatLng(lat, long);
    var mapOptions = {
        center: LatLng,
        zoom: 10,
        mapId: "4504f8b37365c3d0",
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    var map = new google.maps.Map(document.getElementById("map"), mapOptions);
    var marker = new google.maps.Marker({
        position: LatLng,
        title: "You are here: " + current_data.results[0].formatted_address
    });

    marker.setMap(map);
    var getInfoWindow = new google.maps.InfoWindow({
        content: "<b>Your Current Location</b><br/> " +
            current_data.results[0].formatted_address + ""
    });
    getInfoWindow.open(map, marker);
    const infoWindow = new InfoWindow();

    if (data.response.length > 0) {
        bounds.extend(LatLng);
        for (const property of data.response) {
            const marker = new google.maps.marker.AdvancedMarkerElement({
                map,
                content: buildContent(property),
                position: property.position,
                title: property.description,
            });

            marker.addListener("gmp-click", ({ domEvent, latLng }) => {
                toggleHighlight(marker, property, domEvent, infoWindow);
            });
            bounds.extend(property.position);
        }

        map.setZoom(map.getZoom() - 1);
        if (map.getZoom() > 18) {
            map.setZoom(18);
        }
    } else {
        bounds.extend(center);
        map.setZoom(18);
    }

    map.fitBounds(bounds);
    map.setCenter(bounds.getCenter());
}
function toggleHighlight(marker, property, domEvent, infoWindow) {
    if (marker.content.classList.contains("highlight")) {
        marker.content.classList.remove("highlight");
        //window.location.href = property.url;
        marker.zIndex = null;
    } else {
        marker.content.classList.add("highlight");
        marker.zIndex = 1;
    }
    //infoWindow.close();
    //infoWindow.setContent(buildContent(property));
    //infoWindow.open(marker.map, marker);
}

function buildContent(property) {
    const content = document.createElement("div");

    content.classList.add("property");
    content.innerHTML = `
                                        <div class="icon">
                                            <i aria-hidden="true" class="fa fa-icon fa-${property.type}" title="${property.address}"></i>
                                            <span class="fa-sr-only">${property.address}</span>
                                        </div>
                                        <div class="details">
                                            <div class="price">$ ${property.price}</div>
                                            <div class="address"><i aria-hidden="true" class="fas fa-home" title="bedroom"></i> <a href="${property.url}"> ${property.address}</a></div>
                                            <div class="features">
                                            <div>
                                                        <i aria-hidden="true" class="fas fa-rupee-sign" title="bedroom"></i>
                                                <span class="fa-sr-only">bedroom</span>
                                                <span>${property.bed}</span>
                                            </div>
                                            <div>
                                                        <i aria-hidden="true" class="fas fa-phone-square-alt" title="bathroom"></i>
                                                <span class="fa-sr-only">bathroom</span>
                                                <span>${property.bath}</span>
                                            </div>
                                            <div>
                                                <i aria-hidden="true" class="fa fa-ruler fa-lg size" title="size"></i>
                                                <span class="fa-sr-only">size</span>
                                                <span>${property.size} <sup>2</sup></span>
                                            </div>
                                            </div>
                                        </div>
                                        `;
    return content;
}