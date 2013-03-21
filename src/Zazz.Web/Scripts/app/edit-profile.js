/** Will call some functions from global.js  **/

var cropSize;
var cropPhotoId;
var aspectRatio;
var updateUrl;

function onProfilePicChangeClick() {
    aspectRatio = 1;
    updateUrl = "/user/ChangeProfilePic/";
}

function onCoverPicChangeClick() {
    aspectRatio = 10 / 3;
    updateUrl = "/user/ChangeCoverPic/";
}

function updateUserPhotoId(photoId) {
    $.ajax({
        url: updateUrl,
        data: {
            id: photoId
        },
        error: function() {
            toastr.error("An error occured. Please try again later.");
        }
    });
}

function showCropImg(modalBody, imgId, imgUrl) {

    cropPhotoId = imgId;

    var html = '<img id="cropImg" src="' + imgUrl + '" />';
    html += '<button style="margin-top:10px;" id="cropBtn" type="button" class="btn btn-info">Crop</button>';

    modalBody.slideUp(function () {
        modalBody.html(html);
        modalBody.slideDown();

        $('#cropImg').Jcrop({
            boxWidth: 530,
            onChange: function (c) {
                cropSize = c;
            },
            aspectRatio: aspectRatio
        });
    });

}

$('#uploadPicModalWithCrop').on('show', function() {

    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));

    initImgUploader(function(id, name, response) {
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

$('#pg-modalWithCrop').on('show', function() {

    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function(res) {
        loadPGPhotos();
    });

});


$(document).on('click', '#pg-modalWithCrop button[data-selectPhoto]', function (e) {

    e.preventDefault();

    var imgId = $(this).data('id');
    var imgUrl = $(this).data('url');
    var modalBody = $(this).closest('.modal-body');

    updateUserPhotoId(imgId);

    showCropImg(modalBody, imgId, imgUrl);
});

$(document).on('click', '#cropBtn', function(e) {
    e.preventDefault();
    var btn = $(this);

    showBtnBusy(btn);

    $.ajax({
        url: '/photo/crop/' + cropPhotoId,
        data: cropSize,
        error: function() {
            toastr.error('An error occured. Please try again later.');
            hideBtnBusy(btn, 'Crop');
        },
        success: function() {
            hideBtnBusy(btn, 'Crop');

            var modal = btn.parent().parent();
            modal.modal('hide');
        }
    });
});

function resetModalContents() {

}


$(document).on('click', '.selectProfilePic a', function() {
    onProfilePicChangeClick();
});

$(document).on('click', '.selectCoverPic a', function() {
    onCoverPicChangeClick();
});