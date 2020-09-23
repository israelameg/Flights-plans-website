
var planeIcon = L.icon({
    iconUrl: '/design/airplane.png',
    iconSize: [30, 30] // size of the icon
    //iconAnchor: [22, 94], // point of the icon which will correspond to marker's location
    //popupAnchor: [-3, -76]  // point from which the popup should open relative to the iconAnchor
});
var flights = new Map(); var markersMap = new Map();
var poly;
var map;

//function adds map when uploading website.
function addMap() {
    //add the map
    map = L.map('mapid').setView([51.505, -0.09], 5);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    map.on('click', clickOnMap);
}
//load the map when open the web
window.addEventListener("load", addMap);

//function adds all flights to map.
function addFlights(data) {
    data.forEach(element => updateFlightIcon(element));
}
//event funtion when clicked on map.
function clickOnMap() {
    removePoly();
    unHighlightRow();
}
//remove path of flight from map.
function removePoly() {
    if (poly != undefined) {
        map.removeControl(poly);
        poly = undefined;
    }
}
//function gets message from plane.
function getMessage(plane) {
    var msg = "";
    msg += "Passengers: ";
    msg += plane.Passengers;
    //msg
    return msg;

}

//function draws path of flight.
function drawLine(p) {
    removePoly();
    let locations = getLocations(p);
    poly = L.polyline(locations).addTo(map);
}
//function gets all locations of flight.
function getLocations(p) {

    let segments = p.segments; let lat; let lng;
    let locations = [];
    locations.push([p.initial_location.latitude, p.initial_location.longitude])
    let i = 0;
    for (i = 0; i < segments.length; i++) {
        let element = segments[i];
        lat = element.latitude;
        lng = element.longitude;
        locations.push([lat, lng]);
    }
    return locations;
}
//fucntion get all time segments.
function getSegTimes(p) {
    let segments = p.segments;
    let i = 0;
    let times = [];
    for (i = 0; i < segments.length; i++) {
        let element = segments[i];
        let t = element.timespanSeconds*1000;
        times.push(t);
    }
    return times;
}

//function adds flight or update flight location on map.
function updateFlightIcon(flight) {
    
    if (flight == undefined) {
        return;
    }
    if (!flights.has(flight.flight_Id)) {
        flights.set(flight.flight_Id, flight);
    }
    var location = [flight.latitude.toFixed(2), flight.longitude.toFixed(2)];
    if (markersMap.has(flight.flight_Id)) {
        let mark = markersMap.get(flight.flight_Id);
        mark.setLatLng(location).update();
    }
    else {
        let mark = L.marker(location, { icon: planeIcon }).addTo(map);
        mark._icon.id = "marker_" + flight.flight_Id;
        mark.bindPopup(" ");
        mark.on('click', function (e) {
            getFlightPlan(flight);

        });
        markersMap.set(flight.flight_Id, mark);
    }
}
//event function when click on marker.
function clickOnMarker(p,flight) {
    drawLine(p);
    highlightRow(p, flight);
}
//get the flight plan from server, given the flight.
function getFlightPlan(flight) {
    let id = flight.flight_Id;
    let curr_url = "/api/FlightPlan/" + id;
    $.get(curr_url, function (data, status) {
        try {
            clickOnMarker(data,flight);
            return data;
        } catch (e) {
            return undefined;
        }

    });
}


