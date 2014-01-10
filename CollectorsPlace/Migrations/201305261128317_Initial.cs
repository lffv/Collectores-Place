namespace CollectorsPlace.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserEmail = c.String(),
                        UserName = c.String(),
                        UserAvatar = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.UserCollections",
                c => new
                    {
                        CollId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        Description = c.String(),
                        PrivacyType = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CollId)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        TagId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.TagId);
            
            CreateTable(
                "dbo.UserArticles",
                c => new
                    {
                        ArticleId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        AcquiredDate = c.DateTime(nullable: false),
                        BusinessDisponibility = c.Int(nullable: false),
                        EstimatedValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CollId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ArticleId)
                .ForeignKey("dbo.UserCollections", t => t.CollId, cascadeDelete: true)
                .Index(t => t.CollId);
            
            CreateTable(
                "dbo.Negocios",
                c => new
                    {
                        NegId = c.Int(nullable: false, identity: true),
                        ProposalDate = c.DateTime(nullable: false),
                        valor = c.Decimal(nullable: false, precision: 18, scale: 2),
                        descricao = c.String(),
                        tipo = c.Int(nullable: false),
                        UserIdComprador = c.Int(nullable: false),
                        UserIdVendedor = c.Int(nullable: false),
                        ArtId = c.Int(nullable: false),
                        UserProfileComprador_UserId = c.Int(),
                        UserProfileVendedor_UserId = c.Int(),
                        Article_ArticleId = c.Int(),
                    })
                .PrimaryKey(t => t.NegId)
                .ForeignKey("dbo.UserProfile", t => t.UserProfileComprador_UserId)
                .ForeignKey("dbo.UserProfile", t => t.UserProfileVendedor_UserId)
                .ForeignKey("dbo.UserArticles", t => t.Article_ArticleId)
                .Index(t => t.UserProfileComprador_UserId)
                .Index(t => t.UserProfileVendedor_UserId)
                .Index(t => t.Article_ArticleId);
            
            CreateTable(
                "dbo.UserArticlesTags",
                c => new
                    {
                        ArticleId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ArticleId, t.TagId })
                .ForeignKey("dbo.UserArticles", t => t.ArticleId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.ArticleId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.ArtigosTroca",
                c => new
                    {
                        NegId = c.Int(nullable: false),
                        ArticleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.NegId, t.ArticleId })
                .ForeignKey("dbo.Negocios", t => t.NegId, cascadeDelete: true)
                .ForeignKey("dbo.UserArticles", t => t.ArticleId, cascadeDelete: true)
                .Index(t => t.NegId)
                .Index(t => t.ArticleId);
            
            CreateTable(
                "dbo.UserCollectionsTags",
                c => new
                    {
                        CollId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CollId, t.TagId })
                .ForeignKey("dbo.UserCollections", t => t.CollId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.CollId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.UsersFollow",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FollowerID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.FollowerID })
                .ForeignKey("dbo.UserProfile", t => t.UserId)
                .ForeignKey("dbo.UserProfile", t => t.FollowerID)
                .Index(t => t.UserId)
                .Index(t => t.FollowerID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UsersFollow", new[] { "FollowerID" });
            DropIndex("dbo.UsersFollow", new[] { "UserId" });
            DropIndex("dbo.UserCollectionsTags", new[] { "TagId" });
            DropIndex("dbo.UserCollectionsTags", new[] { "CollId" });
            DropIndex("dbo.ArtigosTroca", new[] { "ArticleId" });
            DropIndex("dbo.ArtigosTroca", new[] { "NegId" });
            DropIndex("dbo.UserArticlesTags", new[] { "TagId" });
            DropIndex("dbo.UserArticlesTags", new[] { "ArticleId" });
            DropIndex("dbo.Negocios", new[] { "Article_ArticleId" });
            DropIndex("dbo.Negocios", new[] { "UserProfileVendedor_UserId" });
            DropIndex("dbo.Negocios", new[] { "UserProfileComprador_UserId" });
            DropIndex("dbo.UserArticles", new[] { "CollId" });
            DropIndex("dbo.UserCollections", new[] { "UserId" });
            DropForeignKey("dbo.UsersFollow", "FollowerID", "dbo.UserProfile");
            DropForeignKey("dbo.UsersFollow", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserCollectionsTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.UserCollectionsTags", "CollId", "dbo.UserCollections");
            DropForeignKey("dbo.ArtigosTroca", "ArticleId", "dbo.UserArticles");
            DropForeignKey("dbo.ArtigosTroca", "NegId", "dbo.Negocios");
            DropForeignKey("dbo.UserArticlesTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.UserArticlesTags", "ArticleId", "dbo.UserArticles");
            DropForeignKey("dbo.Negocios", "Article_ArticleId", "dbo.UserArticles");
            DropForeignKey("dbo.Negocios", "UserProfileVendedor_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Negocios", "UserProfileComprador_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserArticles", "CollId", "dbo.UserCollections");
            DropForeignKey("dbo.UserCollections", "UserId", "dbo.UserProfile");
            DropTable("dbo.UsersFollow");
            DropTable("dbo.UserCollectionsTags");
            DropTable("dbo.ArtigosTroca");
            DropTable("dbo.UserArticlesTags");
            DropTable("dbo.Negocios");
            DropTable("dbo.UserArticles");
            DropTable("dbo.Tags");
            DropTable("dbo.UserCollections");
            DropTable("dbo.UserProfile");
        }
    }
}
