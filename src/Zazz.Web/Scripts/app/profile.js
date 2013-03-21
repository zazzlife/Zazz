
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