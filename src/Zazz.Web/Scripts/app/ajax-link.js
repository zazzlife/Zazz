/*
    Parameters:

    data-ajax-request="1" (enabling it)
    data-container="#containerId" (id of the container to update)


*/

$(document).on('cick', '*[data-ajax-request]', function (e) {
    e.preventDefault();


    var self = $(this);
    var url = self.data('url');
    var container = $(self.data('container'));

    container.css('opacity', '0.6');

    $.ajax({
        url: url,
        error: function() {
            toastr.error('An error occured');
        },
        success: function(res) {
            container.fadeOut(function() {
                container.html(res);
                container.fadeIn();
            });
        }
    });
})