function initInputTags() {
    if (availableTags) {

        $('*[data-tag]').each(function () {
            $(this).atwho("#", {
                data: availableTags
            });
        });

    }
}