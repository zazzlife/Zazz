function showBtnBusy(btn) {
    var originalText = $(btn).html();

    var textWithSpinner = '<i class="icon-refresh icon-spin"></i>' + originalText;

    $(btn).attr('disabled', 'disabled');
    $(btn).addClass('disabled');
    $(btn).html(textWithSpinner);

    return originalText;
}

$(function () {
    
    $('.datepicker').datetimepicker();

})