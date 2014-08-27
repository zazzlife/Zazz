namespace Zazz.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Majors",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Name = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Schools",
                c => new
                    {
                        Id = c.Short(nullable: false, identity: true),
                        Name = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Name = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OAuthScopes",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Name = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Username = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 200),
                        Password = c.String(maxLength: 50),
                        AccountType = c.Byte(nullable: false),
                        LastActivity = c.DateTime(nullable: false),
                        JoinedDate = c.DateTime(nullable: false, storeType: "date"),
                        IsConfirmed = c.Boolean(nullable: false),
                        ProfilePhotoId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserPreferences", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.UserValidationTokens",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Token = c.Binary(),
                        ExpirationTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.LinkedAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ProviderUserId = c.Long(nullable: false),
                        AccessToken = c.String(maxLength: 4000),
                        Provider = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserPreferences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SyncFbEvents = c.Boolean(nullable: false),
                        SyncFbPosts = c.Boolean(nullable: false),
                        SyncFbImages = c.Boolean(nullable: false),
                        SendSyncErrorNotifications = c.Boolean(nullable: false),
                        LastSyncError = c.DateTime(),
                        LasySyncErrorEmailSent = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserDetails",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        FullName = c.String(maxLength: 100),
                        Gender = c.Byte(nullable: false),
                        SchoolId = c.Short(),
                        MajorId = c.Byte(),
                        CityId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Schools", t => t.SchoolId)
                .ForeignKey("dbo.Majors", t => t.MajorId)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .ForeignKey("dbo.Users", t => t.Id)
                .Index(t => t.SchoolId)
                .Index(t => t.MajorId)
                .Index(t => t.CityId)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ClubDetails",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ClubName = c.String(maxLength: 500),
                        ClubTypesBits = c.Int(nullable: false),
                        Address = c.String(maxLength: 500),
                        CoverPhotoId = c.Int(),
                        SchoolId = c.Short(),
                        CityId = c.Int(),
                        ShowSync = c.Boolean()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Schools", t => t.SchoolId)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .ForeignKey("dbo.Users", t => t.Id)
                .Index(t => t.SchoolId)
                .Index(t => t.CityId)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.UserReceivedLikes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        Count = c.Int(nullable: false),
                        LastUpdate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Weeklies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        DayOfTheWeek = c.Byte(nullable: false),
                        Name = c.String(maxLength: 100),
                        PhotoId = c.Int(),
                        Description = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Photos", t => t.PhotoId)
                .Index(t => t.UserId)
                .Index(t => t.PhotoId);
            
            CreateTable(
                "dbo.Photos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Description = c.String(maxLength: 4000),
                        AlbumId = c.Int(),
                        IsFacebookPhoto = c.Boolean(nullable: false),
                        FacebookId = c.String(maxLength: 4000),
                        UploadDate = c.DateTime(nullable: false),
                        PageId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Albums", t => t.AlbumId)
                .ForeignKey("dbo.FacebookPages", t => t.PageId)
                .Index(t => t.UserId)
                .Index(t => t.AlbumId)
                .Index(t => t.PageId);
            
            CreateTable(
                "dbo.Albums",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        UserId = c.Int(nullable: false),
                        IsFacebookAlbum = c.Boolean(nullable: false),
                        FacebookId = c.String(maxLength: 4000),
                        PageId = c.Int(),
                        CreatedDate = c.DateTime(nullable: false, storeType: "date"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.FacebookPages", t => t.PageId)
                .Index(t => t.UserId)
                .Index(t => t.PageId);
            
            CreateTable(
                "dbo.FacebookPages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Name = c.String(maxLength: 4000),
                        FacebookId = c.String(maxLength: 4000),
                        AccessToken = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Photo_Categories",
                c => new
                    {
                        CategoryId = c.Byte(nullable: false),
                        PhotoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CategoryId, t.PhotoId })
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Photos", t => t.PhotoId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.PhotoId);
            
            CreateTable(
                "dbo.Follows",
                c => new
                    {
                        ToUserId = c.Int(nullable: false),
                        FromUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ToUserId, t.FromUserId })
                .ForeignKey("dbo.Users", t => t.ToUserId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.FromUserId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 4000),
                        Description = c.String(),
                        IsDateOnly = c.Boolean(nullable: false),
                        IsFacebookEvent = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        FacebookEventId = c.Long(),
                        FacebookPhotoLink = c.String(maxLength: 4000),
                        PhotoId = c.Int(),
                        Time = c.DateTimeOffset(nullable: false),
                        TimeUtc = c.DateTime(nullable: false),
                        Location = c.String(maxLength: 4000),
                        Street = c.String(maxLength: 4000),
                        City = c.String(maxLength: 4000),
                        Latitude = c.Single(),
                        Longitude = c.Single(),
                        Price = c.Single(),
                        PageId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.FacebookPages", t => t.PageId)
                .Index(t => t.UserId)
                .Index(t => t.PageId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FromUserId = c.Int(nullable: false),
                        ToUserId = c.Int(),
                        Message = c.String(),
                        FacebookId = c.Long(nullable: false),
                        PageId = c.Int(),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.FromUserId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.ToUserId)
                .ForeignKey("dbo.FacebookPages", t => t.PageId)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.PageId);
            
            CreateTable(
                "dbo.Post_Categories",
                c => new
                    {
                        CategoryId = c.Byte(nullable: false),
                        PostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CategoryId, t.PostId })
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.PostId);

            CreateTable(
                "dbo.Post_Tags",
                c => new
                    {
                        PostId = c.Int(nullable: false),
                        ClubId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PostId, t.ClubId })
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.ClubId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Message = c.String(maxLength: 4000),
                        Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PhotoComments",
                c => new
                    {
                        CommentId = c.Int(nullable: false),
                        PhotoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Photos", t => t.PhotoId, cascadeDelete: true)
                .ForeignKey("dbo.Comments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.PhotoId)
                .Index(t => t.CommentId);
            
            CreateTable(
                "dbo.PostComments",
                c => new
                    {
                        CommentId = c.Int(nullable: false),
                        PostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Comments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.CommentId);
            
            CreateTable(
                "dbo.EventComments",
                c => new
                    {
                        CommentId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Comments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.EventId)
                .Index(t => t.CommentId);
            
            CreateTable(
                "dbo.FollowRequests",
                c => new
                    {
                        ToUserId = c.Int(nullable: false),
                        FromUserId = c.Int(nullable: false),
                        RequestDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.ToUserId, t.FromUserId })
                .ForeignKey("dbo.Users", t => t.ToUserId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.FromUserId);
            
            CreateTable(
                "dbo.Feeds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        FeedType = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PostFeeds",
                c => new
                    {
                        FeedId = c.Int(nullable: false),
                        PostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FeedId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Feeds", t => t.FeedId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.FeedId);
            
            CreateTable(
                "dbo.EventFeeds",
                c => new
                    {
                        FeedId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FeedId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Feeds", t => t.FeedId, cascadeDelete: true)
                .Index(t => t.EventId)
                .Index(t => t.FeedId);
            
            CreateTable(
                "dbo.Feed_Photos",
                c => new
                    {
                        FeedId = c.Int(nullable: false),
                        PhotoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.FeedId, t.PhotoId })
                .ForeignKey("dbo.Feeds", t => t.FeedId, cascadeDelete: true)
                .ForeignKey("dbo.Photos", t => t.PhotoId, cascadeDelete: true)
                .Index(t => t.FeedId)
                .Index(t => t.PhotoId);
            
            CreateTable(
                "dbo.Feed_Users",
                c => new
                    {
                        FeedId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.FeedId, t.UserId })
                .ForeignKey("dbo.Feeds", t => t.FeedId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.FeedId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.FacebookSyncRetries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FacebookUserId = c.Long(nullable: false),
                        Path = c.String(maxLength: 4000),
                        Fields = c.String(maxLength: 4000),
                        LastTry = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        UserBId = c.Int(nullable: false),
                        NotificationType = c.Byte(nullable: false),
                        Time = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserBId)
                .Index(t => t.UserId)
                .Index(t => t.UserBId);
            
            CreateTable(
                "dbo.PostNotifications",
                c => new
                    {
                        NotificationId = c.Long(nullable: false),
                        PostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Notifications", t => t.NotificationId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.NotificationId);
            
            CreateTable(
                "dbo.EventNotifications",
                c => new
                    {
                        NotificationId = c.Long(nullable: false),
                        EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Notifications", t => t.NotificationId, cascadeDelete: true)
                .Index(t => t.EventId)
                .Index(t => t.NotificationId);
            
            CreateTable(
                "dbo.CommentNotifications",
                c => new
                    {
                        NotificationId = c.Long(nullable: false),
                        CommentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Comments", t => t.CommentId, cascadeDelete: true)
                .ForeignKey("dbo.Notifications", t => t.NotificationId, cascadeDelete: true)
                .Index(t => t.CommentId)
                .Index(t => t.NotificationId);
            
            CreateTable(
                "dbo.CategoryStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastUpdate = c.DateTime(nullable: false),
                        CategoryId = c.Byte(nullable: false),
                        UsersCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.PhotoLikes",
                c => new
                    {
                        PhotoId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PhotoId, t.UserId })
                .ForeignKey("dbo.Photos", t => t.PhotoId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.PhotoId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OAuthRefreshTokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        OAuthClientId = c.Int(nullable: false),
                        VerificationCode = c.String(maxLength: 4000),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.OAuthClients", t => t.OAuthClientId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.OAuthClientId);
            
            CreateTable(
                "dbo.OAuthRefreshToken_Scopes",
                c => new
                    {
                        RefreshTokenId = c.Int(nullable: false),
                        ScopeId = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => new { t.RefreshTokenId, t.ScopeId })
                .ForeignKey("dbo.OAuthRefreshTokens", t => t.RefreshTokenId, cascadeDelete: true)
                .ForeignKey("dbo.OAuthScopes", t => t.ScopeId, cascadeDelete: true)
                .Index(t => t.RefreshTokenId)
                .Index(t => t.ScopeId);
            
            CreateTable(
                "dbo.OAuthClients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        ClientId = c.String(maxLength: 1000),
                        Secret = c.String(maxLength: 2000),
                        IsAllowedToRequestPasswordGrantType = c.Boolean(nullable: false),
                        IsAllowedToRequestFullScope = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClubRewards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClubId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 1000),
                        Cost = c.Int(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ClubId, cascadeDelete: true)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.UserRewards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RewardId = c.Int(nullable: false),
                        RedeemedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.ClubRewards", t => t.RewardId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RewardId);
            
            CreateTable(
                "dbo.UserPoints",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClubId = c.Int(nullable: false),
                        Points = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.ClubId })
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.ClubId)
                .Index(t => t.UserId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.ClubPointRewardScenarios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClubId = c.Int(nullable: false),
                        Scenario = c.Byte(nullable: false),
                        MondayAmount = c.Int(nullable: false),
                        TuesdayAmount = c.Int(nullable: false),
                        WednesdayAmount = c.Int(nullable: false),
                        ThursdayAmount = c.Int(nullable: false),
                        FridayAmount = c.Int(nullable: false),
                        SaturdayAmount = c.Int(nullable: false),
                        SundayAmount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ClubId, cascadeDelete: true)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.UserPointsHistory",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClubId = c.Int(nullable: false),
                        RewardId = c.Int(nullable: false),
                        ChangedAmount = c.Int(nullable: false),
                        RewardScenario = c.Byte(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.ClubId)
                .ForeignKey("dbo.ClubRewards", t => t.RewardId)
                .Index(t => t.UserId)
                .Index(t => t.ClubId)
                .Index(t => t.RewardId);
            
            CreateTable(
                "dbo.UserRewardsHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        EditorUserId = c.Int(nullable: false),
                        RewardId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.EditorUserId)
                .ForeignKey("dbo.ClubRewards", t => t.RewardId)
                .Index(t => t.UserId)
                .Index(t => t.EditorUserId)
                .Index(t => t.RewardId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserRewardsHistory", new[] { "RewardId" });
            DropIndex("dbo.UserRewardsHistory", new[] { "EditorUserId" });
            DropIndex("dbo.UserRewardsHistory", new[] { "UserId" });
            DropIndex("dbo.UserPointsHistory", new[] { "RewardId" });
            DropIndex("dbo.UserPointsHistory", new[] { "ClubId" });
            DropIndex("dbo.UserPointsHistory", new[] { "UserId" });
            DropIndex("dbo.ClubPointRewardScenarios", new[] { "ClubId" });
            DropIndex("dbo.UserPoints", new[] { "ClubId" });
            DropIndex("dbo.UserPoints", new[] { "UserId" });
            DropIndex("dbo.UserRewards", new[] { "RewardId" });
            DropIndex("dbo.UserRewards", new[] { "UserId" });
            DropIndex("dbo.ClubRewards", new[] { "ClubId" });
            DropIndex("dbo.OAuthRefreshToken_Scopes", new[] { "ScopeId" });
            DropIndex("dbo.OAuthRefreshToken_Scopes", new[] { "RefreshTokenId" });
            DropIndex("dbo.OAuthRefreshTokens", new[] { "OAuthClientId" });
            DropIndex("dbo.OAuthRefreshTokens", new[] { "UserId" });
            DropIndex("dbo.PhotoLikes", new[] { "UserId" });
            DropIndex("dbo.PhotoLikes", new[] { "PhotoId" });
            DropIndex("dbo.CategoryStatistics", new[] { "CategoryId" });
            DropIndex("dbo.CommentNotifications", new[] { "NotificationId" });
            DropIndex("dbo.CommentNotifications", new[] { "CommentId" });
            DropIndex("dbo.EventNotifications", new[] { "NotificationId" });
            DropIndex("dbo.EventNotifications", new[] { "EventId" });
            DropIndex("dbo.PostNotifications", new[] { "NotificationId" });
            DropIndex("dbo.PostNotifications", new[] { "PostId" });
            DropIndex("dbo.Notifications", new[] { "UserBId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.Feed_Users", new[] { "UserId" });
            DropIndex("dbo.Feed_Users", new[] { "FeedId" });
            DropIndex("dbo.Feed_Photos", new[] { "PhotoId" });
            DropIndex("dbo.Feed_Photos", new[] { "FeedId" });
            DropIndex("dbo.EventFeeds", new[] { "FeedId" });
            DropIndex("dbo.EventFeeds", new[] { "EventId" });
            DropIndex("dbo.PostFeeds", new[] { "FeedId" });
            DropIndex("dbo.PostFeeds", new[] { "PostId" });
            DropIndex("dbo.FollowRequests", new[] { "FromUserId" });
            DropIndex("dbo.FollowRequests", new[] { "ToUserId" });
            DropIndex("dbo.EventComments", new[] { "CommentId" });
            DropIndex("dbo.EventComments", new[] { "EventId" });
            DropIndex("dbo.PostComments", new[] { "CommentId" });
            DropIndex("dbo.PostComments", new[] { "PostId" });
            DropIndex("dbo.PhotoComments", new[] { "CommentId" });
            DropIndex("dbo.PhotoComments", new[] { "PhotoId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Post_Categories", new[] { "PostId" });
            DropIndex("dbo.Post_Categories", new[] { "CategoryId" });
            DropIndex("dbo.Post_Tags", new[] { "PostId" });
            DropIndex("dbo.Post_Tags", new[] { "ClubId" });
            DropIndex("dbo.Posts", new[] { "PageId" });
            DropIndex("dbo.Posts", new[] { "ToUserId" });
            DropIndex("dbo.Posts", new[] { "FromUserId" });
            DropIndex("dbo.Events", new[] { "PageId" });
            DropIndex("dbo.Events", new[] { "UserId" });
            DropIndex("dbo.Follows", new[] { "FromUserId" });
            DropIndex("dbo.Follows", new[] { "ToUserId" });
            DropIndex("dbo.Photo_Categories", new[] { "PhotoId" });
            DropIndex("dbo.Photo_Categories", new[] { "CategoryId" });
            DropIndex("dbo.FacebookPages", new[] { "UserId" });
            DropIndex("dbo.Albums", new[] { "PageId" });
            DropIndex("dbo.Albums", new[] { "UserId" });
            DropIndex("dbo.Photos", new[] { "PageId" });
            DropIndex("dbo.Photos", new[] { "AlbumId" });
            DropIndex("dbo.Photos", new[] { "UserId" });
            DropIndex("dbo.Weeklies", new[] { "PhotoId" });
            DropIndex("dbo.Weeklies", new[] { "UserId" });
            DropIndex("dbo.UserReceivedLikes", new[] { "UserId" });
            DropIndex("dbo.ClubDetails", new[] { "Id" });
            DropIndex("dbo.ClubDetails", new[] { "CityId" });
            DropIndex("dbo.ClubDetails", new[] { "SchoolId" });
            DropIndex("dbo.UserDetails", new[] { "Id" });
            DropIndex("dbo.UserDetails", new[] { "CityId" });
            DropIndex("dbo.UserDetails", new[] { "MajorId" });
            DropIndex("dbo.UserDetails", new[] { "SchoolId" });
            DropIndex("dbo.LinkedAccounts", new[] { "UserId" });
            DropIndex("dbo.UserValidationTokens", new[] { "Id" });
            DropIndex("dbo.Users", new[] { "Id" });
            DropForeignKey("dbo.UserRewardsHistory", "RewardId", "dbo.ClubRewards");
            DropForeignKey("dbo.UserRewardsHistory", "EditorUserId", "dbo.Users");
            DropForeignKey("dbo.UserRewardsHistory", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserPointsHistory", "RewardId", "dbo.ClubRewards");
            DropForeignKey("dbo.UserPointsHistory", "ClubId", "dbo.Users");
            DropForeignKey("dbo.UserPointsHistory", "UserId", "dbo.Users");
            DropForeignKey("dbo.ClubPointRewardScenarios", "ClubId", "dbo.Users");
            DropForeignKey("dbo.UserPoints", "ClubId", "dbo.Users");
            DropForeignKey("dbo.UserPoints", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRewards", "RewardId", "dbo.ClubRewards");
            DropForeignKey("dbo.UserRewards", "UserId", "dbo.Users");
            DropForeignKey("dbo.ClubRewards", "ClubId", "dbo.Users");
            DropForeignKey("dbo.OAuthRefreshToken_Scopes", "ScopeId", "dbo.OAuthScopes");
            DropForeignKey("dbo.OAuthRefreshToken_Scopes", "RefreshTokenId", "dbo.OAuthRefreshTokens");
            DropForeignKey("dbo.OAuthRefreshTokens", "OAuthClientId", "dbo.OAuthClients");
            DropForeignKey("dbo.OAuthRefreshTokens", "UserId", "dbo.Users");
            DropForeignKey("dbo.PhotoLikes", "UserId", "dbo.Users");
            DropForeignKey("dbo.PhotoLikes", "PhotoId", "dbo.Photos");
            DropForeignKey("dbo.CategoryStatistics", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.CommentNotifications", "NotificationId", "dbo.Notifications");
            DropForeignKey("dbo.CommentNotifications", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.EventNotifications", "NotificationId", "dbo.Notifications");
            DropForeignKey("dbo.EventNotifications", "EventId", "dbo.Events");
            DropForeignKey("dbo.PostNotifications", "NotificationId", "dbo.Notifications");
            DropForeignKey("dbo.PostNotifications", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Notifications", "UserBId", "dbo.Users");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.Users");
            DropForeignKey("dbo.Feed_Users", "UserId", "dbo.Users");
            DropForeignKey("dbo.Feed_Users", "FeedId", "dbo.Feeds");
            DropForeignKey("dbo.Feed_Photos", "PhotoId", "dbo.Photos");
            DropForeignKey("dbo.Feed_Photos", "FeedId", "dbo.Feeds");
            DropForeignKey("dbo.EventFeeds", "FeedId", "dbo.Feeds");
            DropForeignKey("dbo.EventFeeds", "EventId", "dbo.Events");
            DropForeignKey("dbo.PostFeeds", "FeedId", "dbo.Feeds");
            DropForeignKey("dbo.PostFeeds", "PostId", "dbo.Posts");
            DropForeignKey("dbo.FollowRequests", "FromUserId", "dbo.Users");
            DropForeignKey("dbo.FollowRequests", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.EventComments", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.EventComments", "EventId", "dbo.Events");
            DropForeignKey("dbo.PostComments", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.PostComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PhotoComments", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.PhotoComments", "PhotoId", "dbo.Photos");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Post_Categories", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Post_Categories", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Post_Tags", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Post_Tags", "ClubId", "dbo.Users");
            DropForeignKey("dbo.Posts", "PageId", "dbo.FacebookPages");
            DropForeignKey("dbo.Posts", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "FromUserId", "dbo.Users");
            DropForeignKey("dbo.Events", "PageId", "dbo.FacebookPages");
            DropForeignKey("dbo.Events", "UserId", "dbo.Users");
            DropForeignKey("dbo.Follows", "FromUserId", "dbo.Users");
            DropForeignKey("dbo.Follows", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.Photo_Categories", "PhotoId", "dbo.Photos");
            DropForeignKey("dbo.Photo_Categories", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.FacebookPages", "UserId", "dbo.Users");
            DropForeignKey("dbo.Albums", "PageId", "dbo.FacebookPages");
            DropForeignKey("dbo.Albums", "UserId", "dbo.Users");
            DropForeignKey("dbo.Photos", "PageId", "dbo.FacebookPages");
            DropForeignKey("dbo.Photos", "AlbumId", "dbo.Albums");
            DropForeignKey("dbo.Photos", "UserId", "dbo.Users");
            DropForeignKey("dbo.Weeklies", "PhotoId", "dbo.Photos");
            DropForeignKey("dbo.Weeklies", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserReceivedLikes", "UserId", "dbo.Users");
            DropForeignKey("dbo.ClubDetails", "Id", "dbo.Users");
            DropForeignKey("dbo.ClubDetails", "CityId", "dbo.Cities");
            DropForeignKey("dbo.ClubDetails", "SchoolId", "dbo.Schools");
            DropForeignKey("dbo.UserDetails", "Id", "dbo.Users");
            DropForeignKey("dbo.UserDetails", "CityId", "dbo.Cities");
            DropForeignKey("dbo.UserDetails", "MajorId", "dbo.Majors");
            DropForeignKey("dbo.UserDetails", "SchoolId", "dbo.Schools");
            DropForeignKey("dbo.LinkedAccounts", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserValidationTokens", "Id", "dbo.Users");
            DropForeignKey("dbo.Users", "Id", "dbo.UserPreferences");
            DropTable("dbo.UserRewardsHistory");
            DropTable("dbo.UserPointsHistory");
            DropTable("dbo.ClubPointRewardScenarios");
            DropTable("dbo.UserPoints");
            DropTable("dbo.UserRewards");
            DropTable("dbo.ClubRewards");
            DropTable("dbo.OAuthClients");
            DropTable("dbo.OAuthRefreshToken_Scopes");
            DropTable("dbo.OAuthRefreshTokens");
            DropTable("dbo.PhotoLikes");
            DropTable("dbo.CategoryStatistics");
            DropTable("dbo.CommentNotifications");
            DropTable("dbo.EventNotifications");
            DropTable("dbo.PostNotifications");
            DropTable("dbo.Notifications");
            DropTable("dbo.FacebookSyncRetries");
            DropTable("dbo.Feed_Users");
            DropTable("dbo.Feed_Photos");
            DropTable("dbo.EventFeeds");
            DropTable("dbo.PostFeeds");
            DropTable("dbo.Feeds");
            DropTable("dbo.FollowRequests");
            DropTable("dbo.EventComments");
            DropTable("dbo.PostComments");
            DropTable("dbo.PhotoComments");
            DropTable("dbo.Comments");
            DropTable("dbo.Post_Categories");
            DropTable("dbo.Post_Tags");
            DropTable("dbo.Posts");
            DropTable("dbo.Events");
            DropTable("dbo.Follows");
            DropTable("dbo.Photo_Categories");
            DropTable("dbo.FacebookPages");
            DropTable("dbo.Albums");
            DropTable("dbo.Photos");
            DropTable("dbo.Weeklies");
            DropTable("dbo.UserReceivedLikes");
            DropTable("dbo.ClubDetails");
            DropTable("dbo.UserDetails");
            DropTable("dbo.UserPreferences");
            DropTable("dbo.LinkedAccounts");
            DropTable("dbo.UserValidationTokens");
            DropTable("dbo.Users");
            DropTable("dbo.OAuthScopes");
            DropTable("dbo.Categories");
            DropTable("dbo.Schools");
            DropTable("dbo.Majors");
            DropTable("dbo.Cities");
        }
    }
}
