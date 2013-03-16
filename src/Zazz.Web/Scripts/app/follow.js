$(function () {
    
    /* Follow or Unfollow */
    $(document).on('click', '.btn-follow', function () {
        var btn = $(this);
        var action = btn.data('action');
        var originalBtnText = showBtnBusy(btn);

        var url = btn.data('url');

        var options = {
            url: url,
            cache: false,
            error: function () {
                toastr.error("An error occured, Please try again later.");
            }
        };


    });


    /* Accept or Reject */
    
    $(document).on('click', '.btn-followrequest-action', function () {
        var btn = $(this);
        var originalBtnText = showBtnBusy(btn);

        var url = btn.data('url');

        var options = {
            url: url,
            cache: false,
            error: function () {
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