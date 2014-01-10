using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Globalization;
using System.Web.Security;

namespace CollectorsPlace.Models
{

    /* ----------------- UTILIZADORES ---------------------------------------- */

    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Webpages_Membership> webpages_Memberships { get; set; }

        public DbSet<UserCollection> UserCollections { get; set; }
        public DbSet<UserArticle> UserArticles { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCollection>().
              HasMany(c => c.CollTags).
              WithMany(p => p.TagCollections).
              Map(
               m =>
               {
                   m.MapLeftKey("CollId");
                   m.MapRightKey("TagId");
                   m.ToTable("UserCollectionsTags");
               });

            modelBuilder.Entity<UserArticle>().
              HasMany(c => c.ArtTags).
              WithMany(p => p.TagArticles).
              Map(
               m =>
               {
                   m.MapLeftKey("ArticleId");
                   m.MapRightKey("TagId");
                   m.ToTable("UserArticlesTags");
               });

            modelBuilder.Entity<UserProfile>().
                HasMany(m => m.Followers).
                WithMany(p => p.Following).
                Map(w => w.ToTable("UsersFollow").
                    MapLeftKey("UserId").
                    MapRightKey("FollowerID"));

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Negocio>().
              HasMany(c => c.ArtTroca).
              WithMany(p => p.ArtTrocas).
              Map(
               m =>
               {
                   m.MapLeftKey("NegId");
                   m.MapRightKey("ArticleId");
                   m.ToTable("ArtigosTroca");
               });

        }

        public DbSet<Negocio> Negocios { get; set; }

        public DbSet<Message> Messages { get; set; }

    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }

        public virtual ICollection<UserCollection> UserCollection { get; set; }

        public virtual ICollection<UserProfile> Followers { get; set; }
        public virtual ICollection<UserProfile> Following { get; set; }

    }


    [Table("webpages_Membership")]
    public class Webpages_Membership
    {

        [Key]
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string ConfirmationToken { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime LastPasswordFailureDate { get; set; }
        public int PasswordFailuresSinceLastSuccess { get; set; }
        public string Password { get; set; }
        public DateTime PasswordChangedDate { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordVerificationToken { get; set; }
        public DateTime PasswordVerificationTokenExpirationDate { get; set; }
    }

    /* ------------------------ COLECOES ----------------------------- */

    [Table("UserCollections")]
    public class UserCollection
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Collection")]
        public int CollId { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Description { get; set; }
        public int PrivacyType { get; set; }

        [DefaultValue("noimage.jpg")]
        public string CollAvatar { get; set; }

        public int UserId { get; set; }
        public UserProfile UserProfile { get; set; }

        public ICollection<Tag> CollTags { get; set; }
        public ICollection<UserArticle> CollArticles { get; set; }

        public UserCollection() {
            CollTags = new HashSet<Tag>();
            CollArticles = new HashSet<UserArticle>();
        }

    }

    [Table("UserArticles")]
    public class UserArticle {
        
        [Key]
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        [DefaultValue("noimage.jpg")]
        public string CollAvatar { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Acquired in")]
        public DateTime AcquiredDate { get; set; }

        // = 0 -> disponivel para nenhum
        // = 1 -> disponivel para venda
        // = 2 -> disponivel para troca
        // = 3 -> disponivel para ambos
        public int BusinessDisponibility { get; set; }

        [Display(Name = "Value")]
        public decimal EstimatedValue { get; set; }

        [Display(Name = "Collection")]
        public int CollId { get; set; }
        [Display(Name = "Collection")]
        public UserCollection UserCollection { get; set; }

        public ICollection<Tag> ArtTags { get; set; }
        public ICollection<Negocio> ArtTrocas { get; set; }

        public UserArticle()
        {
            ArtTags = new HashSet<Tag>();
            ArtTrocas = new HashSet<Negocio>();
        }

    }

    /* ---------------------- TAGS ------------------------ */

    [Table("Tags")]
    public class Tag {

        [Key]
        public int TagId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<UserCollection> TagCollections { get; set; }
        public ICollection<UserArticle> TagArticles { get; set; }

        public Tag(){
            TagCollections = new HashSet<UserCollection>();
            TagArticles = new HashSet<UserArticle>();
        }

    }

    /* ---------------------- Negocios ------------------------ */

    [Table("Negocios")]
    public class Negocio
    {

        [Key]
        public int NegId { get; set; }

        [Display(Name = "Date")]
        public DateTime ProposalDate { get; set; }

        public decimal valor  { get; set; }

        public string descricao  { get; set; }

        // tipo = 0 -> venda directa
        // tipo = 1 -> proposta de negocio (troca ou venda)
        public int tipo { get; set; }

        public int UserIdComprador { get; set; }
        public UserProfile UserProfileComprador { get; set; }

        public int UserIdVendedor { get; set; }
        public UserProfile UserProfileVendedor { get; set; }

        public int ArtId { get; set; }
        public UserArticle Article { get; set; }

        // estado = 0 -> em negociação
        // estado = 1 -> concluida
        public int Estado { get; set; }

        public ICollection<UserArticle> ArtTroca { get; set; }

        public Negocio()
        {
            ArtTroca = new HashSet<UserArticle>();
        }

    }

    [Table("Messages")]
    public class Message {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int MsgId { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public int UserIdSender { get; set; }
        public UserProfile UserProfileSender { get; set; }

        public int UserIdReceiver { get; set; }
        public UserProfile UserProfileReceiver { get; set; }

        public DateTime MsgDate { get; set; }

        // = 0 Nao lida
        // = 1 lida
        public int Read { get; set; }

    }

    /* --------------  Tipagem das Views -------------------------- */

    public class ListarMensagens {
        public UserProfile Profile { get; set; }
        public IEnumerable<Message> Messages { get; set; }
    }

    public class ListarMensagem
    {
        public UserProfile Profile { get; set; }
        public Message Message { get; set; }
    }

    public class AddPropostaTroca{

        public UserProfile Profile { get; set; }
        public List<UserArticle> Articles { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; }

    }

    public class ListaFollowwings {

        public UserProfile Profile { get; set; }

    }

    public class ListNegocio
    {

        public UserProfile Profile { get; set; }
        public Negocio Negocio { get; set; }
        public IEnumerable<UserArticle> Articles { get; set; }
        public UserProfile UserComprador { get; set; }
        public UserProfile UserVendedor { get; set; }

    }

    public class ListNegocios
    {

        public UserProfile Profile { get; set; }
        public IEnumerable<Negocio> Negocios { get; set; }
        public IEnumerable<UserArticle> Articles { get; set; }
        public IEnumerable<UserProfile> Users { get; set; }
        public IEnumerable<Negocio> NegociosPending { get; set; }

    }

    public class UserPage {

        public UserProfile Profile { get; set; }
        public UserProfile User { get; set; }

    }

    public class RegisterArticlesModel
    {

        [Required]
        [MaxLength(60, ErrorMessage = "Max of 60")]
        public string Name { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Max of 500")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name="Acquired in")]
        public DateTime AcquiredDate { get; set; }
        
        [Display(Name="Value")]
        [RegularExpression(@"[0-9]+(.[0-9]{1,2})?", ErrorMessage="Wrong format")]
        public decimal EstimatedValue { get; set; }

        [Display(Name = "Collection")]
        [Required(ErrorMessage="Collection is Required")]
        public int CollId { get; set; }

        public UserProfile Profile { get; set; }
    }

    public class BuyArticle
    {

        public UserProfile Profile { get; set; }
        public UserArticle Article { get; set; }
        public UserCollection Collection { get; set; }
        public System.Web.Mvc.SelectList Collections { get; set; }

    }

    public class ListArticle
    {

        public UserProfile Profile { get; set; }
        public UserArticle Article { get; set; }
        public UserCollection Collection{ get; set; }

    }

    public class EditArticle {
        public UserArticle Article{ get; set; }
        public UserProfile Profile { get; set; }
        [Required(ErrorMessage = "Collection is Required")]
        public int CollId { get; set; }
    }

    public class ListCollections
    {
        public UserProfile Profile { get; set; }
        public IEnumerable<CollectorsPlace.Models.UserCollection> Collections { get; set; }
    }

    public class RegisterCollectionModel
    {
        [Required]
        [MaxLength(60, ErrorMessage="Max of 60")]
        public string Name{ get; set; }
        [Required]
        [MaxLength(500, ErrorMessage="Max of 500")]
        public string Description{ get; set; }

        public UserProfile Profile { get; set; }
    }

    public class EditCollectionModel
    {
        [Required]
        public int CollId{ get; set; }
        [Required]
        [MaxLength(60, ErrorMessage = "Max of 60")]
        public string Name { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Max of 500")]
        public string Description { get; set; }

        public string CollAvatar { get; set; }

        public UserProfile Profile { get; set; }
    }

    public class DetailsCollectionModel
    {

        public UserCollection Collection{ get; set; }
        public IEnumerable<UserArticle> Articles { get; set; }

        public UserProfile Profile { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "Name")]
        public string UserName { get; set; }

        public string UserEmail { get; set; }
        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public UserProfile Profile { get; set; }

    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User Email")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
        public string UserEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User Email")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
        public string UserEmail { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel {

        [Required]
        [Display(Name = "User Email")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
        public string UserEmail { get; set; }

    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class RecentActivityModel {
        
        public UserProfile Profile { get; set; }
        public List<ActivityModel> Activity { get; set; }

    }

    public class ActivityModel {

        public UserProfile User { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

    }

    public class CollectionsModel {

        public UserProfile Profile { get; set; }


    }

}
