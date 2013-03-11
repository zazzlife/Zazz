$(function() {
    $('#fileupload').fileupload({ autoUpload: true });

    $('#fileupload').bind('fileuploadsend', function (e, data) {
        alert('send');
    });
    
    $('#fileupload').bind('fileuploaddone', function (e, data) {
        alert('done');
    });
    
    $('#fileupload').bind('fileuploadfail', function (e, data) {
        alert('fail');
    });
})