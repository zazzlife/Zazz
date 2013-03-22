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

$('.comment-textbox').on('keypress', function(e) {

    if (e.keyCode != 13) {
        return;
    }
    
    // Enter is pressed
    var self = $(this);
    var comment = self.val();

    if (!comment) {
        return;
    }
        

    //disableing the current text box and removing focus
    self.attr('disabled', 'disabled');
    self.blur();

    //getting the current position and creating the loading indicator
    var position = self.offset();
    var i = document.createElement('i');
    var loadingIndicator = $(i);

    position.left += (self.width() - 8);
    position.top += 8;

    loadingIndicator.addClass('icon-refresh');
    loadingIndicator.addClass('icon-spin');
    loadingIndicator.css(position);
    loadingIndicator.css('position', 'absolute');

    loadingIndicator.appendTo(self.parent());
    loadingIndicator.hide().fadeIn('slow');


    var id = self.data('id');
    var feedType = self.attr('data-feedType');
    var url = "/comment/new";

    $.ajax({
        url: url,
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

        }
    });

});

///////////////////////////////
$(function() {

    $('.datepicker').datetimepicker();

    $('*[title]').tooltip();

    $('#party-web-link').click(function() {
        var url = "/follow/GetFollowRequests";

        $.ajax({
            url: url,
            cache: false,
            error: function() {
                toastr.error("An error occured, Please try again later.");
            },
            success: function(data) {
                var container = $('#party-web-requests-body');
                container.fadeOut('slow', function() {
                    container.html(data);
                    container.fadeIn('slow');
                });
            }
        });

    });
});