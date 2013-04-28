/********************************
    Mark As Read
*********************************/

function markAllNotificationsAsRead() {
    var url = "/notification/markall";

    $.ajax({
        url: url,
        cache: false
    });
}

/********************************
    Popover
*********************************/

var isPopoutVisible = false;
var clickedAwayFromPopout = false;

$('#notifications-link').popover({
    html: true,
    trigger: 'manual'
}).click(function (e) {

    var self = $(this);
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
    var allNotificationsLink = $('<div class="notification-popover-footer"><a>See all</a></div>');
    allNotificationsLink.appendTo(popover);

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
            
            // calculating the new position of the popover
            var linkPosition = self.parent('li').offset();
            var linkWidth = self.parent('li').width();
            var linkHeight = self.parent('li').height();

            var left = linkPosition.left + linkWidth;
            var top = (linkPosition.top + (linkHeight / 2)) - (popover.height() / 2);

            popover.css({
                top: top,
                left: left
            });

        }
    });
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
})