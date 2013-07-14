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

function profilePhotoSelected(photoId, photoUrl) {
    updateUserPhotoId(photoId);
    var cropLink = '/photos/crop/' + photoId + '?for=' + photoType;
    $('#cropLink').attr('href', cropLink);
    $('#cropPromptModal').modal('show');
}

$(document).on('click', '#cropLink', function() {
    $('#cropPromptModal').modal('hide');
});

$(document).on('click', '.selectProfilePic a', function () {
    onProfilePicChangeClick();
});

$(document).on('click', '.selectCoverPic a', function () {
    onCoverPicChangeClick();
});