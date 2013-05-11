// Try to not move the follow ajax calls into ajax.js, it becomes worse.

/* Follow or Unfollow */
$(document).on('click', '.btn-follow', function () {
    var self = $(this);
    var action = self.data('action');
    var originalBtnText = showBtnBusy(self, true);
    var id = self.data('id');

    var url = self.data('url');

    var options = {
        url: url,
        cache: false,
        error: function () {
            toastr.error("An error occured, Please try again later.");
        }
    };

    $.ajax(options).done(function () {

        if (action == "follow") {
            self.attr('title', 'Follow request has been sent.');
            setBtnDisabled(self);
            self.html("<i class='icon-ok'></i>");
        } else if (action == "unfollow") {
            hideBtnBusy(self, "<i class='icon-plus'></i>");
            self.attr('url', 'data-url="/follow/followuser/');
            self.attr("action", "follow");
        }
        
        applyPageStyles();
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