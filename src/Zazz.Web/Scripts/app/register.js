$('#_AccountType_User').click(function () {
    $('#clubDetails').slideUp('fast');
});

$('#_AccountType_ClubAdmin').click(function () {
    $('#clubDetails').slideDown('fast');
});

function isUsernameAvailable(username) {

    var url = "/account/isAvailable";
    var dff = $.Deferred();

    $.ajax({
        url: url,
        data: {
            username: username
        },
        success: function(res) {

            if (res.toLowerCase() == "false") {
                dff.resolve(false);
            } else {
                dff.resolve(true);
            }
        }
    });

    return dff.promise();
}

$(document).on('change', 'input[data-checkusername]', function(e) {

    var self = $(this);
    var username = self.val();
    if (!username) {
        return;
    }

    var container = self.closest('.control-group');
    var help = container.find('.help-inline');
    
    var usrNamePromise = isUsernameAvailable(username);

    usrNamePromise.done(function(isAvailable) {

        if (isAvailable) {
            container.removeClass('error');
            container.addClass('success');

            help.text('Available!');

        } else {
            container.removeClass('success');
            container.addClass('error');

            help.text('Username is not available');

        }
        

    });

})