---------------------------------------------------
-- EventComments
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_EventComments_DELETE
ON dbo.EventComments
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Comments WHERE Id IN(SELECT CommentId FROM deleted);
END
GO