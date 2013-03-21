
$('#followsYouSign').hover(
    function mouseIn() {

        $(this).removeClass('label-success');
        $(this).addClass('label-warning');
        $(this).html('Stop follow  <i class="icon icon-remove-sign"></i>');

    }, function mouseOut() {

        $(this).addClass('label-success');
        $(this).removeClass('label-warning');
        $(this).html('Follows you <i class="icon icon-ok-sign"></i>');
        
    });