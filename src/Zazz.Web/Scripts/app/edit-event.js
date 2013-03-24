$('#uploadEventPicModal').on('show', function () {
    loadAlbumsDropDownAsync(document.getElementById("upload-albumSelect"));

    initImgUploader(function (id, name, response) {
        if (!response.success) {
            toastr.error(response.error);
        } else {
            $('#photoId').val(response.photoId);
            $('#selectedImg-thumbnail').attr('src', response.photoUrl);
            $('#uploadEventPicModal').modal('hide');

            if (imgUploadBtn) {
                hideBtnBusy(imgUploadBtn, "Upload");
            }
        }
    });
});