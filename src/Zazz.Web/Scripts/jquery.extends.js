http://stackoverflow.com/questions/5624733/jquery-load-not-firing-on-images-probably-caching
jQuery.fn.extend({
    ensureLoad: function(handler) {
        return this.each(function() {
            if(this.complete) {
                handler.call(this);
            } else {
                $(this).load(handler);
            }
        });
    }
});