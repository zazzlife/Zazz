﻿// ADD

$(document).on('click', '#selectPostCategoriesBtn', function() {
    var $btn = $(this);
    var $modal = $('#selectPostCategoryModal');
        
    var categories = [];

    $('#selectPostCategoryModal .category-select-btn.active').each(function () {
        var id = $(this).data('id');
        if (id) {
            categories.push(id);
        }
    });
    

        var message = $('#postInput').val();
        var toUser = $('#submitPostBtn').data('touser');

        if (!message) {
            toastr.error("Post message cannot be empty!");
            $modal.modal('hide');
            return;
        }

        showBtnBusy($btn);
        var url = '/posts/new';

        $.ajax({
            url: url,
            type: 'POST',
            data: {
                message: message,
                toUser: toUser ? toUser : null,
                categories: categories
            },
            traditional: true,
            error: function() {
                toastr.error('An error occured, Please try again later.');
                hideBtnBusy($btn, "Submit");

                $modal.modal('hide');
            },
            success: function(res) {
                var feed = $(res.trim());
                feed.prependTo($('#feedsContainer')).hide().slideDown();

                hideBtnBusy($btn, "Submit");
                applyPageStyles();

                $('#postInput').val("");

                $modal.modal('hide');
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


    var editHtml = '<textarea data-tag="1" id="editPostInput-' + id + '" style="margin-top: 25px; width: 82%; height: 70px;font-size: 12px;font-style: italic;"></textarea>';
    editHtml += '<button data-id="' + id + '" style="float:right;margin-top: 11px;" type="button" class="btn btn-small btn-success submitPostEdit">Submit</button>';
    editHtml += '<button style="float:right;margin-top: 45px;margin-right: -65px;width: 67px;" type="button" class="btn btn-small cancelPostEdit">Cancel</button>';

    p.html(editHtml);
    var editElem = p.children('#editPostInput-' + id);
    editElem.val(originalPostText);
    p.hide();


    p.fadeIn('fast', function () {
        editElem.focus();
        initInputTags();
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
        url: '/posts/edit/' + id,
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

$(function () {
    if (typeof ClubUsernames === 'undefined') return;
    $("#postInput")
        .bind("keydown", function (event) {
            if (event.keyCode === $.ui.keyCode.TAB && $(this).data("ui-autocomplete").menu.active)
                event.preventDefault();
        })
        .autocomplete({
            minLength: 0,
            source: function (request, response) {
                var m = request.term.match(/(?:[^\w]|^)@(\w*)$/);
                if (m == null || m.length != 2) {
                    response(null);
                    return;
                }
                var tag = m[1];

                m = $.map(ClubUsernames, function (item) {
                    if (item.substring(0, tag.length).toUpperCase() === tag.toUpperCase())
                        return item;
                });
                response(m);
            },
            focus: function () {
                return false;
            },
            select: function (event, ui) {
                var i = this.value.lastIndexOf("@");
                if (i == -1) return false;
                this.value = this.value.substring(0, i + 1) + ui.item.value;

                return false;
            }
        });
});

function TagFilter(tag) {
    $("#tag-filter").val(tag);
    $("#cat-select").change();
}