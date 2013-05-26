/********************************
    Mark As Read
*********************************/

function markAllNotificationsAsRead() {
    var url = "/notification/markall";

    $.ajax({
        url: url,
        cache: false,
        success: function() {
            $('#new-notifications-count').fadeOut('slow');
        }
    });
}

/********************************
    Popover
*********************************/

var isPopoutVisible = false;
var clickedAwayFromPopout = false;

function calculatePopoverPosition(link, popover) {
    
    var linkPosition = link.offset();
    var linkWidth = link.width();
    var linkHeight = link.height();

    var left = linkPosition.left + (linkWidth / 2) - (popover.width() / 2);
    var top = linkPosition.top + linkHeight;

    popover.css({
        top: top,
        left: left
    });
}

$('#notifications-link').popover({
    html: true,
    trigger: 'manual',
    placement: 'bottom'
}).click(function (e) {

    var self = $(this);
    var li = self.parent('li');
    self.popover('show');
    clickedAwayFromPopout = false;
    isPopoutVisible = true;

    e.preventDefault();

    var popover = self.next('.popover');

    popover.css('width', '555px');
    popover.css('max-width', '555px');
    popover.css('min-height', '90px');

    var popoverContent = popover.children('.popover-content');

    popover.children('.notification-popover-footer').remove();
    var allNotificationsLink = $('<div class="notification-popover-footer"><a href="/notification">View all</a></div>');
    allNotificationsLink.appendTo(popover);

    calculatePopoverPosition(li, popover);

    var url = "/notification/get";
    var take = 5;

    $.ajax({
        url: url,
        cache: false,
        data: {
            take: take
        },
        error: function() {
            toastr.error("failed to get notifications, please try again later.");
        },
        success: function (res) {
            popoverContent.html(res);
            
            //moving lightbox markups out of the notification popover
            $('.popover-content .lightbox').each(function() {
                var article = $('article');
                $(this).appendTo(article);
            });

            // calculating the new position of the popover
            calculatePopoverPosition(li, popover);
            //var linkPosition = self.parent('li').offset();
            //var linkWidth = self.parent('li').width();
            //var linkHeight = self.parent('li').height();

            //var left = linkPosition.left + (linkWidth / 2) - (popover.width() / 2);
            //var top = linkPosition.top + linkHeight;

            //popover.css({
            //    top: top,
            //    left: left
            //});
            applyPageStyles();
            markAllNotificationsAsRead();
        }
    });
});

$(document).on('click', '.popover', function() {
    clickedAwayFromPopout = false;
});

$(document).click(function (e) {
    if (isPopoutVisible & clickedAwayFromPopout) {
        $('#notifications-link').popover('hide');
        isPopoutVisible = clickedAwayFromPopout = false;
    }
    else {
        clickedAwayFromPopout = true;
    }
});

/********************************
    Remove
*********************************/

$(document).on('click', '*[data-removeNotification]', function() {

    var url = "/notification/remove";
    var self = $(this);
    var li = self.parent();

    var id = self.data('id');
    if (!id) {
        return;
    }

    li.css('opacity', '0.6');
    $.ajax({
        url: url,
        cache: false,
        data: {
            id: id
        },
        error: function() {
            toastr.error('An error occured, please try again later.');
            li.css('opacity', '1');
        },
        success: function() {
            li.fadeOut();
        }
    });
});

/********************************
    Load more
*********************************/

$(document).on('click', '#load-notifications', function() {

    var self = $(this);

    var lastNotification = $('.notification-row:last-child');
    var lastNotificationId = lastNotification.data('id');

    if (!lastNotification) {
        return;
    }

    var btnText = showBtnBusy(self);

    var url = "/notification/get";
    var take = 30;

    $.ajax({
        url: url,
        data: {
            take: take,
            lastNotification: lastNotificationId
        },
        cache: false,
        error: function() {
            toastr.error('Failed to get notifications, please try again later.');
            hideBtnBusy(self, btnText);
        },
        success: function (res) {
            if (res) {
                var data = res.trim();
                //since the server returns ul i have to remove it here.
                var li = $(data).children('li');
                
                if (li.length > 0) {

                    var container = $('.notification-list');
                    $(li).appendTo(container).hide().fadeIn();
                    applyPageStyles();
                } else {
                    hideBtnBusy(self, "That's all!");
                    setBtnDisabled(self);
                    return;
                }
            }
            
            hideBtnBusy(self, btnText);
        }
    });
});