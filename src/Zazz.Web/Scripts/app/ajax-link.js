/*
    Parameters:

    data-ajax="1" (enabling it)
    data-container="#containerId" (id of the container to update)


*/

$(document).on('click', 'a[data-ajax]', function (e) {
    e.preventDefault();


    var self = $(this);
    
    var url = self.attr('href');
    if (!url) {
        console.log('undefined url');
        return;
    }
    
    var containerId = self.data('container');
    if (!containerId) {
        console.log('undefined container');
        return;
    }

    var container = $(containerId);

    container.css('opacity', '0.5');

    $.ajax({
        url: url,
        error: function() {
            toastr.error('An error occured');
            container.css('opacity', '1');
        },
        success: function (res) {
            container.css('opacity', '1');
            container.html(res);
        }
    });
})