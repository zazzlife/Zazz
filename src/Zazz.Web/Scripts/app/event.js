
var flipDuration = 250;

function flipToBack(elem, front, back) {

    $(elem).transition({
        perspective: '1000px',
        rotateX: '90deg',
        duration: flipDuration,
        complete: function () {
            front.hide();
            back.show();
        }
    }).transition({
        perspective: '1000px',
        rotateX: '-90deg',
        duration: 0
    }).transition({
        perspective: '1000px',
        rotateX: '0deg',
        duration: flipDuration,
        complete: function () {
            $(elem).data('current-side', 'back');
        }
    });
}

function flipToFront(elem, front, back) {

    $(elem).transition({
        perspective: '1000px',
        rotateX: '-90deg',
        duration: flipDuration,
        complete: function () {
            front.show();
            back.hide();
        }
    }).transition({
        perspective: '1000px',
        rotateX: '90deg',
        duration: 0
    }).transition({
        perspective: '1000px',
        rotateX: '0deg',
        duration: flipDuration,
        complete: function () {
            $(elem).data('current-side', 'front');
        }
    });
}

$(document).on('click', '.event', function (e) {


    var self = this;

    var side = $(self).data('current-side');
    var front = $(self).children('*[data-side="front"]');
    var back = $(self).children('*[data-side="back"]');

    if (side == "front") {
        flipToBack(self, front, back);

    } else {
        flipToFront(self, front, back);
    }

});

$(document).on('click', '.event a', function (e) {
    e.stopPropagation();
});