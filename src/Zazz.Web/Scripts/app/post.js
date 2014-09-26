// ADD
var selectData = [];
var userTag = [];
var clubTag = [];
var lockUser = [];
$(document).ready(function () {
    $('.tag_select_a').on('change', function (evt, params) {
        if (params.selected) {
            //~/users/profile/4
            var data = params.selected.split("|");
            switch ($(this).data('tagtype')) {
                case "user":
                    userTag.push(data[2]);
                    break;
                case "club":
                    clubTag.push(data[2]);
                    break;
                case "lock":
                    lockUser.push(params.selected);
                    break;
            }
            var value = $("#postInput1").html().trim();
            if (value == '<i style="color:#b1b1b1">What\'s on your Mind?</i>')
                value = "";
            if (value == "")
                value += "<a href='/users/profile/" + data[2] + "' style='cursor:pointer' id='" + data[0] + "'>" + data[1] + "</a> ";
            else
                value += " <a href='/users/profile/" + data[2] + "' style='cursor:pointer' id='" + data[0] + "'>" + data[1] + "</a> ";
            $("#postInput1").html(value);
            selectData.push(data[0]);
        }
        else if (params.deselected) {
            switch ($(this).data('tagtype')) {
                case "user":
                    var data = params.deselected.split("|");
                    var removeItem = data[2];

                    userTag = jQuery.grep(userTag, function (value) {
                        return value != removeItem;
                    });

                    break;
                case "club":
                    var data = params.deselected.split("|");
                    var removeItem = data[2];

                    clubTag = jQuery.grep(clubTag, function (value) {
                        return value != removeItem;
                    });
                    break;
                case "lock":
                    var removeItem = params.deselected;
                    lockUser = jQuery.grep(lockUser, function (value) {
                        return value != removeItem;
                    });
                    break;
            }
        }
    });
    $('.tag_select_a1').on('change', function (evt, params) {
        if (params.selected) {
            lockUser.push(params.selected);
        }
        else if (params.deselected) {
            var removeItem = params.deselected;

            lockUser = jQuery.grep(lockUser, function (value) {
                return value != removeItem;
            });
        }
    });
});
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
    
    var oldmessage = $('#postInput1').html();
    $.each(selectData, function (index, data) {
        $("#postInput1 > a").each(function () {
            var data_id = $(this).attr('id');
            if (data_id == data) {
                $(this).replaceWith(" "+data+" ");
            }
        });
        
            
        });
    var message = $('#postInput1').html();
    message = message.replace("&nbsp;", "");
        var toUser = $('#submitPostBtn').data('touser');
        if (!message || message == '<i style="color:#b1b1b1">What\'s on your Mind?</i>') {
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
                categories: categories,
                metaData: '{ "taguser": ['+userTag+'], "tagclub": ['+clubTag+'], "lockuser": ['+lockUser+'] }'
            },
            traditional: true,
            error: function () {
                toastr.error('An error occured, Please try again later.');
                hideBtnBusy($btn, "Submit");
                $('#postInput1').html(oldmessage)
                $modal.modal('hide');
            },
            success: function(res) {
                var feed = $(res.trim());
                feed.prependTo($('#feedsContainer')).hide().slideDown();

                hideBtnBusy($btn, "Submit");
                applyPageStyles();

                $('#postInput1').html("<i style='color:#b1b1b1'>What's on your Mind?</i>");

                $modal.modal('hide');

                $('.tag_select_a').val('').trigger('chosen:updated');
                $('.tag_select_a1').val('').trigger('chosen:updated');

                $(".tagbtn").each(function () {
                    $(this).removeClass('active');
                    $(this).removeClass('yellow');
                    $("#" + $(this).data('tagid')).hide();
                });
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