---------------------------------------------------
-- PhotoNotifications
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_PhotoNotifications_DELETE
ON dbo.PhotoNotifications
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Notifications WHERE Id IN(SELECT NotificationId FROM deleted);
END