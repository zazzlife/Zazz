$(function () {

    var container = $('*[data-scroll-pagination]');
    if (!container) {
        return;
    }

    var url = container.data('url');
	if (!url) {
		return;
	}
	
    var isLoading = false;

    $(window).scroll(function() {
        if (!isLoading && ($(window).scrollTop() >  $(document).height() - $(window).height()- 100)) {
            
			isLoading = true;
            var currentPage = container.data('page');
			var clearfix = container.data('clearfix');
            currentPage++;

			$.ajax({
				url: url,
				data: {
					page: currentPage
				},
				error: function () {
					toastr.error('Failed to load more data');
				},
				success: function (res) {
					var data = $(res.trim());
					
					if (clearfix) {
						var oldClearfix = container.children('.clearfix');
						oldClearfix.remove();
					}
					
					data.appendTo(container);
					
					var clear = $('<div class="clearfix"></div>');
					clear.appendTo(container);

				    container.data('page', currentPage);

				    isLoading = false;
				}
			});
        }
    });

    
    


});
