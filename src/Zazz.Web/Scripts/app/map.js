var geocoder;
var map;
var marker;
var infowindow = new google.maps.InfoWindow();
var latitude;
var longitude;

function placeMarker(location, zoom) {
    if (marker != null) {
        marker.setMap(null);
    }

    marker = new google.maps.Marker({
        position: location,
        map: map
    });

    //console.log(location.lat());
    //console.log(location.lng());

    $('input[name="Latitude"]').val(location.lat());
    $('input[name="Longitude"]').val(location.lng());

    map.setCenter(location);

    if (zoom)
        map.setZoom(zoom);
}

function initMap(lat, lng) {
    geocoder = new google.maps.Geocoder();

    var mapOptions = {
        center: new google.maps.LatLng(lat, lng),
        zoom: 8,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    map = new google.maps.Map(document.getElementById("map-canvas"),
        mapOptions);

    google.maps.event.addListener(map, 'click', function (event) {
        placeMarker(event.latLng);
    });
}

function getMarkerPosition() {
    if (marker == undefined) {
        alert("position is not set");
        return null;
    }

    var pos = marker.getPosition();
    var lat = pos.lat();
    var lng = pos.lng();

    return {
        lat: lat,
        lng: lng
    };
}