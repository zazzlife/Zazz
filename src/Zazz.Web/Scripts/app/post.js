// ADD
$(document).on('click', '#submitPostBtn', function () {

    var self = $(this);
    var message = $('#postInput').val();
    var toUser = self.data('touser');

    if (!message) {
        toastr.error("Post message cannot be empty!");
        return;
    }

    showBtnBusy(self);
    var url = '/post/new';

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            message: message,
            toUser: toUser ? toUser : null
        },
        error: function () {
            toastr.error('An error occured, Please try again later.');
            hideBtnBusy(self, "Submit");
        },
        success: function (res) {
            var feed = $(res.trim());
            feed.prependTo($('#feedsContainer')).hide().slideDown();

            hideBtnBusy(self, "Submit");
            applyPageStyles();

            $('#postInput').val("");
        }
    });

});

//Edit
var originalPostText;
var isPostFeedEditBoxVisible = false;

$(document).on('click', '.editFeedBtn', function () {

    isPostFeedEditBoxVisible = true;

    var self = $(this);
    var p = self.parent().next('.post-feed-message');
    originalPostText = p.text().trim();
    var id = self.data('id');
    self.parent('.feed-actions').hide();


    var editHtml = '<textarea id="editPostInput-' + id + '" style="margin-top: 25px; width: 82%; height: 70px;font-size: 12px;font-style: italic;"></textarea>';
    editHtml += '<button data-id="' + id + '" style="float:right;margin-top: 11px;" type="button" class="btn btn-small btn-success submitPostEdit">Submit</button>';
    editHtml += '<button style="float:right;margin-top: 45px;margin-right: -65px;width: 67px;" type="button" class="btn btn-small cancelPostEdit">Cancel</button>';

    p.html(editHtml);
    var editElem = p.children('#editPostInput-' + id);
    editElem.val(originalPostText);
    p.hide();


    p.fadeIn('fast', function () {
        editElem.focus();
    });

});

$(document).on('click', '.submitPostEdit', function () {

    var self = $(this);
    var p = self.parent();
    var id = self.data('id');
    var editor = $('#editPostInput-' + id);
    var text = editor.val();

    if (!text || text.trim().length == 0) {
        toastr.error('Post cannot be empty');
        return;
    }

    editor.css('width', '78%');
    showBtnBusy(self);

    var showNormalPost = function (val) {
        p.fadeOut(function () {
            p.html('');
            p.css('margin-top', '6px');
            p.text(val);
            p.fadeIn();

            isPostFeedEditBoxVisible = false;
        });
    };

    $.ajax({
        url: '/post/edit/' + id,
        type: 'POST',
        cache: false,
        data: {
            text: text
        },
        error: function () {
            toastr.error('An error occured. Please try again later.');
            showNormalPost(originalPostText);
        },
        success: function () {
            showNormalPost(text);
        }
    });

});

$(document).on('click', '.cancelPostEdit', function () {

    var self = $(this);
    var p = self.parent();

    p.fadeOut('fast', function () {
        p.html('');
        p.text(originalPostText);
        p.css('margin-top', '6px');
        p.fadeIn();

        isPostFeedEditBoxVisible = false;
    });

});