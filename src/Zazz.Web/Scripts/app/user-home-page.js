$(document).on('click', '#manageFbPages', function() {

    var url = "/facebook/getpages";

    $.ajax({
        url: url,
        error: function() {
            toastr.error('An error occured, Please try again later.');
        },
        success: function(res) {
            var container = $('#manageFbPagesModal .modal-body');
            container.fadeOut(function() {
                container.html(res);
                container.fadeIn();
            });

        }
    });

});

$(document).on('click', '.linkPageBtn', function () {

    var self = $(this);
    var url = '/facebook/linkpage';
    var pageId = self.data('id');

    showBtnBusy(self);

    $.ajax({
        url: url,
        data: {
            pageId: pageId
        },
        error: function(res) {
            showAjaxErrorMessage(res);
            hideBtnBusy(self, "Link");
        },
        success: function() {
            toastr.success("This page has been successfully linked!");
            var parent = self.parent();
            parent.html('<button style="width: 85px;" data-id="' + pageId + '" class="btn btn-danger unlinkPageBtn">Unlink</button>');
            var syncBtn = parent.next().children('button');
            syncBtn.removeClass('disabled');
        }
    });

});

$(document).on('click', '.unlinkPageBtn', function () {

    var self = $(this);
    var url = '/facebook/unlinkpage';
    var pageId = self.data('id');

    showBtnBusy(self);

    $.ajax({
        url: url,
        data: {
            pageId: pageId
        },
        error: function () {
            toastr.error('An error occured, Please try again later.');
            hideBtnBusy(self, "Unlink");
        },
        success: function () {
            toastr.success("This page has been successfully Unlinked!");
            var parent = self.parent();
            parent.html('<button style="width: 85px;" data-id="' + pageId + '" class="btn btn-success linkPageBtn">Link</button>');
            var syncBtn = parent.next().children('button');
            syncBtn.addClass('disabled');
        }
    });
});

$(document).on('click', '.syncPageBtn', function () {

    var self = $(this);
    var url = '/facebook/syncpage';
    var pageId = self.data('id');
    
    var text = showBtnBusy(self);

    $.ajax({
        url: url,
        data: {
            pageId: pageId
        },
        error: function() {
            toastr.error('An error occured, please try again later.');
            hideBtnBusy(self, text);
        },
        success: function() {
            toastr.success('Sync has been successful, please refresh the page to see the changes.');
            hideBtnBusy(self, text);
        }
    });

});