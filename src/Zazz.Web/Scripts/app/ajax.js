var DEFAULT_ERROR_MESSAGE = "An error occured, Please try again later.";
var REMOVE_PHOTO_URL = "/photo/remove/";
var REMOVE_ALBUM_URL = "/album/remove/";
var UPDATE_PROFILE_PIC_URL = "/user/ChangeProfilePic/";
var UPDATE_COVER_PIC_URL = "/user/ChangeCoverPic/";

/*****************************
        REMOVE PHOTO
******************************/
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

/*****************************
        REMOVE ALBUM
******************************/
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

/*****************************
    UPDATE PROFILE PICTURE
******************************/
function updateProfilePic(picId) {
    $.ajax({
        url: UPDATE_PROFILE_PIC_URL,
        data: {
            id: picId
        },
        error: function() {
            toastr.error(DEFAULT_ERROR_MESSAGE);
        }
    });
}

/*****************************
    UPDATE COVER PICTURE
******************************/
function updateCoverPic(picId) {
    $.ajax({
        url: UPDATE_COVER_PIC_URL,
        data: {
            id: picId
        },
        error: function () {
            toastr.error(DEFAULT_ERROR_MESSAGE);
        }
    });
}