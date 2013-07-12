using System.Data.Entity;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    public class ZazzDbContext : DbContext
    {
        public IDbSet<City> Cities { get; set; }
        public IDbSet<Major> Majors { get; set; }
        public IDbSet<School> Schools { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<OAuthScope> OAuthScopes { get; set; }

        public IDbSet<User> Users { get; set; }
        public IDbSet<UserPreferences> UserPreferences { get; set; }
        public IDbSet<UserDetail> UserDetails { get; set; }
        public IDbSet<ClubDetail> ClubDetails { get; set; }
        public IDbSet<UserValidationToken> ValidationTokens { get; set; }
        public IDbSet<LinkedAccount> LinkedAccounts { get; set; }
        public IDbSet<Album> Albums { get; set; }
        public IDbSet<Photo> Photos { get; set; }
        public IDbSet<PhotoTag> PhotoTags { get; set; }
        public IDbSet<ZazzEvent> Events { get; set; }
        public IDbSet<EventTag> EventTags { get; set; }
        public IDbSet<Post> Posts { get; set; }
        public IDbSet<PostTag> PostTags { get; set; }
        public IDbSet<Comment> Comments { get; set; }
        public IDbSet<Follow> Follows { get; set; }
        public IDbSet<FollowRequest> FollowRequests { get; set; }
        public IDbSet<Feed> Feeds { get; set; }
        public IDbSet<FacebookPage> FacebookPages { get; set; }
        public IDbSet<FeedPhoto> FeedPhotos { get; set; }
        public IDbSet<FeedUser> FeedUsers { get; set; }
        public IDbSet<FacebookSyncRetry> FacebookSyncRetries { get; set; }
        public IDbSet<Notification> Notifications { get; set; }
        public IDbSet<TagStat> TagStats { get; set; }
        public IDbSet<Weekly> Weeklies { get; set; }
        public IDbSet<PhotoVote> PhotoVotes { get; set; }
        public IDbSet<UserReceivedVotes> UserReceivedVotes { get; set; }
        public IDbSet<AppRequestToken> AppRequestTokens { get; set; }
        public IDbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; }
        public IDbSet<OAuthRefreshTokenScope> OAuthRefreshTokenScopes { get; set; }
        public IDbSet<OAuthClient> OAuthClients { get; set; }
        public IDbSet<ClubReward> ClubRewards { get; set; }
        public IDbSet<UserReward> UserRewards { get; set; }
        public IDbSet<UserPoint> UserPoints { get; set; }
        public IDbSet<ClubPointRewardScenario> ClubPointRewardScenarios { get; set; }
        public IDbSet<UserPointHistory> UserPointsHistory { get; set; }
        public IDbSet<UserRewardHistory> UserRewardsHistory { get; set; }

#if DEBUG
        public ZazzDbContext(bool dropDbOnInit = false)
            : base("DevConnectionString")
        {
            if (dropDbOnInit)
                Database.SetInitializer(new DropCreateDatabaseAlwaysWithSeed());
            else
                Database.SetInitializer(new DropCreateDatabaseIfModelChangesWithSeed());

            Database.Initialize(true);
        }
#else
        public ZazzDbContext()
            : base("ProductionConnectionString")
        {
        }
#endif

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPreferences>()
                        .HasRequired(p => p.User)
                        .WithRequiredPrincipal(u => u.Preferences);

            modelBuilder.Entity<User>()
                        .HasOptional(u => u.UserValidationToken)
                        .WithRequired(v => v.User);

            modelBuilder.Entity<User>()
                        .HasOptional(u => u.UserDetail)
                        .WithRequired(d => d.User);

            modelBuilder.Entity<User>()
                        .HasOptional(u => u.ClubDetail)
                        .WithRequired(c => c.User);

            modelBuilder.Entity<Comment>()
                .HasRequired(e => e.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Follow>()
                .HasRequired(e => e.FromUser)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FollowRequest>()
                .HasRequired(e => e.FromUser)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Photo>()
                .HasRequired(i => i.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Post>()
                .HasOptional(p => p.ToUser)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FeedUser>()
                .HasRequired(i => i.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.UserB)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserReward>()
                .HasRequired(r => r.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPoint>()
                .HasRequired(p => p.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPoint>()
                .HasRequired(p => p.Club)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPointHistory>()
                .HasRequired(h => h.Club)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPointHistory>()
                .HasRequired(h => h.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPointHistory>()
                .HasRequired(h => h.Reward)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserRewardHistory>()
                .HasRequired(h => h.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserRewardHistory>()
                .HasRequired(h => h.Editor)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserRewardHistory>()
                .HasRequired(h => h.Reward)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Comment>()
                .HasOptional(c => c.PhotoComment)
                .WithRequired(p => p.Comment)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Comment>()
                .HasOptional(c => c.PostComment)
                .WithRequired(p => p.Comment)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Comment>()
                .HasOptional(c => c.EventComment)
                .WithRequired(e => e.Comment)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Feed>()
                .HasOptional(f => f.PostFeed)
                .WithRequired(p => p.Feed)
                .WillCascadeOnDelete();
            
            modelBuilder.Entity<Feed>()
                .HasOptional(f => f.EventFeed)
                .WithRequired(e => e.Feed)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Notification>()
                .HasOptional(n => n.PostNotification)
                .WithRequired(p => p.Notification)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Notification>()
                .HasOptional(n => n.EventNotification)
                .WithRequired(e => e.Notification)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Notification>()
                .HasOptional(n => n.CommentNotification)
                .WithRequired(c => c.Notification)
                .WillCascadeOnDelete();

            base.OnModelCreating(modelBuilder);
        }
    }
}