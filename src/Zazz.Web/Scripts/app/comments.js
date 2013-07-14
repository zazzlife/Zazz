function showInputBusy(elem) {
    //disableing the current text box and removing focus
    elem.attr('disabled', 'disabled');
    elem.blur();

    //getting the current position and creating the loading indicator
    var position = elem.offset();
    var i = document.createElement('i');
    var loadingIndicator = $(i);

    position.left += (elem.width() - 8);
    position.top += 8;

    loadingIndicator.addClass('icon-refresh icon-spin');
    loadingIndicator.css(position);
    loadingIndicator.css('position', 'absolute');

    loadingIndicator.appendTo(elem.parent());
    loadingIndicator.hide().fadeIn('slow');

    return loadingIndicator;
}

$(document).on('blur', '.comment-textbox', function (e) {
    $(this).tooltip('destroy');
});

//Add
$(document).on('focus', '.comment-textbox', function (e) {
    $(this).tooltip('destroy');

    $(this).tooltip({
        title: "Press 'enter' to send",
        placement: 'right'
    });

    $(this).tooltip('show');
});


$(document).on('keypress', '.comment-textbox', function (e) {

    if (e.keyCode != 13) {
        return;
    }

    // Enter is pressed
    var self = $(this);
    var comment = self.val();

    if (!comment) {
        return;
    }

    var loadingIndicator = showInputBusy(self);

    var id = self.data('id');
    var commentType = self.attr('data-commentType');
    var url = "/comments/new";

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            id: id,
            commentType: commentType,
            comment: comment
        },
        error: function () {
            toastr.error("An error occured. Please try again later.");
            self.removeAttr('disabled');
            loadingIndicator.remove();
        },
        success: function (res) {
            self.val('');
            self.removeAttr('disabled');
            loadingIndicator.remove();

            var commentsContainer = self.parent().next('.feed-comments');
            commentsContainer.prepend(res);

            var commentElem = commentsContainer.children(':first');
            commentElem.hide().slideDown();

            applyPageStyles();
        }
    });

});


// Remove

$(document).on('click', '.comment-remove', function (e) {
    e.preventDefault();

    var self = $(this);
    var id = self.data('id');
    var url = '/comments/remove/' + id;
    var listItem = self.parent().parent().parent().parent();
    //      li       ul      div     li

    listItem.css('opacity', '0.6');

    $.ajax({
        url: url,
        error: function () {
            listItem.css('opacity', '1');
            toastr.error("An error occured. Please try again later.");
        },
        success: function () {
            listItem.fadeOut();
        }
    });

});

// Edit

var originalComment;

$(document).on('click', '.comment-edit', function (e) {
    e.preventDefault();


    var self = $(this);
    var id = self.data('id');
    var p = self.parent().parent().parent().prev();
    var text = p.text();

    originalComment = text;

    var editElem = $(document.createElement('input'));
    editElem.attr('type', 'text');
    editElem.attr('value', text);
    editElem.attr('title', "Press 'enter' to send");
    editElem.attr('data-id', id);
    editElem.css('width', '93%');
    editElem.addClass('comment-edit-box');

    p.html(editElem);

    editElem.tooltip({
        placement: 'right'
    });

    editElem.tooltip('show');

    editElem.focus();

});

$(document).on('keypress', '.comment-edit-box', function (e) {

    var self = $(this);
    var p = self.parent();
    var comment = self.val();

    if (e.keyCode != 13) {
        return;
    }

    // Enter is pressed

    var id = self.data('id');

    var url = '/comments/edit/' + id;

    var loadingIndicator = showInputBusy(self);

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            comment: comment
        },
        error: function () {
            toastr.error('An error occured. Please try again later.');
            self.removeAttr('disabled');
            loadingIndicator.remove();
        },
        success: function () {
            p.html(comment);
        }
    });
});

$(document).on('blur', '.comment-edit-box', function (e) {
    $(this).tooltip('destroy');
    var p = $(this).parent();
    p.html(originalComment);
});

$(document).on('keyup', '.comment-edit-box', function (e) {

    if (e.keyCode == 27) {
        $(this).tooltip('destroy');

        var p = $(this).parent();
        p.html(originalComment);
    }
});

// Get More Comments

$(document).on('click', '.load-comments-btn', function () {

    var self = $(this);
    var id = self.data('id');

    var url = '/comments/get/' + id;
    var commentType = self.attr('data-commentType');

    var ul = self.prev();

    var lastComment = ul.children('li:last-child');
    var lastCommentId = lastComment.data('id');

    var btnText = showBtnBusy(self);

    $.ajax({
        url: url,
        data: {
            commentType: commentType,
            lastComment: lastCommentId
        },
        error: function () {
            toastr.error("An error occured. Please try again later");
            hideBtnBusy(self, btnText);
        },
        success: function (res) {
            var newComments = $(res.trim());
            newComments.appendTo(ul);

            // there is a problem with animating them, so I'm going to do this manually

            for (var i = 0; i < newComments.length; i++) {
                var li = $(newComments[i]);
                var id = li.data('id');

                if (id) {
                    li.hide();
                    li.fadeIn();
                }
            }

            hideBtnBusy(self, btnText);
            applyPageStyles();
        }
    });

});

// Lightbox Comments

function loadLightboxComments(photoId, commentsContainer) {

    var isLoaded = commentsContainer.data('isLoaded');
    if (isLoaded) {
        return;
    }

    var url = "/comments/LightboxComments";

    $.ajax({
        url: url,
        cache: false,
        data: {
            id: photoId
        },
        error: function() {
            commentsContainer.html('<h4>Failed to load comments</h4>');
        },
        success: function(res) {
            commentsContainer.fadeOut(function() {
                commentsContainer.html(res);
                commentsContainer.fadeIn();
                commentsContainer.data('isLoaded', 1);

                commentsContainer.find('*[title]').each(function() {
                    $(this).data('placement', 'top');
                });

                applyPageStyles();
            });
        }
    });
}