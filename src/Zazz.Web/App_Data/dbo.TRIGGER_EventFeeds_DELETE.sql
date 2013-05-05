---------------------------------------------------
-- EventFeeds
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_EventFeeds_DELETE
ON dbo.EventFeeds
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Feeds WHERE Id IN(SELECT FeedId FROM deleted);
END