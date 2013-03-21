/** Will call some functions from global.js  **/

var cropSize;
var cropPhotoId;

$('#uploadPicModalWithCrop').on('show', function() {

    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));

    initImgUploader(function(id, name, response) {
        if (!response.success) {
            toastr.error(response.error);
        } else {

            var photoId = response.photoId;
            var photoUrl = response.photoUrl;
            var modalBody = $('#uploadPicModalWithCrop .modal-body');

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

function showCropImg(modalBody, imgId, imgUrl) {

    cropPhotoId = imgId;
    
    var html = '<img id="cropImg" src="' + imgUrl + '" />';
    html += '<button style="margin-top:10px;" id="cropBtn" type="button" class="btn btn-info">Crop</button>';

    modalBody.slideUp(function () {
        modalBody.html(html);
        modalBody.slideDown();

        $('#cropImg').Jcrop({
            boxWidth: 530,
            onChange: function(c) {
                cropSize = c;
            },
            aspectRatio: 10 / 3
        });
    });

}

$(document).on('click', '#pg-modalWithCrop button[data-selectPhoto]', function (e) {

    e.preventDefault();

    var imgId = $(this).data('id');
    var imgUrl = $(this).data('url');
    var modalBody = $(this).closest('.modal-body');

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
})