namespace Zazz.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Stat_Users",
                c => new
                    {
                        CategoryStatId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CategoryStatId, t.UserId })
                .ForeignKey("dbo.CategoryStatistics", t => t.CategoryStatId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.CategoryStatId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Stat_Users", new[] { "UserId" });
            DropIndex("dbo.Stat_Users", new[] { "CategoryStatId" });
            DropForeignKey("dbo.Stat_Users", "UserId", "dbo.Users");
            DropForeignKey("dbo.Stat_Users", "CategoryStatId", "dbo.CategoryStatistics");
            DropTable("dbo.Stat_Users");
        }
    }
}
