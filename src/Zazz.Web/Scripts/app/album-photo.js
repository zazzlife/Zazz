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

$(document).on('click', '.remove-photo-btn', function () {

    var self = $(this);
    var url = self.data('url');
    var btnIcon = '<i class="icon-remove"></i>';
    self.html('');
    showBtnBusy(self);


    $.ajax({
        url: url,
        cache: false,
        error: function() {
            toastr.error("An error occured, Please try again later.");
            hideBtnBusy(self, btnIcon);
        },
        success: function() {
            self.closest('li').fadeOut();
        }
    });
});

$(document).on('click', '#createAlbumBtn', function (e) {

    e.preventDefault();

    var self = $(this);
    var albumName = $('#AlbumName').val();
    
    if (!albumName) {
        toastr.error("Album name cannot be empty");
        return;
    }

    var btnText = showBtnBusy(self);

    $.ajax({
        url: '/album/CreateAlbum',
        cache: false,
        data: {
            albumName: albumName
        },
        type: 'POST',
        error: function() {
            toastr.error('An error occured, Please try again later.');
            hideBtnBusy(self, btnText);
        },
        success: function (res) {

            var album = $(res.trim());
            album.css('opacity', '0');
            album.appendTo('ul.thumbnails');

            // fadeIn just doesnt work!
            var opacity = 0;
            var fadeIn = setInterval(function() {
                opacity += 0.1;

                album.css('opacity', opacity);
                
                if (opacity >= 1) {
                    clearInterval(fadeIn);
                }

            }, 4);


            hideBtnBusy(self, btnText);
        }
    });

});