/** Will call some functions from global.js  **/

$('#pg-modalWithCrop').on('show', function () {
    
    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function (res) {
        loadPGPhotos();
    });
    
})

$(document).on('click', '#pg-modalWithCrop button[data-selectPhoto]', function (e) {

    e.preventDefault();

    var id = $(this).data('id');
    var imgUrl = $(this).data('url');

    var html = '<input id="photoId" type="hidden" value="' + id + '" />';
    html += '<img src="' + imgUrl + '" />';
    html += '<button style="margin-top:10px;" id="cropBtn" type="button" class="btn btn-info">Crop</button>';

    var modalBody = $(this).closest('.modal-body');
    modalBody.slideUp(function() {
        modalBody.html(html);
        modalBody.slideDown();
        

    });    


});
