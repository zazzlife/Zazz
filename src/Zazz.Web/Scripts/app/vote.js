function showVoteBtn($btn, id) {
    
    var voteHtml = '<i class="icon-thumbs-up"></i> Vote';
    var actionUrl = "/vote/add/" + id;
    $btn.html(voteHtml);

    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'add');
}

function showRemoveVoteBtn($btn, id) {

    var actionUrl = "/vote/remove/" + id;
    var removeVoteHtml = '<i class="icon-thumbs-down"></i> Remove vote';

    $btn.html(removeVoteHtml);
    
    $btn.attr('data-url', actionUrl);
    $btn.removeClass('disabled');
    $btn.removeAttr('disabled');
    $btn.attr('data-isloaded', '1');
    $btn.css('opacity', '1');
    $btn.attr('data-action', 'remove');
}

$(document).on('click', 'button[data-vote-btn]', function () {

    var $self = $(this);
    var isLoaded = $self.attr('data-isloaded');
    if (!isLoaded) {
        return;
    }

    $self.css('opacity', '0.4');
    var url = $self.attr('data-url');
    var id = $self.attr('data-id');

    var action = $self.attr('data-action');

    $.ajax({
        url: url,
        cache: false,
        error: function() {
            toastr.error("An error occured, please try again later");
            $self.css('opacity', '1');
        },
        success: function () {
            var votesContainer = $('#photoVotesCount-' + id);
            var votesCount = parseInt(votesContainer.text()
                .replace(')', '')
                .replace('(', ''));

            if (action == 'remove') {
                showVoteBtn($self, id);
                votesCount--;
            } else {
                showRemoveVoteBtn($self, id);
                votesCount++;
            }

            votesContainer.text('(' + votesCount + ')');
        }
    });

});

function loadVoteAction(id, $btn) {
    
    var url = "/vote/exists/" + id;

    $.ajax({
        url: url,
        cache: false,
        success: function(res) {
            
            if (res.toLowerCase() == 'true') {
                showRemoveVoteBtn($btn, id);
            } else {
                showVoteBtn($btn, id);
            }
        }
    });

}

function getVotesCount(id) {

    var url = "/vote/count/" + id;
    var votesContainer = $('#photoVotesCount-' + id);


    $.ajax({
        url: url,
        cache: false,
        error: function() {
            toastr.error("Failed to get the votes count");
            votesContainer.fadeOut('slow');
        },
        success: function(res) {
            votesContainer.html();
            votesContainer.text('(' + res + ')');
        }
    });
}

function loadVoteButtons() {

    $('button[data-vote-btn]').each(function() {

        var $self = $(this);

        var isLoaded = $self.attr('data-isloaded');
        if (isLoaded == "1") {
            return;
        }
        
        $self.css('opacity', '0.4');
        var id = $self.attr('data-id');

        getVotesCount(id);
        loadVoteAction(id, $self);

    });
}