//var eventLargeImg;

//$(document).on('mouseenter', '.event', function () {
//    var self = $(this);

//    var img = self.data('img');
//    if (!img) {
//        return;
//    }

//    var imgMaxHeight = 300;
//    var offset = self.offset();
//    var elemWidth = self.width();
//    var elemHeight = self.height();
//    var top, left;
//    var side = self.parent().parent().attr('data-eventImgSide');
    
//    var centerY = offset.top + (elemHeight / 2);

//    eventLargeImg = $('<img />', {
//        src: img,
//    });

//    eventLargeImg.css({
//        'position': 'absolute',
//        'max-height': imgMaxHeight + 'px'
//    });

//    eventLargeImg.appendTo($('article')).hide();

//    eventLargeImg.ensureLoad(function () {
        
//        if (side == "left") {
//            self.addClass('event-arrow-left');
//            left = offset.left - eventLargeImg.width() - 15;
//        } else {
//            self.addClass('event-arrow-right');
//            left = offset.left + elemWidth + 30;
//        }

//        top = (centerY - eventLargeImg.height() / 2);

//        eventLargeImg.css({
//            'left': left,
//            'top': top
//        });

//        eventLargeImg.show();
//    });
//});

//$(document).on('mouseleave', '.event', function () {
//    $(this).removeClass('event-arrow-right');
//    $(this).removeClass('event-arrow-left');

//    if (eventLargeImg) {
//        eventLargeImg.remove();
//    }
    
//});

///////////////////// Flip /////////////////////////

//var flipDuration = 250;

//function flipToBack(elem, front, back) {

//    $(elem).transition({
//        perspective: '1000px',
//        rotateX: '90deg',
//        duration: flipDuration,
//        complete: function () {
//            front.hide();
//            back.show();
//        }
//    }).transition({
//        perspective: '1000px',
//        rotateX: '-90deg',
//        duration: 0
//    }).transition({
//        perspective: '1000px',
//        rotateX: '0deg',
//        duration: flipDuration,
//        complete: function () {
//            $(elem).data('current-side', 'back');
//        }
//    });
//}

//function flipToFront(elem, front, back) {

//    $(elem).transition({
//        perspective: '1000px',
//        rotateX: '-90deg',
//        duration: flipDuration,
//        complete: function () {
//            front.show();
//            back.hide();
//        }
//    }).transition({
//        perspective: '1000px',
//        rotateX: '90deg',
//        duration: 0
//    }).transition({
//        perspective: '1000px',
//        rotateX: '0deg',
//        duration: flipDuration,
//        complete: function () {
//            $(elem).data('current-side', 'front');
//        }
//    });
//}

//$(document).on('click', '.event', function (e) {


//    var self = this;

//    var side = $(self).data('current-side');
//    var front = $(self).children('*[data-side="front"]');
//    var back = $(self).children('*[data-side="back"]');

//    if (side == "front") {
//        flipToBack(self, front, back);

//    } else {
//        flipToFront(self, front, back);
//    }

//});

//$(document).on('click', '.event a', function (e) {
//    e.stopPropagation();
//});