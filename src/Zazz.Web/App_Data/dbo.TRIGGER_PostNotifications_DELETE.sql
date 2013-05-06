---------------------------------------------------
-- PostNotifications
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_PostNotifications_DELETE
ON dbo.PostNotifications
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Notifications WHERE Id IN(SELECT NotificationId FROM deleted);
END