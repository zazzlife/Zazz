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

$(document).on('click', '*[data-btn-follow]', function () {
    var self = $(this);
    var icon = self.find('i');
    var action = self.data('action');
    var id = self.data('id');
    var url = self.data('url');

    setBtnDisabled(self);
    icon.removeClass();
    icon.addClass('icon-spin');
    icon.addClass('icon-refresh');

    var options = {
        url: url,
        cache: false,
        error: function () {
            toastr.error("An error occured, Please try again later.");
        }
    };

    $.ajax(options).done(function () {

        //if (action == "follow") {
        //    self.attr('title', 'Follow request has been sent.');
        //    setBtnDisabled(self);
        //    self.html("<i class='icon-ok'></i>");
        //} else if (action == "unfollow") {
        //    hideBtnBusy(self, "<i class='icon-plus'></i>");
        //    self.attr('data-url', '/follow/followuser/');
        //    self.attr("data-action", "follow");
        //}

        
        icon.removeClass();
        icon.addClass('icon-ok');
        applyPageStyles();
    });
});


/********************************
    Accept / Reject Follow
*********************************/

//$(document).on('click', '.btn-followrequest-action', function () {
//    var btn = $(this);
//    var originalBtnText = showBtnBusy(btn);
//
//    var url = btn.data('url');
//
//    var options = {
//        url: url,
//        cache: false,
//        error: function () {
//            toastr.error("An error occured, Please try again later.");
//        }
//    };
//
//    $.ajax(options).done(function () {
//        btn.closest('tr').fadeOut('fast');
//
//        var followReqCountElem = $('#follow-request-count');
//        var followReqCount = parseInt(followReqCountElem.text());
//
//        followReqCount--;
//
//        followReqCountElem.text(followReqCount);
//
//        if (followReqCount < 1) {
//            followReqCountElem.fadeOut();
//        }
//
//    });
//
//});

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

// Create / Edit
$(document).on('click', '.saveWeeklyBtn', function() {

    var self = $(this);
    var id = self.data('id');

    var action = id === 0 ? 'new' : 'edit';
    var url = action === 'new' ? '/weekly/new' : '/weekly/edit';
    
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
            closeWeeklyPopover(id);

            var li = $(res.trim());
            var ul = $('.weeklies-items');
            
            if (action === 'new') {
                li.appendTo(ul).hide().fadeIn();
                
                li.find('.weekly-description').popover();
                createWeeklyEditable(li);
            } else {
                var oldItem = $('.weekly[data-id="' + id + '"]');
                oldItem.popover('destroy');
                oldItem.find('.weekly-description').popover('destroy');
                oldItem.fadeOut(function() {
                    oldItem.html(li.html());
                    oldItem.fadeIn();
                    
                    oldItem.find('.weekly-description').popover();
                    createWeeklyEditable(oldItem);
                });
            }
        }
    });

});

// Remove

$(document).on('click', '.removeWeeklyBtn', function() {

    var self = $(this);
    var id = self.data('id');
    var url = '/weekly/remove/' + id;

    showBtnBusy(self);

    $.ajax({
        url: url,
        error: function() {
            toastr.error('An error occured, Please try again later');
            hideBtnBusy(self, "Remove");
        },
        success: function() {
            hideBtnBusy(self, "Remove");

            var li = $('.weekly[data-id="' + id + '"]');
            li.popover('hide');
            li.fadeOut();
        }
    });

});

// initializing

function createWeeklyEditable(elem) {
    var editForm = elem.find('.weekly-edit-form');
    elem.popover({
        html: true,
        placement: 'top',
        title: 'Edit Weekly',
        content: editForm.html()
    });

    editForm.remove();
}

function initWeeklies() {
    var addWeekly = $('#addWeekly');
    if (addWeekly) {

        var addWeeklyContent = $('#addWeeklyContent');

        addWeekly.popover({
            html: true,
            placement: 'top',
            title: 'New Weekly',
            content: addWeeklyContent.html()
        });

        addWeeklyContent.remove();
    }

    $('.weekly-description').each(function () {
        $(this).popover();
    });

    $('.weekly[data-editable="1"]').each(function () {

        var self = $(this);
        createWeeklyEditable(self);

    });
}

/********************************
    Show/Hide change profile pic btn
*********************************/
$("#changeProfilePicArea").on({
    mouseenter: function () {
        $('#changeProfilePicBtn').slideDown();
    },
    mouseleave: function () {
        $('#changeProfilePicBtn').slideUp();
    }
});