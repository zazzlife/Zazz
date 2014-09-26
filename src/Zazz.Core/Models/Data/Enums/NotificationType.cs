namespace Zazz.Core.Models.Data.Enums
{
    public enum NotificationType : byte
    {
        FollowRequestAccepted = 1,
        CommentOnPhoto = 2,
        CommentOnPost = 3,
        CommentOnEvent = 4,
        NewEvent = 5,
        WallPost = 6,
        TagNotification = 7,
        TagPhotoNotification = 8
    }
}