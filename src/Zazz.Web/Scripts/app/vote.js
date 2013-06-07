function isUserVoted(id, btn) {
    
}

function getVotesCount(id) {
    
}

function loadVoteButtons() {

    $('button[data-vote-btn]').each(function() {

        var $self = $(this);

        var isLoaded = $self.data('isloaded');
        if (isLoaded) {
            return;
        }
        
        $self.css('opacity', '0.6');



    });

}