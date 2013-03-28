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

$(document).on('click', '#linkPageBtn', function () {

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
            parent.html('<button id="unlinkPageBtn" data-id="' + pageId + '" class="btn btn-danger">Un-link</button>');
        }
    });

});

$(document).on('click', '#unlinkPageBtn', function () {

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
            hideBtnBusy(self, "Un-Link");
        },
        success: function () {
            toastr.success("This page has been successfully Un-Linked!");
            var parent = self.parent();
            parent.html('<button id="linkPageBtn" data-id="' + pageId + '" class="btn btn-success">Link</button>');
        }
    });

});