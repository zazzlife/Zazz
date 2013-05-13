function eventPhotoSelected(photoId, photoUrl) {
    $('#photoId').val(photoId);
    $('#selectedImg-thumbnail').attr('src', photoUrl);
}