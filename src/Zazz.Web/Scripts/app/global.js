var LOADING_INDICATOR = '<i class="icon-spin icon-refresh"></i>';

function showBtnBusy(btn) {
    var originalText = $(btn).html();

    var textWithSpinner = '<i style="margin-right:5px;" class="icon-refresh icon-spin"></i>' + originalText;

    $(btn).attr('disabled', 'disabled');
    $(btn).addClass('disabled');
    $(btn).html(textWithSpinner);

    return originalText;
}

function hideBtnBusy(btn, originalText) {
    $(btn).removeAttr('disabled', 'disabled');
    $(btn).removeClass('disabled');
    $(btn).html(originalText);
}

function applyPageStyles() {
    $('.datepicker').datetimepicker();
    $('*[title]').tooltip();
}

$('#party-web-link').click(function () {
    var url = "/follow/GetFollowRequests";

    $.ajax({
        url: url,
        cache: false,
        error: function () {
            toastr.error("An error occured, Please try again later.");
        },
        success: function (data) {
            var container = $('#party-web-requests-body');
            container.fadeOut('slow', function () {
                container.html(data);
                container.fadeIn('slow');
            });
        }
    });

});

/////////////// SELECT IMAGE FROM GALLERY ////////////////

function loadAlbumsDropDownAsync(dropdownElem) {

    var def = $.Deferred();

    $.ajax({
        url: "/album/getalbums",
        error: function () {
            toastr.error("Failed to load the albums. Please try again later.");
            def.reject();
        },
        success: function (res) {

            var options = "";

            _.forEach(res, function (obj) {
                options += '<option value="' + obj.id + '">' + obj.name + '</option>';
            });

            $(dropdownElem).html(options);
            def.resolve(res);
        }
    });

    return def.promise();
}

function loadPGPhotos() {
    var albumId = $('#pg-albumSelect').val();
    if (albumId) {

        var url = "/album/getphotos";
        var photoContainer = $('#pg-photos');

        photoContainer.html(LOADING_INDICATOR);

        $.ajax({
            url: url,
            data: {
                albumId: albumId
            },
            error: function () {
                toastr.error("Failed to load photos. Please try again later");
            },
            success: function (res) {
                photoContainer.html(res);
            }
        });
    }
}

$('#pg-modal').on('show', function () {
    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function (res) {
        loadPGPhotos();
    });
});

$(document).on('click', '*[data-ajax-pagination] a', function (e) {
    e.preventDefault();

    var url = $(this).attr('href');
    var panelToUpdate = $($(this).closest('*[data-ajax-pagination]').attr('data-update'));
    panelToUpdate.html(LOADING_INDICATOR);

    $.ajax({
        url: url,
        error: function () {
            toastr.error("An error occured");
        },
        success: function (res) {
            panelToUpdate.html(res);
        }
    });

});

$(document).on('click', '#pg-modal button[data-selectPhoto]', function (e) {

    e.preventDefault();

    var id = $(this).data('id');
    var imgUrl = $(this).data('url');

    $('#photoId').val(id);
    $('#selectedImg-thumbnail').attr('src', imgUrl);

    $('#pg-modal').modal('hide');
});



$(document).on('change', '#pg-albumSelect', function () {
    loadPGPhotos();
});

///////////////////////UPLOAD PHOTO MODAL////////////////////////////////

var imgUploader;
var imgUploadBtn;

function initImgUploader(onComplete) {
    
    imgUploader = new qq.FineUploader({
        element: document.getElementById("upload"),
        request: {
            endpoint: '/photo/ajaxupload/',
            inputName: "image"
        },
        autoUpload: false,
        multiple: false,
        disableCancelForFormUploads: true,
        validation: {
            allowedExtensions: ['jpg', 'jpeg']
        },
        showMessage: function (msg) {
            toastr.info(msg);
        },
        callbacks: {
            onComplete: onComplete
        },
        template: '<div class="qq-uploader">' +
    '<div class="qq-upload-drop-area"><span>{dragZoneText}</span></div>' +
    '<div class="qq-upload-button btn btn-info"><div>{uploadButtonText}</div></div>' +
    '<span class="qq-drop-processing"><span>{dropProcessingText}</span><span class="qq-drop-processing-spinner"></span></span>' +
    '<ul class="qq-upload-list"></ul>' +
    '</div>',
        fileTemplate: '<li class="hide">' +
    '<div class="qq-progress-bar"></div>' +
    '<span class="qq-upload-spinner"></span>' +
    '<span class="qq-upload-finished"></span>' +
    '<span class="qq-upload-file"></span>' +
    '<span class="qq-upload-size"></span>' +
    '<a class="qq-upload-cancel" href="#">{cancelButtonText}</a>' +
    '<a class="qq-upload-retry" href="#">{retryButtonText}</a>' +
    '<a class="qq-upload-delete" href="#">{deleteButtonText}</a>' +
    '<span class="qq-upload-status-text">{statusText}</span>' +
    '</li>'

    });

}

$('#uploadPicModal').on('show', function () {
    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));

    initImgUploader(function(id, name, response) {
        if (!response.success) {
            toastr.error(response.error);
        } else {
            $('#photoId').val(response.photoId);
            $('#selectedImg-thumbnail').attr('src', response.photoUrl);
            $('#uploadPicModal').modal('hide');

            if (imgUploadBtn) {
                hideBtnBusy(imgUploadBtn, "Upload");
            }
        }
    });
});

$(document).on('click', '#uploadImg', function () {
    var description = $('#Description').val();
    var albumId = $('#upload-albumSelect').val();
    if (!albumId) {
        toastr.error("Album must be selected");
        return;
    }

    if (!imgUploader)
        return;
    imgUploader.setParams({
        albumId: albumId,
        description: description
    });

    imgUploader.uploadStoredFiles();

    showBtnBusy($(this));
    imgUploadBtn = $(this);
});

/////////////////////////////////////////////////////////////////////////

/********************************
    Search Autocomplete
*********************************/

function showSearchIconBusy(callback) {

    var searchIcon = $('#searchIcon');

    searchIcon.fadeOut('fast', function() {

        searchIcon.removeClass('icon-search');
        searchIcon.addClass('icon-refresh');
        searchIcon.addClass('icon-spin');

        searchIcon.fadeIn('fast', function() {
            callback();
        });
    });
}

function hideSearchIconBusy() {
    
    var searchIcon = $('#searchIcon');

    searchIcon.fadeOut('fast', function() {

        searchIcon.removeClass('icon-refresh');
        searchIcon.removeClass('icon-spin');
        searchIcon.addClass('icon-search');

        searchIcon.fadeIn('fast');
    });
}

var searchAutocompleteCache = {};

$('#navbarSearch').autocomplete({
    delay: 500,
    minLength: 2,
    source: function (req, res) {

        showSearchIconBusy(function() {

            var q = req.term;

            if (q in searchAutocompleteCache) {
                res(searchAutocompleteCache[q]);
                hideSearchIconBusy();
                return;
            }

            $.ajax({
                url: '/home/search',
                data: {
                    q: q
                },
                success: function (data) {
                    hideSearchIconBusy();
                    searchAutocompleteCache[q] = data;
                    res(data);
                }
            });

        });
        
    }
}).data("ui-autocomplete")._renderItem = function (ul, item) {

    var tmpl = "<a href='/user/profile/" + item.id + "' style='padding: 5px; margin-bottom:50;'><img class='img-rounded' style='margin-right:8px; width:32px; heigth:32px;' src='" + item.img + "' />" + item.value + "</a>";

    return $("<li />")
            .data("ui-autocomplete-item", item)
            .append(tmpl)
            .appendTo(ul);
};

/********************************
    Comments
*********************************/

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


$(document).on('keypress', '.comment-textbox', function(e) {

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
    var feedType = self.attr('data-feedType');
    var url = "/comment/new";

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            id: id,
            feedType: feedType,
            comment: comment
        },
        error: function() {
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

$(document).on('click', '.comment-remove', function(e) {
    e.preventDefault();

    var self = $(this);
    var id = self.data('id');
    var url = '/comment/remove/' + id;
    var listItem = self.parent().parent().parent().parent();
                    //      li       ul      div     li

    listItem.css('opacity', '0.6');

    $.ajax({
        url: url,
        error: function () {
            listItem.css('opacity', '1');
            toastr.error("An error occured. Please try again later.");
        },
        success: function() {
            listItem.fadeOut();
        }
    });

});

// Edit

var originalComment;

$(document).on('click', '.comment-edit', function(e) {
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
    editElem.css('width', 370);
    editElem.addClass('comment-edit-box');

    p.html(editElem);

    editElem.tooltip({
        placement: 'right'
    });

    editElem.tooltip('show');

    editElem.focus();

});

$(document).on('keypress', '.comment-edit-box', function(e) {

    var self = $(this);
    var p = self.parent();
    var comment = self.val();

    if (e.keyCode != 13) {
        return;
    }

    // Enter is pressed
    
    var id = self.data('id');

    var url = '/comment/edit/' + id;

    var loadingIndicator = showInputBusy(self);

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            comment: comment
        },
        error: function() {
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

$(document).on('keyup', '.comment-edit-box', function(e) {

    if (e.keyCode == 27) {
        $(this).tooltip('destroy');
        
        var p = $(this).parent();
        p.html(originalComment);
    }
});

// Get More Comments

$(document).on('click', '.load-comments-btn', function() {

    var self = $(this);
    var id = self.data('id');

    var url = '/comment/get/' + id;
    var feedType = self.attr('data-feedType');

    var ul = self.prev();

    var lastComment = ul.children('li:last-child');
    var lastCommentId = lastComment.data('id');

    var btnText = showBtnBusy(self);

    $.ajax({
        url: url,
        data: {
            feedType: feedType,
            lastComment: lastCommentId
        },
        error: function() {
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

/********************************
    Post
*********************************/

$(document).on('click', '#submitPostBtn', function() {

    var self = $(this);
    var message = $('#postInput').val();
    
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
            message: message
        },
        error: function() {
            toastr.error('An error occured, Please try again later.');
            hideBtnBusy(self, "Submit");
        },
        success: function(res) {
            var feed = $(res.trim());
            feed.prependTo($('#feedsContainer')).hide().slideDown();
            
            hideBtnBusy(self, "Submit");
            applyPageStyles();

            $('#postInput').val("");
        }
    });

});

/********************************
    Feed
*********************************/

$(document).on('mouseenter', '.feed-content', function () {
    $(this).children('.feed-actions').fadeIn('fast');
});

$(document).on('mouseleave', '.feed-content', function () {
    $(this).children('.feed-actions').fadeOut('fast');
});

$(function() {

    applyPageStyles();
    
});