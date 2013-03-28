$(document).on('click', '#manageFbPages', function() {

    var url = "/facebook/getpages";

    $.ajax({
        url: url,
        error: function() {
            toastr.error('An error occured, Please try again later.');
        },
        success: function(res) {
            var container = $('#manageFbPagesModal .modal-body');
            container.html(res);
        }
    });

})