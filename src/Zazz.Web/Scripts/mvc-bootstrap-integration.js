/*http://www.joe-stevens.com/2013/01/23/twitter-bootstrap-validation-styles-with-asp-net-mvc/comment-page-1/#comment-22194*/

jQuery.validator.setDefaults({
    highlight: function (element, errorClass, validClass) {
        if (element.type === 'radio') {
            this.findByName(element.name).addClass(errorClass).removeClass(validClass);
        } else {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).closest('.control-group').removeClass('success').addClass('error');
        }
    },
    unhighlight: function (element, errorClass, validClass) {
        if (element.type === 'radio') {
            this.findByName(element.name).removeClass(errorClass).addClass(validClass);
        } else {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).closest('.control-group').removeClass('error').addClass('success');
        }
    }
});