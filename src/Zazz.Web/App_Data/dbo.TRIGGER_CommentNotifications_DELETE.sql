---------------------------------------------------
-- CommentNotifications
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_CommentNotifications_DELETE
ON dbo.CommentNotifications
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Notifications WHERE Id IN(SELECT NotificationId FROM deleted);
END