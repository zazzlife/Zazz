var eventLargeImg;

$(document).on('mouseenter', '.event', function () {
    var self = $(this);

    var img = self.data('img');
    if (!img) {
        return;
    }

    

    var imgHeight = 300;
    var offset = self.offset();
    var elemWidth = self.width();
    var elemHeight = self.height();
    var top, left;

    

    var centerY = offset.top + (elemHeight / 2);
    top = centerY - (imgHeight / 2);

    var side = self.parent().parent().attr('data-eventImgSide');
    if (side == "left") {
        
        self.addClass('event-arrow-left');

    } else {
        
        self.addClass('event-arrow-right');
    }

    left = offset.left + elemWidth + 30;

    eventLargeImg = $('<img />', {
        src: img,
    });

    eventLargeImg.css({
        'position': 'absolute',
        'height': imgHeight + 'px',
        'top': top,
        'left': left
    });

    eventLargeImg.appendTo($('article'));
});

$(document).on('mouseleave', '.event', function () {
    $(this).removeClass('event-arrow-right');
    $(this).removeClass('event-arrow-left');
    eventLargeImg.remove();
});

/////////////////// Flip /////////////////////////

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