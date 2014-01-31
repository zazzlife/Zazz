
//Follow / Unfollow
$(document).on('click', '.clubs-list-follow-img', function () {

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

function updateTab($target, data, callback) {

    $('.school-filter-img').each(function () {
        $(this).removeClass('active');
    });

    $target.html('<p><i class="icon-spin icon-refresh"></i> Loading...</p>');
    $.ajax({
        url: '/home/clubs',
        data: data,
        success: function (res) {
            $target.html(res);

            if (callback)
                callback();
        },
        error: function () {
            toastr.error("Request Failed!");
        }
    });
}

//Club Tabs
$(document).on('click', 'a[data-load-clubs]', function() {

    var $self = $(this);
    var type = $self.attr('data-load-clubs');
    var target = $self.attr('href');

    updateTab($(target), { type: type }, function() {
        if (type == "schoolclubs") {
            $('#schoolClubsFilter').fadeIn();
        } else {
            $('#schoolClubsFilter').fadeOut();
        }
    });

});

//school filters
$(document).on('click', '.school-filter-img', function() {

    var $self = $(this);
    var $target = $('#schoolClubsTab');
    var schoolName = $self.data('name');

    updateTab($target, { type: 'schoolclubs', schoolName: schoolName }, function() {
        $self.addClass('active');
    });

});