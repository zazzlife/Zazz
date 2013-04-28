/********************************
    Popover
*********************************/

var isPopoutVisible = false;
var clickedAwayFromPopout = false;

$('#notifications-link').popover({
    html: true,
    trigger: 'manual'
}).click(function (e) {
    $(this).popover('show');
    clickedAwayFromPopout = false;
    isPopoutVisible = true;

    $(this).next('.popover').css('width', '555px');
    $(this).next('.popover').css('max-width', '555px');
    $(this).next('.popover').css('min-height', '90px');

    e.preventDefault();
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

