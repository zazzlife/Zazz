/********************************
    wrapping tags in anchortags
*********************************/

function replaceTagsWithAnchorTags() {
    if (availableTags) {

        var TAGS_REGEX = "#[a-zA-z0-9-]+";

        $('*[data-containsTags]').each(function () {

            var self = $(this);
            var text = self.html();

            var allTags = text.match(TAGS_REGEX);
            var baseAddress = "/home/tags?select=";

            for (var i = 0; i < allTags.length; i++) {
                var tag = allTags[i];
                var tagWithoutHash = tag.replace("#", "");

                var exists = false;
                for (var j = 0; j < availableTags.length; j++) {
                    var availTag = availableTags[j].toLowerCase();

                    if (tagWithoutHash.toLowerCase() === availTag) {
                        exists = true;
                    }
                }

                if (exists) {
                    var url = baseAddress + tagWithoutHash;
                    var anchorTag = '<a class="tag" href="' + url + '">' + tag + '</a>';

                    text = text.replace(tag, anchorTag);
                }
            }

            self.html(text);
        });

    }
}
