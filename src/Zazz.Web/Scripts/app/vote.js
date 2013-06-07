function showVoteOrUnvoteBtn(id, btn) {
    
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
        
        $self.css('opacity', '0.6');
        var id = $self.data('id');

        getVotesCount(id);
        showVoteOrUnvoteBtn(id, $self);

    });

}