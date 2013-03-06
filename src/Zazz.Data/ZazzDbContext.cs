﻿using System.Data.Entity;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    public class ZazzDbContext : DbContext
    {
        public IDbSet<City> Cities { get; set; }
        public IDbSet<Major> Majors { get; set; }
        public IDbSet<School> Schools { get; set; }

        public IDbSet<User> Users { get; set; }
        public IDbSet<ValidationToken> ValidationTokens { get; set; }
        public IDbSet<OAuthAccount> OAuthAccounts { get; set; }
        public IDbSet<UserImage> UserImages { get; set; }
        public IDbSet<UserEvent> UserEvents { get; set; }
        public IDbSet<UserEventComment> UserEventComments { get; set; }
        public IDbSet<UserFollow> UserFollows { get; set; }
        public IDbSet<UserFollowRequest> UserFollowRequests { get; set; }
        public IDbSet<UserFeed> UserFeeds { get; set; }

        public IDbSet<Club> Clubs { get; set; }
        public IDbSet<ClubAdmin> ClubAdmins { get; set; }
        public IDbSet<ClubImage> ClubImages { get; set; }
        public IDbSet<ClubPost> ClubPosts { get; set; }
        public IDbSet<ClubPostComment> ClubPostComments { get; set; }
        public IDbSet<ClubFollow> ClubFollows { get; set; }


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
                        .WithRequired();
          
            modelBuilder.Entity<UserEventComment>()
                .HasRequired(e => e.From)
                .WithMany(u => u.UserEventComments)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserFollow>()
                .HasRequired(e => e.FromUser)
                .WithMany(u => u.FollowingUsers)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserFollowRequest>()
                .HasRequired(e => e.FromUser)
                .WithMany(u => u.SentFollowRequests)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}