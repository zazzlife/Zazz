var REMOVE_PHOTO_URL = "/photo/remove/";
var REMOVE_ALBUM_URL = "/album/remove/";

function removePhoto(photoId, errorCallback, successCallback) {
    if (!photoId) {
        alert("Photo id cannot be 0");
        return;
    }
    
    var url = REMOVE_PHOTO_URL + photoId;

    $.ajax({
        url: url,
        cache: false,
        error: errorCallback,
        success: successCallback
    });
}

function removeAlbum(albumId, errorCallback, successCallback) {
    if (!albumId) {
        alert("Album id cannot be 0");
        return;
    }

    var url = REMOVE_ALBUM_URL + albumId;
    $.ajax({
        url: url,
        cache: false,
        error: errorCallback,
        success: successCallback
    });
}