var toHiglightRow; //row to highlight.
var selectedFlight; //flight that has been clicked on.
var rowsId = []; // the id of all rows.

//add cells to given row.
function addRow(row, plane, isInternal) {
    
    let j = 0; let cell = row.insertCell(0);
    cell.innerHTML = plane.flight_Id;
    cell.setAttribute("headers", "flight_Id");
    cell = row.insertCell(++j);
    cell.innerHTML = plane.company_Name;
    cell = row.insertCell(++j);
    cell.innerHTML = "(" + plane.longitude.toFixed(2) + ", " + plane.latitude.toFixed(2) + ")";
    cell.setAttribute("headers", "lanlat");
    cell = row.insertCell(++j);
    cell.innerHTML = plane.passengers;
    if (!isInternal) {
        return;
    }
    let id = row.id; let RId = "deleteFlightButton-" + plane.flight_Id;
    $("#" + id).append("<td class=\"button\">  <Button class=\"X_Button\" id=\"" + RId + "\"" + ">X</Button> </td>  ");
    $("#" + RId).on('click', function () {
        deleteFlight(plane.flight_Id);
    });
};


//function check if row already exists. 
//if not, adds it to rows list and returns false so we'll add it to table.
//if it does, updates the location.
function isExist(rowId, plane, livingRows) {
    var element = document.getElementById(rowId);
    let isExist = false;
    livingRows.push(rowId);
    if (!rowsId.includes(rowId)) {
        rowsId.push(rowId);
    }
    if (element) {
        isExist = true;
        element = element.cells;
        element[2].innerHTML = "(" + plane.longitude.toFixed(2) + ", " + plane.latitude.toFixed(2) + ")";
    }
    return isExist;
}
//add rows to table
function addRows(planes) {
    let i; let row; let rowId; let table; let livingRows = [];
    let isInternal = false;
    let externalTable = document.getElementById("external-filghts-table"); let internalTable = document.getElementById("internal-filghts-table");
    for (i = 0; i < planes.length; i++) {
        let plane = planes[i];
        let id = plane.flight_Id;
        if (plane.isExternal == true) {
            table = externalTable;
            rowId = "external-row-" + id;
        } else {
            isInternal = true;
            table = internalTable;
            rowId = "internal-row-" + id;
        }
        
        if (isExist(rowId, plane, livingRows)) {
            continue;
        }
        row = table.insertRow(table.rows.length);
        row.setAttribute("id", rowId);
        addRow(row, plane, isInternal);
        $("#" + rowId).click(function () {
            clickOnRow(plane);
        });
        handleSelectedFlight(plane);
    }
    checkForDeadRows(livingRows);
};
//deletes dead planes rows.
function checkForDeadRows(livingRows) {

    let i = 0; let size = rowsId.length;
    for (i = 0; i < size; i++) {
        if (!livingRows.includes(rowsId[i])) {
            deleteRow(rowsId[i]);
        }
    }
}
//delete given a row, given its id.
function deleteRow(rowid) {
    //$("#" + rowid).remove();
    var row = document.getElementById(rowid);
    if (row == undefined) {
        return;
    }
    //row.remove()
    row.parentNode.removeChild(row);
    let index = rowsId.indexOf(rowid);
    rowsId.slice(index, index);
}
//handle selected plane, given its already selected.
function handleSelectedFlight(flight) {
    if (selectedFlight == undefined) {
        return;
    }
    if (selectedFlight.flight_Id == flight.flight_Id) {
        selectedFlight = flight;
    }
    detailTable();
}
//highlight row, given its flight plan.
function highlightRow(plane, flight) {
    
    selectedFlight = flight;
    let id = flight.flight_Id;
    if (toHiglightRow != undefined) {
        toHiglightRow.className = "";
        toHiglightRow = undefined;
    }
    let cName = "external-row-" + id;
    if (flight.isExternal == false) {
        cName = "internal-row-" + id;
    }
    toHiglightRow = document.getElementById(cName);
    if (toHiglightRow == undefined) {
        return;
    }
    toHiglightRow.className = "highlightedRow";
    toHiglightRow.scrollIntoView();
    return toHiglightRow;
}
//unhighlight row.
function unHighlightRow() {
    if (selectedFlight == undefined) {
        return;
    }
    let id = selectedFlight.flight_Id;
    let cName = "external-row-" + id;
    if (selectedFlight.isExternal == false) {
        cName = "internal-row-" + id;
    }
    toUnHiglightRow = document.getElementById(cName);
    if (toUnHiglightRow != undefined) {
        toUnHiglightRow.className = "";
    }
    selectedFlight = undefined;
}
//action when clicked on row.
function clickOnRow(plane) {
    console.log(plane.flight_Id);
    getFlightPlan(plane);
    let icc = markersMap.get(plane.flight_Id);
    icc.openPopup();
}

//handles the detail table.
function detailTable() {
    let table = document.getElementById("details-filghts-table");
    let flight = selectedFlight;
    if ((selectedFlight == undefined) && (table.rows.length > 1)) {
        table.deleteRow(1);
        return;
    }
    if (selectedFlight == undefined) {
        return;
    }
    let id = flight.flight_Id;

    if (table == undefined) {
        return;
    }
    if (table.rows.length > 1) {
        table.deleteRow(1);
    }
    let r = table.insertRow(table.rows.length);
    cell = r.insertCell(0);
    cell.innerHTML = flight.latitude.toFixed(2);
    cell = r.insertCell(0);
    cell.innerHTML = flight.longitude.toFixed(2);
    cell = r.insertCell(0);
    cell.innerHTML = flight.passengers;
    cell = r.insertCell(0);
    cell.innerHTML = flight.company_Name;
    cell = r.insertCell(0);
    cell.innerHTML = id;
}

function removeHighlightExternalRow() {

    toHiglightRow.className = "";
    return toHiglightRow;
}
//starting function, that defines the functionallity of the drop zone.
(function () {
    "use strict";
    let dropZone = document.getElementById('drop-zone'); let externalFlights = document.getElementById('internallFlights');
    let externalClass = 'well externalFlights'; let hidden = 'hidden';
    var startUpload = function (files) {
        let file = files[0]; let fr = new FileReader();
        fr.onload = function () {
            sendPostFlights(fr.result);
        }
        if (file.type == "application/json") {
            fr.readAsText(file);
        }
    };
    externalFlights.ondragover = function () {
        document.getElementById('internalFlightsText').className = hidden;
        dropZone.className = 'upload-console-drop';
        return false;
    };

    externalFlights.ondragleave = function () {
        return false;
    };
    //drop functionallity
    dropZone.ondrop = function (e) {
        e.preventDefault();
        //this.className = 'upload-console-drop';
        document.getElementById('internalFlightsText').className = 'well inner';
        dropZone.className = hidden;
        startUpload(e.dataTransfer.files);

    };
    dropZone.ondragover = function () {
        this.className = 'upload-console-drop drop';
        return false;
    };
    dropZone.ondragleave = function () {
        this.className = 'upload-console-drop';
        dropZone.className = hidden;
        document.getElementById('internalFlightsText').className = 'well inner';
        return false;
    };

}());

