/********************************
    wrapping tags in anchortags
*********************************/

function highlightTags() {

    $('*[data-containsTags]').each(function () {

        var self = $(this);
        var text = self.html();

        var allTags = text.match(/#\w*/gi);
        if (!allTags) {
            return;
        }


        for (var i = 0; i < allTags.length; i++) {
            var tag = allTags[i];
            var highlighted = '<strong>' + tag + '</strong>';

            text = text.replace(tag, highlighted);
        }

        self.html(text);
    });

}
