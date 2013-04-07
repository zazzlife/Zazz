// Try to not move the follow ajax calls into ajax.js, it becomes worse.

$(function () {
    
    /* Follow or Unfollow */
    $(document).on('click', '.btn-follow', function () {
        var btn = $(this);
        var action = btn.data('action');
        var originalBtnText = showBtnBusy(btn);
        var id = btn.data('id');

        var url = btn.data('url');

        var options = {
            url: url,
            cache: false,
            error: function () {
                toastr.error("An error occured, Please try again later.");
            }
        };

        $.ajax(options).done(function() {

            var btnContainer = $('#followRequestBtnContainer');
            var btn;
            
            if (action == "followRequest") {
                btn = '<button title="Follow request has been sent before. You must wait for the user to accept your request." class="btn btn-large pull-right btn-info disabled" disabled="disabled">Follow Request Sent</button>';
            } else if (action == "follow") {
                btn = '<button data-id="' + id + '" data-url="/follow/unfollow/' + id + '" data-action="unfollow" class="btn btn-large pull-right btn-info btn-follow">Unfollow</button>';
            } else if (action == "unfollow") {
                var isClub = $('#isClub').val().toLowerCase() === 'true';
                if (isClub) {
                    btn = '<button data-id="' + id + '" data-url="/follow/followuser/' + id + '" data-action="follow" class="btn btn-large pull-right btn-info btn-follow">Follow</button>';
                } else {
                    btn = '<button data-id=' + id + '" data-url="/follow/followuser/' + id + '" data-action="followRequest" class="btn btn-large pull-right btn-info btn-follow">Send Follow Request</button>';
                }
            }

            btnContainer.html(btn);

        });
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