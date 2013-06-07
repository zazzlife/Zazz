function showVoteOrUnvoteBtn(id, $btn) {
    
    var url = "/vote/exists/" + id;

    var removeVoteHtml = '<i class="icon-thumbs-down"></i> Remove vote';
    var removeVoteUrl = "/vote/remove/" + id;
    var voteUrl = "/vote/add/" + id;

    var actionUrl;

    $.ajax({
        url: url,
        cache: false,
        success: function(res) {
            
            if (res.toLowerCase() == 'true') {
                $btn.html(removeVoteHtml);
                actionUrl = removeVoteUrl;
            } else {
                actionUrl = voteUrl;
            }

            $btn.attr('data-url', actionUrl);
            $btn.removeClass('disabled');
            $btn.removeAttr('disabled');
            $btn.attr('data-isloaded', '1');
            $btn.css('opacity', '1');
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

        var isLoaded = $self.data('isloaded');
        if (isLoaded) {
            return;
        }
        
        $self.css('opacity', '0.4');
        var id = $self.data('id');

        getVotesCount(id);
        showVoteOrUnvoteBtn(id, $self);

    });

}