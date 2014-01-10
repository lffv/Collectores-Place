namespace CollectorsPlace.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Estado : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Negocios", "Estado", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Negocios", "Estado");
        }
    }
}
