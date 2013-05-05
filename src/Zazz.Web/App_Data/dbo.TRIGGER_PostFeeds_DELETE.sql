---------------------------------------------------
-- PostFeeds
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_PostFeeds_DELETE
ON dbo.PostFeeds
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Feeds WHERE Id IN(SELECT FeedId FROM deleted);
END