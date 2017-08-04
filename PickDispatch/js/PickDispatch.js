
/* ==========UI CONTROLS========== */
var mnuActualizar = $('#mnuActualizar');
var mnuNuevo = $('#mnuNuevo');
var mnuGuardar = $('#mnuGuardar');
var mnuCancelar = $('#mnuCancelar');
var mnuPrint = $('#mnuPrint');
var mnuUploadFile = $('#mnuUploadFile');
var mnuUploadPic = $('#mnuUploadPic');
var ddlOpcion = $('#ddlOpcion');
var tbInventario = $('#tbInventario');
var tbSalidas = $('#tbSalidas');
var divLabel = $('#divLabel');
var ibox1 = $('#ibox1');
var iboxInbound = $('#iboxInbound');
var iboxEvents = $('#iboxEvents');
var iboxOutbound = $('#iboxOutbound');
var iboxDetail = $('#iboxDetail');
var iboxPictures = $('#iboxPictures');
var iboxFiles = $('#iboxFiles');
var txtCaja = $('#txtCaja');
var txtTranspotista = $('#txtTranspotista');
var ddlEquipment = $('#ddlEquipment');
var txtTractor = $('#txtTractor');
var txtTransfer = $('#txtTransfer');
var txtOperador = $('#txtOperador');
var ddlStatus = $('#ddlStatus');
var txtSello = $('#txtSello');
var ddlLocation = $('#ddlLocation');
var txtObservaciones = $('#txtObservaciones');
var ddlCustomers = $('#ddlCustomers');
var btCustomers = $('#btCustomers');
var pickGrid = $("#pickGrid");
var selectedGrid = $("#selectedGrid");
var btRemove = $('#btRemove');
var btSchedule = $('#btSchedule');
var btSelect = $('#btSelect');
var txtCustomer = $('#txtCustomer');
var txtConsignee = $('#txtConsignee');
var txtBOL = $('#txtBOL');
var txtPackages = $('#txtPackages');
var txtGrossWeight = $('#txtGrossWeight');
var txtCustomerOrder = $('#txtCustomerOrder');
var txtConsigneeAddress = $('#txtConsigneeAddress');
var txtPickNo = $('#txtPickNo');
var txtDispatchNo = $('#txtDispatchNo');
var txtCarrierName = $('#txtCarrierName');
var txtRemarks = $('#txtRemarks');
var ddlFreightParty = $('#ddlFreightParty');
var ddlNotifyParty = $('#ddlNotifyParty');
var txtTrackingNo = $('#txtTrackingNo');
var txtSealNo = $('#txtSealNo');
var txtDriverName = $('#txtDriverName');
var txtLoadNo = $('#txtLoadNo');
var txtSearchInv = $('#txtSearchInv');
var txtSearchSel = $('#txtSearchSel');
var txtTraffic = $('#txtTraffic');
var txtOrderNo = $('#txtOrderNo');

/* ==========EVENTS========== */
$('.fa-flag-o').click(function () {
    if ($(this).hasClass("text-danger")) {
        $(this).addClass('fa-flag-o').removeClass('fa-flag').removeClass('text-danger');
    }
    else {
        $(this).removeClass('fa-flag-o').addClass('fa-flag').addClass('text-danger');
    }
    
});

//$('.fa-flag').click(function () {
//    $(this).removeClass('fa-flag').addClass('fa-flag-o').removeClass('text-danger');
//});

mnuActualizar.click(function () {
    //mnuActualizar.find('i').toggleClass('fa-spin').attr('disabled', true);
    mnuActualizar.attr('disabled', true).find('i').addClass('fa-spin');
    $('#ibox1').children('.ibox-content').toggleClass('sk-loading');

    setTimeout(function () {
        mnuActualizar.attr('disabled', false).find('i').removeClass('fa-spin');
        $('#ibox1').children('.ibox-content').toggleClass('sk-loading');
    }, 300)

});

ddlOpcion.change(function () {
    mnuActualizar.attr('disabled', true).find('i').addClass('fa-spin');
    $('#ibox1').children('.ibox-content').toggleClass('sk-loading');

    if (ddlOpcion.val() == 1) {
        tbInventario.removeClass('hidden'); tbSalidas.addClass('hidden');
    }
    else if (ddlOpcion.val() == 3) {
        tbInventario.addClass('hidden'); tbSalidas.removeClass('hidden');
    }
   
    setTimeout(function () {
        mnuActualizar.attr('disabled', false).find('i').removeClass('fa-spin');
        $('#ibox1').children('.ibox-content').toggleClass('sk-loading');
    }, 300)
});

mnuNuevo.click(function () {
    iboxInbound.find('input').val(''); txtObservaciones.val('');
    iboxInbound.find('input').attr('disabled', false); iboxInbound.find('select,textarea').attr('disabled', false);
    mnuNuevo.addClass('hidden');
    ibox1.addClass('hidden');
    divLabel.html('CHECK IN');
    mnuCancelar.removeClass('hidden');
    iboxInbound.removeClass('hidden');
    //iboxEvents.removeClass('hidden');
    iboxDetail.removeClass('hidden');
    //iboxPictures.removeClass('hidden');
    mnuGuardar.removeClass('hidden');
    mnuPrint.removeClass('hidden');
    txtFechaEntrada.val(FormatoFecha(new Date));
    txtCaja.focus();
})

mnuCancelar.click(function () {
    //mnuActualizar.find('i').toggleClass('fa-spin').attr('disabled', true);
    mnuCancelar.addClass('hidden');
    iboxInbound.addClass('hidden');
    iboxEvents.addClass('hidden');
    iboxDetail.addClass('hidden');
    iboxOutbound.addClass('hidden');
    iboxPictures.addClass('hidden');
    iboxFiles.addClass('hidden');
    mnuGuardar.addClass('hidden');
    mnuPrint.addClass('hidden');
    divLabel.html('WMS Dashboard - Receipts');
    mnuNuevo.removeClass('hidden');
    ibox1.removeClass('hidden');
    iboxOutbound.find('div.ibox-content').slideDown();
})

EditarEntrada = function (entradaId) {
    iboxInbound.find('input').attr('disabled', false); iboxInbound.find('select,textarea').attr('disabled', false);
    mnuNuevo.addClass('hidden');
    ibox1.addClass('hidden');
    divLabel.html('DETAILS | EDIT - IIF1700001');
    mnuCancelar.removeClass('hidden');
    iboxInbound.removeClass('hidden');
    iboxEvents.removeClass('hidden');
    iboxDetail.removeClass('hidden');
    iboxPictures.removeClass('hidden');
    iboxOutbound.removeClass('hidden');
    //iboxOutbound.find('div.ibox-content').slideUp();
    //iboxOutbound.find('.collapse-link').click();
    iboxFiles.removeClass('hidden');
    mnuGuardar.removeClass('hidden');
    mnuPrint.removeClass('hidden');
    txtCaja.val('510054'); txtTranspotista.val('FEMA'); txtTractor.val('23'); txtTransfer.val('FEMA');
    txtOperador.val('JOHN LENNON'); txtSello.val('CE726635'); txtObservaciones.val('SAMPLE');
}

DarSalida = function (entradaId) {
    iboxInbound.find('input').attr('disabled', true); iboxInbound.find('select,textarea').attr('disabled', true);
    mnuNuevo.addClass('hidden');
    ibox1.addClass('hidden');
    divLabel.html('Departure');
    mnuCancelar.removeClass('hidden');
    iboxInbound.removeClass('hidden');
    iboxEvents.removeClass('hidden');
    iboxDetail.removeClass('hidden');
    iboxPictures.removeClass('hidden');
    iboxOutbound.removeClass('hidden');
    //iboxOutbound.find('div.ibox-content').slideUp();
    //iboxOutbound.find('.collapse-link').click();
    iboxFiles.removeClass('hidden');
    mnuGuardar.removeClass('hidden');
    mnuPrint.removeClass('hidden');
    txtCaja.val('510054'); txtTranspotista.val('FEMA'); txtTractor.val('23'); txtTransfer.val('FEMA');
    txtOperador.val('JOHN LENNON'); txtSello.val('CE726635'); txtObservaciones.val('SAMPLE');
}

EditarSalida = function (entradaId) {
    iboxInbound.find('input').attr('disabled', false); iboxInbound.find('select,textarea').attr('disabled', false);
    mnuNuevo.addClass('hidden');
    ibox1.addClass('hidden');
    divLabel.html('DETAILS | EDIT - YID1700009');
    mnuCancelar.removeClass('hidden');
    iboxInbound.removeClass('hidden');
    iboxEvents.removeClass('hidden');
    iboxDetail.removeClass('hidden');
    iboxPictures.removeClass('hidden');
    iboxOutbound.removeClass('hidden');
    iboxOutbound.find('div.ibox-content').slideDown();
    //iboxOutbound.find('.collapse-link').click();
    iboxFiles.removeClass('hidden');
    mnuGuardar.removeClass('hidden');
    mnuPrint.removeClass('hidden');
    txtCaja.val('510054'); txtTranspotista.val('FEMA'); txtTractor.val('23'); txtTransfer.val('FEMA');
    txtOperador.val('JOHN LENNON'); txtSello.val('CE726635'); txtObservaciones.val('SAMPLE');
}

FormatoFecha = function (date) {
    var dd = date.getDate();
    var MM = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    dd = (dd < 10 ? "0" : "") + dd;
    MM = (MM < 10 ? "0" : "") + MM;
    return MM + '/' + dd + '/' + yyyy;
}

FormatoHora = function (date) {
    var hh = date.getHours();
    var mm = date.getMinutes();
    hh = (hh < 10 ? "0" : "") + hh;
    mm = (mm < 10 ? "0" : "") + mm;
    return hh + ":" + mm + ":00";
}

AbrirPdf = function (entradaId) {
    var path = 'file:///C:/Users/AMGF/Desktop/YMS/Bol.pdf';
    //var path = 'http://apps6.laser.com.mx/Logistica/BOL/2992014102021.pdf';
    window.open(path, '_blank');
}

mnuUploadFile.click(function () {
    $('#file').click(); return false
});
mnuUploadPic.click(function () {
    $('#file').click(); return false
});
$("input:file").change(function () {
    $('#openModal').click();
    //var fileName = $(this).val();
    //$(".filename").html(fileName);
});
mnuGuardar.click(function () {
    iboxInbound.removeClass('hidden');
    //iboxFiles.addClass('hidden');
    //iboxPictures.addClass('hidden');
    $('#mnuGuardar').html('<i class="fa fa-floppy-o"></i>&nbsp;&nbsp;<span class="bold">Create</span>');
    $('#mnuCancelar').removeClass('hidden');
});

MoverEntrada = function (entradaId) {
    $('#openModal3').click();
}
SubirFotos = function (entradaId) {
    $('#file').click(); return false
}

/* ==========ON START========== */

$.ajax('pickdispatchservice.asmx/GetCustomers', {
    async: false,
    contentType: 'application/json',
    type: 'POST',
    success: function (data) {
        var id = jQuery.parseJSON(data.d)[0].group_id;
        var name = jQuery.parseJSON(data.d)[0].name;
        ddlCustomers.append('<li data=' + id + '><a href="#">' + name + '</a></li>');
    }
});

ddlCustomers.click(function (e) {
    btCustomers[0].innerText = e.target.innerText;
    CreateGrid('GetInventoryFromCustomer');
    btSelect.removeClass('hidden');
})



CreateGrid = function (method) {

    var fieldsPick = [
        { name: 'LPID', type: 'string' },
        { name: 'ReceiptNo', type: 'string' },
        { name: 'Date', type: 'string' },
        { name: 'Equipment', type: 'string' },
        { name: 'OrderNo', type: 'string' },
        { name: 'InvoiceNo', type: 'string' },
        { name: 'BLNo', type: 'string' },
        { name: 'GrossWeight', type: 'string' }
    ]

    var columnsPick = [
          { text: 'LP ID', datafield: 'LPID', width: "10%", editable: false },
          { text: 'Receipt No', datafield: 'ReceiptNo', width: "14%", editable: false },
          { text: 'Date', datafield: 'Date', width: "14%", editable: false},
          { text: 'Equipment', datafield: 'Equipment', width: "10%", editable: false },
          { text: 'Order No', datafield: 'OrderNo', width: "14%", editable: false},
          { text: 'Invoice No', datafield: 'InvoiceNo', width: "14%", editable: false },
          { text: 'BL No', datafield: 'BLNo', width: "14%", editable: false },
          { text: 'Weight', datafield: 'GrossWeight', width: "10%", editable: false }
    ]

    var source = {
        datatype: "json",
        datafields: fieldsPick,
        id: 'id',
        localdata: LoadEntries(method),
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

    pickGrid.jqxGrid({
        width: "100%",
        height: "50%",        
        source: dataAdapter,
        editable: true,
        sortable: true,
        selectionmode: 'singlerow',
        columns: columnsPick,
        enabletooltips: true
    });

    selectedGrid.jqxGrid({
        width: "100%",
        height: "50%",
        source: selectedSource,
        editable: true,
        sortable: true,
        selectionmode: 'singlerow',
        columns: columnsPick,
        enabletooltips: true
    });

    pickGrid.jqxGrid('localizestrings', localizationobj);
}


WSJson = function (url) {
    return $.ajax(url, {
        async: false,
        data: JSON.stringify({ 'customer_id': "1" }),
        contentType: 'application/json',
        type: 'POST'
    }).responseJSON.d;
}

/* Webservice call to load employees table*/
LoadEntries = function (method) {
    return WSJson('pickdispatchservice.asmx/' + method);
}

btSchedule.click(function () {
    var selectedrowindex = pickGrid.jqxGrid('getselectedrowindex');
    if (selectedrowindex != undefined) {
        var selectedRowData = pickGrid.jqxGrid('getrowdata', selectedrowindex);
        selectedGrid.jqxGrid('addrow', null, selectedRowData);
        //var id = pickGrid.jqxGrid('getrowid', selectedrowindex);
        //pickGrid.jqxGrid('deleterow', id);
    }
})

btRemove.click(function () {
    var selectedrowindex = selectedGrid.jqxGrid('getselectedrowindex');
    if (selectedrowindex != undefined) {
        var selectedRowData = selectedGrid.jqxGrid('getrowdata', selectedrowindex);
        pickGrid.jqxGrid('addrow', null, selectedRowData);
        var id = selectedGrid.jqxGrid('getrowid', selectedrowindex);
        selectedGrid.jqxGrid('deleterow', id);
    }
})

btSelect.click(function () {
    if (!iboxInbound.hasClass('hidden')) {
        var cAddress1 = txtConsigneeAddress.val().split(',')[0];
        var cAddress2 = txtConsigneeAddress.val().split(',')[1];
        if (cAddress2 == undefined) {
            cAddress2 = "";
        }
        $.ajax('pickdispatchservice.asmx/ProcessPickDispatch', {
            async: false,
            data: JSON.stringify({
                'carrierName': txtCarrierName.val(),
                'consigneeId': txtConsignee.data("id"),
                'bol': txtBOL.val(),
                'remarks': txtRemarks.val(),
                'customerOrders': txtCustomerOrder.val(),
                'packages': txtPackages.val(),
                'notifyParty': ddlNotifyParty.find(':selected').val(),
                'trackingNo': txtTrackingNo.val(),
                'dispatchedBy': '3733',
                'consigneeAddress': cAddress1,
                'consigneeAddress2': cAddress2,
                'freightParty': ddlFreightParty.find(':selected').val(),
                'seal': txtSealNo.val(),
                'grossWeight': txtGrossWeight.val(),
                'driverName': txtDriverName.val(),
                'loadNo': txtLoadNo.val(),
                'customerId': txtCustomer.data("id"),
                'trafficNo': txtTraffic.val(),
                'orderNo': txtOrderNo.val(),
                'notifyPartyName': ddlNotifyParty.find(':selected').text()
            }),
            contentType: 'application/json',
            type: 'POST',
            success: function (data) {
                var pickNo = jQuery.parseJSON(data.d)[0].PickNo;
                var dispatchNo = jQuery.parseJSON(data.d)[0].DispatchNo;
                var pickId = jQuery.parseJSON(data.d)[0].PickId;
                var dispatchId = jQuery.parseJSON(data.d)[0].DispatchId;

                txtPickNo.val(pickNo);
                txtDispatchNo.val(dispatchNo);

                var rows = selectedGrid.jqxGrid('getrows');
                var result = "";
                for (var i = 0; i < rows.length; i++) {
                    var row = rows[i];
                    var currentLP = row.LPID;

                    $.ajax('pickdispatchservice.asmx/UpdateLPN', {
                        async: false,
                        data: JSON.stringify({ 'lp': currentLP, 'pickId': pickId, 'dispatchId': dispatchId }),
                        contentType: 'application/json',
                        type: 'POST',
                        success: function (data) {
                            //Nothing, continue with next
                        },
                        error:function(data){
                            alert('Error:' + data.d);
                        }
                    });
                }

                alert("Pick/Dispatch has been processed!");
                btSelect.attr('disabled', true);
            },
            error: function (data) {
                alert(data.d);
            }
        });
    }
    else {
        $.ajax('pickdispatchservice.asmx/GetNotifyParty', {
            async: false,
            contentType: 'application/json',
            type: 'POST',
            success: function (data) {
                var aux = jQuery.parseJSON(data.d);
                ddlNotifyParty.empty();
                for (var x = 0; x < aux.length; x++) {
                    ddlNotifyParty.append('<option selected="selected" value="' + aux[x].Value + '">' + aux[x].Name + '</option>');
                }
                
            },
            error: function (data) {
                alert('Error: ' + data.d);
            }
        });

        $.ajax('pickdispatchservice.asmx/GetFreightParty', {
            async: false,
            contentType: 'application/json',
            type: 'POST',
            success: function (data) {
                var aux = jQuery.parseJSON(data.d);
                ddlFreightParty.empty();
                for (var x = 0; x < aux.length; x++) {
                    ddlFreightParty.append('<option selected="selected" value="' + aux[x].Value + '">' + aux[x].Value + '-' + aux[x].Name + '</option>');
                }

            },
            error: function (data) {
                alert('Error: ' + data.d);
            }
        });

        $.ajax('pickdispatchservice.asmx/GetHeaderInfo', {
            async: true,
            data: JSON.stringify({ 'serial': GetSerialSelected() }),
            contentType: 'application/json',
            type: 'POST',
            success: function (data) {
                var customerName = jQuery.parseJSON(data.d)[0].CustomerName;
                var consigneeName = jQuery.parseJSON(data.d)[0].ConsigneeName;
                var consigneeId = jQuery.parseJSON(data.d)[0].ConsigneeID;
                var blNo = jQuery.parseJSON(data.d)[0].BLNo;
                var customerOrders = jQuery.parseJSON(data.d)[0].CustomerOrder;
                var consigneeAddress = jQuery.parseJSON(data.d)[0].ConsigneeAddress;
                var freightPartyId = jQuery.parseJSON(data.d)[0].FreightPartyId;
                var freightPartyName = jQuery.parseJSON(data.d)[0].FreightPartyName;
                var notifyPartyId = jQuery.parseJSON(data.d)[0].NotifyPartyId;
                var notifyPartyName = jQuery.parseJSON(data.d)[0].NotifyPartyName;
                var customerId = jQuery.parseJSON(data.d)[0].CustomerId;

                txtCustomer.val(customerName);
                txtCustomer.data("id", customerId);
                txtConsignee.val(consigneeName);
                txtConsignee.data("id", consigneeId);
                txtBOL.val(blNo);
                txtPackages.val(selectedGrid.jqxGrid('getrows').length);
                txtCustomerOrder.val(customerOrders);
                txtConsigneeAddress.val(consigneeAddress);

                //Get GrossWeight from selected LPs
                var selectedLPs = selectedGrid.jqxGrid('getrows');
                var sum = 0;
                for (var x = 0; x < selectedLPs.length; x++) {
                    sum += selectedLPs[x].GrossWeight;
                }
                txtGrossWeight.val(sum);

                ddlFreightParty.append('<option selected="selected" value="' + freightPartyId + '">' + freightPartyName + '</option>');
                

                iboxInbound.removeClass('hidden');
                btSelect.html('<i class="fa fa-floppy-o"></i>&nbsp;&nbsp;<span class="bold">Create</span>');
                $('#mnuCancelar').removeClass('hidden');


                //Desahabilitar los controles para agregar LP
                btSchedule.attr('disabled', true);
                btRemove.attr('disabled', true);
            }
        });
    }
})

GetSerialSelected = function () {
    var rows = selectedGrid.jqxGrid('getrows');
    if (rows.length > 0) {
        return rows[0].LPID;
    }
}

setGlobalFilterInv = function (filtervalue) {
    var columns = pickGrid.jqxGrid('columns');
    var filtergroup, filter;

    // clear filters and exit if filter expression is empty
    pickGrid.jqxGrid('clearfilters');
    if (filtervalue == null || filtervalue == '') {
        return;
    }

    // the filtervalue must be aplied to all columns individually,
    // the column filters are combined using "OR" operator
    for (var i = 0; i < columns.records.length; i++) {
        if (!columns.records[i].hidden && columns.records[i].filterable) {
            filtergroup = new $.jqx.filter();
            filtergroup.operator = 'or';
            filter = filtergroup.createfilter('stringfilter', filtervalue, 'contains');
            filtergroup.addfilter(1, filter);

            pickGrid.jqxGrid('addfilter', columns.records[i].datafield, filtergroup);
        }
    }
    pickGrid.jqxGrid('applyfilters');
}

setGlobalFilterSel = function (filtervalue) {
    var columns = selectedGrid.jqxGrid('columns');
    var filtergroup, filter;

    // clear filters and exit if filter expression is empty
    selectedGrid.jqxGrid('clearfilters');
    if (filtervalue == null || filtervalue == '') {
        return;
    }

    // the filtervalue must be aplied to all columns individually,
    // the column filters are combined using "OR" operator
    for (var i = 0; i < columns.records.length; i++) {
        if (!columns.records[i].hidden && columns.records[i].filterable) {
            filtergroup = new $.jqx.filter();
            filtergroup.operator = 'or';
            filter = filtergroup.createfilter('stringfilter', filtervalue, 'contains');
            filtergroup.addfilter(1, filter);

            selectedGrid.jqxGrid('addfilter', columns.records[i].datafield, filtergroup);
        }
    }
    selectedGrid.jqxGrid('applyfilters');
}

txtSearchInv.keyup(function () {
    setGlobalFilterInv(txtSearchInv.val());
});

txtSearchSel.keyup(function () {
    setGlobalFilterSel(txtSearchSel.val());
});

ddlFreightParty.change(function () {
    txtConsignee.val(ddlFreightParty.find(':selected').text());
})