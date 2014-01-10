namespace CollectorsPlace.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Avatar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserCollections", "CollAvatar", c => c.String());
            AddColumn("dbo.UserArticles", "CollAvatar", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserArticles", "CollAvatar");
            DropColumn("dbo.UserCollections", "CollAvatar");
        }
    }
}
