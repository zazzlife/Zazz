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

        public IDbSet<User> Users { get; set; }
        public IDbSet<UserDetail> UserDetails { get; set; }
        public IDbSet<ClubDetail> ClubDetails { get; set; }
        public IDbSet<ValidationToken> ValidationTokens { get; set; }
        public IDbSet<OAuthAccount> OAuthAccounts { get; set; }
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
            modelBuilder.Entity<User>()
                        .HasOptional(u => u.ValidationToken)
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

            // new db design:

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