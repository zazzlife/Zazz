///////////PHOTOS////////////

$(document).on('mouseenter', '.pic-list .img', function () {

    var self = $(this);
    var removeBtn = self.children('.remove-img');
    removeBtn.fadeIn('fast');

});

$(document).on('mouseleave', '.pic-list .img', function () {

    var self = $(this);
    var removeBtn = self.children('.remove-img');
    removeBtn.fadeOut('fast');

});

$(document).on('click', '.remove-img', function () {
    var self = $(this);
    var parent = self.parent();
    var id = self.data('id');

    parent.css('opacity', '0.7');

    removePhoto(id,
        function () {
            toastr.error('An error occured, Please try again later');
            parent.css('opacity', '1');
        },
        function () {
            parent.fadeOut();
        });

});

///////////ALBUMS////////////

$(document).on('mouseenter', '.album-thumbnail .img', function () {

    var self = $(this);
    var removeBtn = self.children('.remove-album');
    removeBtn.fadeIn('fast');

});

$(document).on('mouseleave', '.album-thumbnail .img', function () {

    var self = $(this);
    var removeBtn = self.children('.remove-album');
    removeBtn.fadeOut('fast');

});

$(document).on('click', '.remove-album', function () {
    var self = $(this);
    var parent = self.parent().parent();
    var id = self.data('id');

    parent.css('opacity', '0.7');

    removeAlbum(id,
        function () {
        toastr.error('An error occured, Please try again later');
        parent.css('opacity', '1');
    },
        function () {
            parent.fadeOut();
        });

});