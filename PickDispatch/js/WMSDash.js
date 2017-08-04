
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

/* ==========EVENTS========== */
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
    $('#openModal2').click();
});

MoverEntrada = function (entradaId) {
    $('#openModal3').click();
}
SubirFotos = function (entradaId) {
    $('#file').click(); return false
}
