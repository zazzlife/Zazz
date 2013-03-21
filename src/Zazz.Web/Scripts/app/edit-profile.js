/** Will call some functions from global.js  **/

var cropSize;
var cropPhotoId;

function onCropSelectionChange(c) {
    cropSize = c;
}

$('#pg-modalWithCrop').on('show', function() {

    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function(res) {
        loadPGPhotos();
    });

});

$(document).on('click', '#pg-modalWithCrop button[data-selectPhoto]', function (e) {

    e.preventDefault();

    cropPhotoId = $(this).data('id');
    var imgUrl = $(this).data('url');

    var html = '<img id="cropImg" src="' + imgUrl + '" />';
    html += '<button style="margin-top:10px;" id="cropBtn" type="button" class="btn btn-info">Crop</button>';

    var modalBody = $(this).closest('.modal-body');
    modalBody.slideUp(function() {
        modalBody.html(html);
        modalBody.slideDown();

        $('#cropImg').Jcrop({
            boxWidth: 530,
            onChange: onCropSelectionChange,
            aspectRatio: 10/3
        });
    });
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