$(document).on('click', '.clubs-list-follow-img', function() {

    var $self = $(this);
    var clubId = $self.data('id');
    var action = $self.data('action');

    if (!clubId || !action) return;

    var isFollowing = action == "unfollow";

    $self.css('opacity', 0.7);

    var url = isFollowing
        ? '/follow/unfollow'
        : '/follow/followuser';

    var newImage = isFollowing
            ? '/Images/follow-icon.png'
            : '/Images/following-icon.png';

    var newAction = isFollowing ? 'follow' : 'unfollow';
    var newTitle = isFollowing ? 'Follow' : 'Unfollow';

    $.ajax({
        url: url,
        data: { id: clubId },
        success: function() {
            $self.attr('src', newImage);
            $self.attr('title', newTitle);
            $self.data('action', newAction);
            $self.css('opacity', 1);

            applyPageStyles();
        },
        error: function() {
            toastr.error("Request Failed!");
        }
    });

});