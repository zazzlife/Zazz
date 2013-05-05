---------------------------------------------------
-- PhotoComments
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_PhotoComments_DELETE
ON dbo.PhotoComments
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Comments WHERE Id IN(SELECT CommentId FROM deleted);
END