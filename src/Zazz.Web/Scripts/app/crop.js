$('#cropImg').ensureLoad(function () {

    var imgElem = document.getElementById("cropImg");
    var height = imgElem.naturalHeight;
    var width = imgElem.naturalWidth;
    var aspectRatio = $('#aspectRatio').val();

    var x = $('#x');
    var x2 = $('#x2');
    var y = $('#y');
    var y2 = $('#y2');
    var w = $('#w');
    var h = $('#h');

    $(imgElem).Jcrop({
        trueSize: [width, height],
        aspectRatio: aspectRatio,
        onChange: function (c) {
            console.log(c);
            x.val(c.x);
            x2.val(c.x2);
            y.val(c.y);
            y2.val(c.y2);
            w.val(c.w);
            h.val(c.h);
        }
    });

});
