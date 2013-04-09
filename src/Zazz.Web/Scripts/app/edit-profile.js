/** Will call some functions from global.js  **/

var cropPhotoId;
var updateUrl;
var photoType;


function onProfilePicChangeClick() {
    photoType = "profile";
}

function onCoverPicChangeClick() {
    photoType = "cover";
}

function updateUserPhotoId(photoId) {
    if (photoType == "profile") {
        updateProfilePic(photoId);
    } else if (photoType == "cover") {
        updateCoverPic(photoId);
    }
}

function showCropImg(modalBody, imgId, imgUrl) {

    cropPhotoId = imgId;

    var html = '<a target="_blank" class="btn btn-large btn-info" href="/photo/crop/' + imgId + '?for=' + photoType + '">Crop</button>';

    modalBody.slideUp(function () {
        modalBody.html(html);
        modalBody.slideDown();

    });

}

$('#uploadPicModalWithCrop').on('show', function () {

    var uploadModalContent = '<div class="control-group">' +
        '<label class="control-label">Picture</label>' +
        '<div id="upload"></div></div>' +
        '<div class="control-group"><label class="control-label" for="upload-albumSelect">Album</label>' +
        '<div class="controls"><select class="span3" id="upload-albumSelect" name="albumId">' +
        '<option>Loading...</option>' +
        '</select>' +
        '</div>' +
        '</div>' +
        '<div class="control-group">' +
        '<label class="control-label" for="Description">Description: </label>' +
        '<div class="controls">' +
        '<div class="editor-field">' +
        '<input class="span4" data-val="true" data-val-length="The field Description must be a string with a maximum length of 250." data-val-length-max="250" id="Description" name="Description" placeholder="Description" type="text" value="" />                            <span class="field-validation-valid help-inline" data-valmsg-for="Description" data-valmsg-replace="true"></span>' +
        '</div></div></div>' +
        '<p><button id="uploadImg" class="btn btn-primary">Submit</button></p>';


    $('#uploadPicModalWithCrop .modal-body').html(uploadModalContent);

    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));

    initImgUploader(function (id, name, response) {
        if (!response.success) {
            toastr.error(response.error);
        } else {

            var photoId = response.photoId;
            var photoUrl = response.photoUrl;
            var modalBody = $('#uploadPicModalWithCrop .modal-body');

            updateUserPhotoId(photoId);

            showCropImg(modalBody, photoId, photoUrl);
        }
    });
});

$('#pg-modalWithCrop').on('show', function () {

    var galleryModalContent = '<strong>Album: </strong><select class="span4" id="pg-albumSelect"><option>Loading...</option></select><div id="pg-photos"><i class="icon-spin icon-refresh"></i></div>';
    $('#pg-modalWithCrop .modal-body').html(galleryModalContent);

    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function (res) {
        loadPGPhotos();
    });

});


$(document).on('click', '#pg-modalWithCrop *[data-selectPhoto]', function (e) {

    e.preventDefault();

    var imgId = $(this).data('id');
    var imgUrl = $(this).data('url');
    var modalBody = $(this).closest('.modal-body');

    updateUserPhotoId(imgId);

    showCropImg(modalBody, imgId, imgUrl);
});

$(document).on('click', '.selectProfilePic a', function () {
    onProfilePicChangeClick();
});

$(document).on('click', '.selectCoverPic a', function () {
    onCoverPicChangeClick();
});