﻿var LOADING_INDICATOR = '<i class="icon-spin icon-refresh"></i>';

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
    $('*[title]').tooltip('destroy');
    $('*[title]').tooltip();
    $('*[data-toggle="popover"]').popover();

    $(document).on('click', '*[data-toggle]', function(e) {
        e.preventDefault();
    });
}

function showAjaxErrorMessage(res) {
    toastr.error(res.message);
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

            var options = '<option value></option>';

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

    var url = "/photo/getphotos";
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
    panelToUpdate.css('opacity', '0.5');

    $.ajax({
        url: url,
        error: function () {
            toastr.error("An error occured");
            panelToUpdate.css('opacity', '1');
        },
        success: function (res) {
            panelToUpdate.fadeOut(function () {
                panelToUpdate.html(res);
                panelToUpdate.css('opacity', '1');

                panelToUpdate.fadeIn();
            });

        }
    });

});

$(document).on('click', '#pg-modal *[data-selectPhoto]', function (e) {

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

$(document).on('click', '#uploadImg', function () {
    var description = $('#Description').val();
    var albumId = $('#upload-albumSelect').val();
    var showInFeed = $(this).data('feed');

    if (!imgUploader)
        return;
    imgUploader.setParams({
        albumId: albumId,
        description: description,
        showInFeed: showInFeed
    });

    imgUploader.uploadStoredFiles();

    showBtnBusy($(this));
    imgUploadBtn = $(this);
});

$(document).on('show', '#uploadPicModalWithFeed', function () {
    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));
    initImgUploader(function (id, name, response) {

        if (!response.success) {
            toastr.error(response.error);
        } else {

            $.ajax({
                url: '/photo/feed/' + response.photoId,
                error: function () {
                    toastr.error('Image was uploaded but failed to get the feed. Please refresh the page.');

                    $('#uploadPicModalWithFeed').modal('hide');
                    if (imgUploadBtn) {
                        hideBtnBusy(imgUploadBtn, "Upload");
                    }
                },
                success: function (res) {
                    $('#uploadPicModalWithFeed').modal('hide');
                    if (imgUploadBtn) {
                        hideBtnBusy(imgUploadBtn, "Upload");
                    }

                    var feed = $(res.trim());
                    feed.prependTo($('#feedsContainer')).hide().slideDown();
                    applyPageStyles();
                }
            });
        }

    });

});

/////////////////////////////////////////////////////////////////////////

/********************************
    Search Autocomplete
*********************************/

function showSearchIconBusy(callback) {

    var searchIcon = $('#searchIcon');

    searchIcon.fadeOut('fast', function () {

        searchIcon.removeClass('icon-search');
        searchIcon.addClass('icon-refresh');
        searchIcon.addClass('icon-spin');

        searchIcon.fadeIn('fast', function () {
            callback();
        });
    });
}

function hideSearchIconBusy() {

    var searchIcon = $('#searchIcon');

    searchIcon.fadeOut('fast', function () {

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

        showSearchIconBusy(function () {

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
    Post
*********************************/

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


/********************************
    Feed
*********************************/

$(document).on('mouseenter', '.feed-content', function () {
    if (!isPostFeedEditBoxVisible) {
        $(this).children('.feed-actions').fadeIn('fast');
    }
});

$(document).on('mouseleave', '.feed-content', function () {
    $(this).children('.feed-actions').fadeOut('fast');
});

$(document).on('click', '.removeFeedBtn', function () {

    var self = $(this);
    var url = self.data('url');


    $.ajax({
        url: url,
        cache: false,
        error: function () {
            toastr.error('An error occured, Please try again later.');
        },
        success: function () {
            self.closest('.feed-item').fadeOut();
        }
    });

});

// load more feeds
$(document).on('click', '#load-feeds', function () {

    var self = $(this);
    var lastFeed = $('.feed-item:last');
    var lastFeedId = lastFeed.data('id');
    var url = self.data('url');

    showBtnBusy(self);

    $.ajax({
        url: url,
        cache: false,
        data: {
            lastFeedId: lastFeedId
        },
        error: function () {
            toastr.error('An error occured, please try again later.');
            hideBtnBusy(self, "Load more...");
        },
        success: function (res) {
            var data = $(res.trim());

            if (data.find('.feed-item').length > 0) {

                self.fadeOut(function () {
                    data.appendTo(lastFeed).hide().slideDown();
                    self.remove();
                    applyPageStyles();
                });

            } else {
                self.text('Not available');
            }
        }
    });

});

$(function () {

    applyPageStyles();

});