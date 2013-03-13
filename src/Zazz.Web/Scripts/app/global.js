function showBtnBusy(btn) {
    var originalText = $(btn).html();

    var textWithSpinner = '<i style="margin-right:5px;" class="icon-refresh icon-spin"></i>' + originalText;

    $(btn).attr('disabled', 'disabled');
    $(btn).addClass('disabled');
    $(btn).html(textWithSpinner);

    return originalText;
}

function hideBtnBusy(btn, originalText) {
    $(btn).removeAttr('disabled', 'disabled');
    $(btn).removeClass('disabled');
    $(btn).html(originalText);
}

$(function () {
    
    $('.datepicker').datetimepicker();

    $('*[title]').tooltip();


    $('.btn-followrequest-action').click(function () {
        var btn = $(this);
        var originalBtnText = showBtnBusy(btn);

        var url = btn.data('url');

        var options = {
            url: url,
            cache: false,
            error: function() {
                toastr.error("An error occured, Please try again later.");
            }
        };

        $.ajax(options).done(function () {
            btn.closest('tr').fadeOut('fast');

            var followReqCountElem = $('#follow-request-count');
            var followReqCount = parseInt(followReqCountElem.text());

            followReqCount--;

            followReqCountElem.text(followReqCount);

            if (followReqCount < 1) {
                followReqCountElem.fadeOut();
            }
                
        });

    });
})