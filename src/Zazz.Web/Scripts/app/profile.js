/********************************
    Follows You Sign
*********************************/
var isFollowSignClicked = false;
$('#followsYouSign').hover(
    function mouseIn() {

        if (isFollowSignClicked) {
            return;
        }

        var sign = $(this);

        sign.removeClass('label-success');
        sign.addClass('label-warning');
        sign.html('Stop follow  <i class="icon icon-remove-sign"></i>');

    }, function mouseOut() {

        if (isFollowSignClicked) {
            return;
        }

        var sign = $(this);

        sign.addClass('label-success');
        sign.removeClass('label-warning');
        sign.html('Follows you <i class="icon icon-ok-sign"></i>');
        
    });

$('#followsYouSign').click(function(e) {
    var sign = $(this);

    var userId = sign.data('id');

    isFollowSignClicked = true;
    sign.removeAttr('id');
    sign.css('opacity', 0.5);
    sign.html('Working <i class="icon icon-spinner icon-spin"></i>');

    $.ajax({
        url: '/follow/stopfollow/' + userId,
        cache: false,
        error: function() {
            toastr.error('An error occured. Please try again later.');
        },
        success: function() {
            toastr.success('This user is no longer following you!');
            sign.fadeOut();
        }
    });
});

/********************************
    Follow / Unfollow
*********************************/

// Try to not move the follow ajax calls into ajax.js, it becomes worse.

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


/********************************
    Accept / Reject Follow
*********************************/

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

/********************************
    Weeklies
*********************************/

// Closing the correct popover

function closeWeeklyPopover(weeklyId) {
    var popoverOwner = $('*[data-popover-holder="' + weeklyId + '"]');
    popoverOwner.popover('hide');
}

$(document).on('click', 'button[data-close-popover]', function() {

    var popoverId = $(this).attr('data-close-popover');
    closeWeeklyPopover(popoverId);

});

// Weekly photo select

var weeklyId;

function weeklyPhotoSelected(photoId, photoUrl) {
    var id = '#weekly-' + weeklyId;
    var thumbnailId = id + '-thumbnail';
    var idHolderId = id + '-photoId';
    
    var thumbnailElem = $(thumbnailId);
    var idHolderElem = $(idHolderId);

    thumbnailElem.attr('src', photoUrl);
    idHolderElem.val(photoId);
}

//saving
$(document).on('click', '.saveWeeklyBtn', function() {

    var self = $(this);
    var id = self.data('id');

    var url = id === 0 ? '/weekly/new' : '/weekly/edit';
    
    var form = self.closest('form');
    var formData = form.serialize();

    showBtnBusy(self);

    $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        error: function() {
            toastr.error('An error occured, Please try again.');
            hideBtnBusy(self, "Save");
        },
        success: function(res) {
            hideBtnBusy(self, "Save");
            var li = $(res.trim());
            var ul = $('.weeklies-items');
            li.appendTo(ul).hide().fadeIn();
        }
    });

});

// initializing
$(function() {

    var addWeekly = $('#add-weekly');
    if (addWeekly) {

        var addWeeklyContent = $('#add-weekly-content');

        addWeekly.popover({
            html: true,
            placement: 'top',
            title: 'New Weekly',
            content: addWeeklyContent.html()
        });

        addWeeklyContent.remove();
    }
});

