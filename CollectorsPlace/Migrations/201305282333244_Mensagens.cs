namespace CollectorsPlace.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mensagens : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MsgId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Body = c.String(),
                        UserIdSender = c.Int(nullable: false),
                        UserIdReceiver = c.Int(nullable: false),
                        MsgDate = c.DateTime(nullable: false),
                        Read = c.Int(nullable: false),
                        UserProfileSender_UserId = c.Int(),
                        UserProfileReceiver_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.MsgId)
                .ForeignKey("dbo.UserProfile", t => t.UserProfileSender_UserId)
                .ForeignKey("dbo.UserProfile", t => t.UserProfileReceiver_UserId)
                .Index(t => t.UserProfileSender_UserId)
                .Index(t => t.UserProfileReceiver_UserId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Messages", new[] { "UserProfileReceiver_UserId" });
            DropIndex("dbo.Messages", new[] { "UserProfileSender_UserId" });
            DropForeignKey("dbo.Messages", "UserProfileReceiver_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Messages", "UserProfileSender_UserId", "dbo.UserProfile");
            DropTable("dbo.Messages");
        }
    }
}
