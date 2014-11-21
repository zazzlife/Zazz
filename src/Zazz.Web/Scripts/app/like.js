function showLikeBtn($btn, id) {
    
    var likeHtml = '<i class="icon-thumbs-up"></i> Like';
    var actionUrl = "/like/add/" + id;
    $btn.html(likeHtml);

    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'add');
}

function showRemoveLikeBtn($btn, id) {

    var actionUrl = "/like/remove/" + id;
    var removeLikeHtml = '<span style="color:#08a7ee;"><i class="icon-thumbs-up"></i> Liked</span>';

    $btn.html(removeLikeHtml);
    
    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'remove');
}

$(document).on('click', 'button[data-like-btn]', function () {

    var $self = $(this);
    var isLoaded = $self.attr('data-isloaded');
    if (!isLoaded) {
        return;
    }

    $self.css('opacity', '0.4');
    var url = $self.attr('data-url');
    var id = $self.attr('data-id');

    var action = $self.attr('data-action');
    $.ajax({
        url: url,
        cache: false,
        error: function() {
            toastr.error("An error occured, please try again later");
            $self.css('opacity', '1');
        },
        success: function () {
            var likesContainer = $('#photoLikesCount-' + id);
            var likesCount = parseInt(likesContainer.text()
                .replace(')', '')
                .replace('(', ''));

            if (action == 'remove') {
                showLikeBtn($self, id);
                likesCount--;
            } else {
                showRemoveLikeBtn($self, id);
                likesCount++;
            }

            likesContainer.text('(' + likesCount + ')');
        }
    });

});

$(document).on('click', 'button[data-postlike-btn]', function () {

    var $self = $(this);
    var isLoaded = $self.attr('data-isloaded');
    if (!isLoaded) {
        return;
    }

    $self.css('opacity', '0.4');
    var url = $self.attr('data-url');
    var id = $self.attr('data-id');
    var action = $self.attr('data-action');
    $.ajax({
        url: url,
        cache: false,
        error: function () {
            toastr.error("An error occured, please try again later");
            $self.css('opacity', '1');
        },
        success: function () {
            var likesContainer = $('#postLikesCount-' + id);
            var likesCount = parseInt(likesContainer.text()
                .replace(')', '')
                .replace('(', ''));

            if (action == 'remove') {
                showPostLikeBtn($self, id);
                likesCount--;
            } else {
                showRemovePostLikeBtn($self, id);
                likesCount++;
            }

            likesContainer.text('(' + likesCount + ')');
        }
    });

});


function loadLikeAction(id, $btn) {
    
    var url = "/like/exists/" + id;

    $.ajax({
        url: url,
        cache: false,
        success: function(res) {
            
            if (res.toLowerCase() == 'true') {
                showRemoveLikeBtn($btn, id);
            } else {
                showLikeBtn($btn, id);
            }
        }
    });

}

function getLikesCount(id) {

    var url = "/like/count/" + id;
    var likesContainer = $('#photoLikesCount-' + id);


    $.ajax({
        url: url,
        cache: false,
        error: function() {
            toastr.error("Failed to get the likes count");
            likesContainer.fadeOut('slow');
        },
        success: function(res) {
            likesContainer.html();
            likesContainer.text('(' + res + ')');
        }
    });
}

function loadLikeButtons() {

    $('button[data-like-btn]').each(function() {

        var $self = $(this);

        var isLoaded = $self.attr('data-isloaded');
        if (isLoaded == "1") {
            return;
        }
        
        $self.css('opacity', '0.4');
        var id = $self.attr('data-id');

        getLikesCount(id);
        loadLikeAction(id, $self);
    });

    $('button[data-postlike-btn]').each(function () {

        var $self = $(this);

        var isLoaded = $self.attr('data-isloaded');
        if (isLoaded == "1") {
            return;
        }

        $self.css('opacity', '0.4');
        var id = $self.attr('data-id');

        getPostLikesCount(id);
        loadPostLikeAction(id, $self);
    });
}





function loadPostLikeAction(id, $btn) {

    var url = "/like/postexists/" + id;

    $.ajax({
        url: url,
        cache: false,
        success: function (res) {

            if (res.toLowerCase() == 'true') {
                showRemovePostLikeBtn($btn, id);
            } else {
                showPostLikeBtn($btn, id);
            }
        }
    });

}


function showPostLikeBtn($btn, id) {

    var likeHtml = '<i class="icon-thumbs-up"></i> Like';
    var actionUrl = "/like/postadd/" + id;
    $btn.html(likeHtml);

    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'add');
}

function showRemovePostLikeBtn($btn, id) {

    var actionUrl = "/like/postremove/" + id;
    var removeLikeHtml = '<span style="color:#08a7ee;"><i class="icon-thumbs-up"></i> Liked</span>';

    $btn.html(removeLikeHtml);

    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'remove');
}


function getPostLikesCount(id) {

    var url = "/like/postcount/" + id;
    var likesContainer = $('#postLikesCount-' + id);


    $.ajax({
        url: url,
        cache: false,
        error: function () {
            toastr.error("Failed to get the likes count");
            likesContainer.fadeOut('slow');
        },
        success: function (res) {
            likesContainer.html();
            likesContainer.text('(' + res + ')');
        }
    });
}