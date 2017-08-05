/* ==========UI CONTROLS========== */
var gridActiveShipments = $('#gridActiveShipments');
var gridDispatchedShipments = $('#gridDispatchedShipments');


/* ==========FUNCTIONS========== */
var renderBOL = function (row, columnfield, value, defaulthtml, columnproperties) {
    return '<span style="margin-left:10px" onmouseover="$(this).css(\'cursor\',\'pointer\')" onclick="alert(\'Open file from here...\');"><i class="fa fa-file"></i>' + value + '</span>';
}

var renderEdit = function (row, columnfield, value, defaulthtml, columnproperties) {
    return '<span style="margin-left:10px;margin-top:5px;margin-bottom:5px" onmouseover="$(this).css(\'cursor\',\'pointer\')"><i class="fa fa-pencil"></i>&nbsp;&nbsp;&nbsp;' + value + '</span>';
}

CreateActiveShipmentsGrid = function () {

    var fieldsPick = [
        { name: 'ID', type: 'string' },
        { name: 'Customer', type: 'string' },
        { name: 'Reference', type: 'string' },
        { name: 'Equipment(TU)', type: 'string' },
        { name: 'Carrier(TU)', type: 'string' },
        { name: 'Packages', type: 'string' },
        { name: 'ArrivalDate', type: 'string' },
        { name: 'Equipment(TL)', type: 'string' },
        { name: 'Carrier(TL)', type: 'string' },
        { name: 'CheckIn', type: 'string' },
        { name: 'Status', type: 'string' },
        { name: 'BOL', type: 'string' }
    ]

    var columnsPick = [
          { text: 'Shipment ID', datafield: 'ID', width: "8%", editable: false, cellsrenderer: renderEdit },
          { text: 'Customer', datafield: 'Customer', width: "10%", editable: false },
          { text: 'Reference', datafield: 'Reference', width: "10%", editable: false },
          { text: 'Equipment(TU)', datafield: 'Equipment(TU)', width: "8%", editable: false },
          { text: 'Carrier(TU)', datafield: 'Carrier(TU)', width: "8%", editable: false },
          { text: 'Packages', datafield: 'Packages', width: "8%", editable: false },
          { text: 'ArrivalDate', datafield: 'ArrivalDate', width: "8%", editable: false },
          { text: 'Equipment(TL)', datafield: 'Equipment(TL)', width: "8%", editable: false },
          { text: 'Carrier(TL)', datafield: 'Carrier(TL)', width: "8%", editable: false },
          { text: 'CheckIn', datafield: 'CheckIn', width: "8%", editable: false },
          { text: 'Status', datafield: 'Status', width: "8%", editable: false },
          { text: 'BOL', width: "8%", editable: false, cellsrenderer: renderBOL }
    ]

    var source = {
        datatype: "json",
        datafields: fieldsPick,
        id: 'id',
        localdata: LoadEntries("GetActiveShipments"),
        addrow: function (rowid, rowdata, position, commit) {
            commit(true);
        },
        deleterow: function (rowid, commit) {
            commit(true);
        }
    };

    var selectedSource = {
        datatype: "json",
        datafields: fieldsPick,
        id: 'id',
        addrow: function (rowid, rowdata, position, commit) {
            commit(true);
        },
        deleterow: function (rowid, commit) {
            commit(true);
        }
    };

    var dataAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data, status, xhr) { },
        loadComplete: function (data) { },
        loadError: function (xhr, status, error) { }
    });

    var localizationobj = {};
    localizationobj.emptydatastring = "No matched records";

    gridActiveShipments.jqxGrid({
        width: "100%",
        height: "300px",
        source: dataAdapter,
        editable: true,
        sortable: true,
        //selectionmode: 'singlerow',
        columns: columnsPick,
        enabletooltips: true
    });

    //pickGrid.jqxGrid('localizestrings', localizationobj);
}

CreateDispatchedShipmentsGrid = function () {

    var fieldsPick = [
        { name: 'ID', type: 'string' },
        { name: 'Customer', type: 'string' },
        { name: 'Reference', type: 'string' },
        { name: 'Equipment(TU)', type: 'string' },
        { name: 'Carrier(TU)', type: 'string' },
        { name: 'Packages', type: 'string' },
        { name: 'ArrivalDate', type: 'string' },
        { name: 'Equipment(TL)', type: 'string' },
        { name: 'Carrier(TL)', type: 'string' },
        { name: 'CheckIn', type: 'string' },
        { name: 'LoadingTime', type: 'string' },
        { name: 'CheckOut', type: 'string' },
        { name: 'SealNumber', type: 'string' }
    ]

    var columnsPick = [
          { text: 'Shipment ID', datafield: 'ID', width: "10%", editable: false },
          { text: 'Customer', datafield: 'Customer', width: "10%", editable: false },
          { text: 'Reference', datafield: 'Reference', width: "10%", editable: false },
          { text: 'Equipment(TU)', datafield: 'Equipment(TU)', width: "7%", editable: false },
          { text: 'Carrier(TU)', datafield: 'Carrier(TU)', width: "7%", editable: false },
          { text: 'Packages', datafield: 'Packages', width: "7%", editable: false },
          { text: 'ArrivalDate', datafield: 'ArrivalDate', width: "7%", editable: false },
          { text: 'Equipment(TL)', datafield: 'Equipment(TL)', width: "7%", editable: false },
          { text: 'Carrier(TL)', datafield: 'Carrier(TL)', width: "7%", editable: false },
          { text: 'CheckIn', datafield: 'CheckIn', width: "7%", editable: false },
          { text: 'LoadingTime', datafield: 'LoadingTime', width: "7%", editable: false },
          { text: 'CheckOut', datafield: 'CheckOut', width: "7%", editable: false },
          { text: 'SealNumber', datafield: 'SealNumber', width: "7%", editable: false }
    ]

    var source = {
        datatype: "json",
        datafields: fieldsPick,
        id: 'id',
        localdata: LoadEntries("GetDispatchedShipments"),
        addrow: function (rowid, rowdata, position, commit) {
            commit(true);
        },
        deleterow: function (rowid, commit) {
            commit(true);
        }
    };

    var selectedSource = {
        datatype: "json",
        datafields: fieldsPick,
        id: 'id',
        addrow: function (rowid, rowdata, position, commit) {
            commit(true);
        },
        deleterow: function (rowid, commit) {
            commit(true);
        }
    };

    var dataAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data, status, xhr) { },
        loadComplete: function (data) { },
        loadError: function (xhr, status, error) { }
    });

    var localizationobj = {};
    localizationobj.emptydatastring = "No matched records";

    gridDispatchedShipments.jqxGrid({
        width: "100%",
        height: "300px",
        source: dataAdapter,
        editable: true,
        sortable: true,
        //selectionmode: 'singlerow',
        columns: columnsPick,
        enabletooltips: true
    });

    //pickGrid.jqxGrid('localizestrings', localizationobj);
}

WSJson = function (url) {
    return $.ajax(url, {
        async: false,
        //data: JSON.stringify({ 'customer_id': "1" }),
        contentType: 'application/json',
        type: 'POST'
    }).responseJSON.d;
}

/* Webservice call to load employees table*/
LoadEntries = function (method) {
    return WSJson('webservice.asmx/' + method);
}


/* ==========ON START========== */
CreateActiveShipmentsGrid();
CreateDispatchedShipmentsGrid();