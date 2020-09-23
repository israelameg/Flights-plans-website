// send interval every 4 seconds
setInterval(getFlights, 250);
// gets flights from the servers

var succededToSend = true;

function getFlights() {
    //getInternalFlights();
    detailTable();
    let currDate = new Date();
    let curr_url = '/api/Flights?relative_to=' + currDate.toISOString() + "&sync_all";
    $.get(curr_url, function (data, status) {
        try {
            console.log(data);
            handleGetData(data);
        } catch (e) {
            return;
        }
        succededToSend = true;
    }).fail(function () {
        if (succededToSend) {
            alert("Messege could not be sent. Check your connection again");
        }
        succededToSend = false;
    });

}
//given data, add it to map and to table.
function handleGetData(data) {
    if (data == undefined) {
        return;
    }
    console.log(data);
    addFlights(data);
    addRows(data);

}
//gets only the internal flights.
function getInternalFlights() {
    let currDate = new Date();
    let curr_url = '/api/Flights?relative_to=' + currDate.toISOString();
    $.get(curr_url, function (data, status) {
        try {
            handleGetData(data);
        } catch (e) {
            return;
        }
        succededToSend = true;
    }).fail(function () {
        if (succededToSend) {
            alert("Messege could not be sent. Check your connection again");
        }
        succededToSend = false;
    });

}

//delete flight.
function deleteFlight(id) {
    console.log("id = " + id);
    deleteRow("internal-row-" + id);
    if (selectedFlight != undefined) {
        if (selectedFlight.flight_Id == id) {
            removePoly();
            selectedFlight = undefined;
        }
    }

    let mark = markersMap.get(id);
    map.removeControl(mark);
    let curr_url = '/api/Flights/' + id;
    $.ajax({
        url: curr_url,
        type: 'DELETE',
        success: function () {
            succededToSend = true;
        },
        Error: function () {
            if (succededToSend) {
                alert("Messege could not be sent. Check your connection again");
            }
            succededToSend = false;
        }
    });

}

//post external flights.
function sendPostFlights(files) {
    let curr_url = '/api/Flights/addInternal';
    $.ajax({
        url: curr_url,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: files,
        success: function (data) {
            succededToSend = true;
            getInternalFlights();
        },
        Error: function () {
            if (succededToSend) {
                alert("Messege could not be sent. Check your connection again");
            }
            succededToSend = false;
        }
    });
}