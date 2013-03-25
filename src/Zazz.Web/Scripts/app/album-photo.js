$(document).on('click', '.remove-photo-btn', function() {

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

})