using CollectorsPlace.Filters;
using CollectorsPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CollectorsPlace.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class UserController : Controller
    {

        private UsersContext db = new UsersContext();

        //
        // GET: /User/

        public ActionResult Index(int id=0)
        {

            UserProfile loggedin = db.UserProfiles.Find(WebSecurity.CurrentUserId);
            UserProfile user = db.UserProfiles.Find(id);

            if (user == null) {
                return RedirectToAction("Index", "Home");
            }

            if (user.UserId == loggedin.UserId)
            {
                return RedirectToAction("Index", "Home");
            }

            UserPage passar = new UserPage()
            {
                Profile = loggedin,
                User = user
            };

            return View(passar);
        }

        public ActionResult Collections(int id=0) {

            UserProfile logged = db.UserProfiles.Find(WebSecurity.CurrentUserId);
            UserProfile user = db.UserProfiles.Find(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (user.UserId == logged.UserId) {
                return RedirectToAction("Index", "Collections");
            }

            var usercollections = db.UserCollections.Where(us => us.UserProfile.UserId == user.UserId);

            ListCollections passar = new ListCollections();

            passar.Profile = logged;
            passar.Collections = usercollections.ToList();

            return View(passar);
        }

        public ActionResult Collection(int id=0) {

            UserCollection usercollection = db.UserCollections.Find(id);
            if (usercollection == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Index");
            }

            if (usercollection.UserId == WebSecurity.CurrentUserId) {
                return RedirectToAction("Details", "Collections", new { id=id});
            }

            DetailsCollectionModel passar = new DetailsCollectionModel();
            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            passar.Collection = usercollection;

            var articles = from p in db.UserArticles
                           where p.CollId == id
                           select p;

            passar.Articles = articles.ToList();

            ViewBag.tags = getTagsByCollection(id);

            passar.Collection.UserProfile = db.UserProfiles.Find(usercollection.UserId);

            return View(passar);

        }

        public ActionResult Article(int id = 0) {

            UserArticle userarticle = db.UserArticles.Find(id);
            if (userarticle == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Index", "Collections");
            }

            UserCollection coll = db.UserCollections.Find(userarticle.CollId);

            if (coll.UserId == WebSecurity.CurrentUserId) {
                return RedirectToAction("Details", "Articles", new { id = id } );
            }

            ListArticle article = new ListArticle()
            {
                Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId),
                Article = userarticle,
                Collection = coll
            };

            ViewBag.tags = getTagsByArticle(id);

            return View(article);

        }

        public ActionResult Buy(int id=0) {

            UserArticle userarticle = db.UserArticles.Find(id);
            if (userarticle == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Index", "Collections");
            }

            UserCollection coll = db.UserCollections.Find(userarticle.CollId);

            if (coll.UserId == WebSecurity.CurrentUserId)
            {
                return RedirectToAction("Details", "Articles", new { id = id });
            }

            List<SelectListItem> listItems = new List<SelectListItem>();

            foreach(UserCollection us in db.UserCollections.Where(u => u.UserId == WebSecurity.CurrentUserId)){
                listItems.Add(new SelectListItem(){
                    Value = us.CollId+"",
                    Text = us.Name
                });
            }

            BuyArticle article = new BuyArticle()
            {
                Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId),
                Article = userarticle,
                Collection = coll,
                Collections = new SelectList(listItems, "Value", "Text")
            };


            return View(article);

        }

        [HttpPost, ActionName("Buy")]
        public ActionResult BuyConfirmed(int id = 0) {

            int collId = 0;

            var colls = Request.Form["colls"];

            if (colls != null)
            {
                try
                {
                    collId = int.Parse(colls);
                }
                catch (Exception) {
                    return RedirectToAction("Index", "Collections");
                }
            }

            if (collId == 0) {
                
                return RedirectToAction("Index", "Collections");
            }

            UserArticle art = db.UserArticles.Find(id);

            if (art == null) {
                return RedirectToAction("Index", "Collections");
            }

            UserCollection coll = db.UserCollections.Find(art.CollId);

            if(coll==null){
                return RedirectToAction("Index", "Collections");
            }

            UserArticle antigo = db.UserArticles.Find(id);

            // Adiciona o novo

            UserArticle novo = new UserArticle() { 
                AcquiredDate = DateTime.Now,
                BusinessDisponibility = 0,
                CollId = collId,
                CreateDate = DateTime.Now,
                Description = antigo.Description,
                EstimatedValue = antigo.EstimatedValue,
                LastModifiedDate = DateTime.Now,
                Name = antigo.Name,
            };

            var tags = getTagsByArticle(id);

            foreach (string tag in tags)
            {

                var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                if (ex == null)
                {

                    novo.ArtTags.Add(new Tag() { Name = tag });
                }
                else
                {
                    novo.ArtTags.Add(ex);
                }

            }

            db.UserArticles.Add(novo);
            db.SaveChanges();

            foreach (var nego in db.Negocios.Where(a => a.ArtId == antigo.ArticleId))
            {
                db.Negocios.Remove(nego);
            }

            db.SaveChanges();

            // Remove o antigo
            db.UserArticles.Remove(antigo);
            db.SaveChanges();

            // ---------- Insere o Negocio -----------------------
            Negocio negocio = new Negocio()
            {
                ArtId = novo.ArticleId,
                Estado = 1, // Foi vendido
                ProposalDate = DateTime.Now,
                tipo = 0,
                valor = novo.EstimatedValue,
                UserIdComprador = WebSecurity.CurrentUserId,
                UserIdVendedor = coll.UserId,
                Article = novo,
                UserProfileVendedor = db.UserProfiles.Find(coll.UserId),
                UserProfileComprador = db.UserProfiles.Find(WebSecurity.CurrentUserId)
            };

            db.Negocios.Add(negocio);
            db.SaveChanges();

            return RedirectToAction("Index", "Business");
            
        }

        public ActionResult ApresentaTroca(int id=0) {

            UserArticle article = db.UserArticles.Find(id);

            if (article == null || (article.BusinessDisponibility != 2 && article.BusinessDisponibility != 3))
            {
                return RedirectToAction("Index", "Business");
            }

            var colls = from p in db.UserCollections
                        where p.UserId == WebSecurity.CurrentUserId
                        select p;

            List<UserArticle> ret = new List<UserArticle>();

            foreach (var c in colls)
            {
                foreach (var a in db.UserArticles)
                {

                    if(a.CollId==c.CollId) ret.Add(a);
                }
            }

            AddPropostaTroca aprt = new AddPropostaTroca()
            {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Articles = ret
            };

            return View(aprt);

        }

        [HttpPost, ActionName("ApresentaTroca")]
        public ActionResult AddProposta(int id = 0) {

            UserArticle article = db.UserArticles.Find(id);

            if (article == null) {
                return RedirectToAction("Index", "Business");
            }

            UserCollection coll = db.UserCollections.Find(article.CollId);

            if (coll == null) {
                return RedirectToAction("Index", "Business");
            }

            String[] arts = Request.Form.GetValues("arttroca");
            decimal val=0;

            decimal.TryParse(Request.Form["Valor"], out val);

            Negocio neg = new Negocio() { 
                Article=article,
                ArtId=id,
                descricao = Request.Form["Descricao"],
                valor = val,
                Estado = 0,
                ProposalDate = DateTime.Now,
                tipo = 1,
                UserIdComprador = WebSecurity.CurrentUserId,
                UserIdVendedor = coll.UserId,
                UserProfileComprador = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                UserProfileVendedor = db.UserProfiles.Find(coll.UserId)
            };

            int i;

            foreach (var a in arts)
            {

                if (int.TryParse(a, out i))
                {
                    var artaxu = db.UserArticles.Find(i);
                    neg.ArtTroca.Add(artaxu);
                }
            }

            db.Negocios.Add(neg);
            db.SaveChanges();

            return RedirectToAction("Index", "Business");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public int Follow(int id=0) {

            UserProfile logged = db.UserProfiles.Find(WebSecurity.CurrentUserId);
            UserProfile user = db.UserProfiles.Find(id);

            if (user == null || logged == null) {
                return 0;
            }

                // Se estiver a seguir, retiro
            UserProfile aux = logged.Following.FirstOrDefault(m => m.UserId == user.UserId);
            if (aux != null)
            {

                logged.Following.Remove(user);
                user.Followers.Remove(logged);

            }
            else
            {
                logged.Following.Add(user);
                user.Followers.Add(logged);
            }

            db.SaveChanges();

            return 1;

        }

        public ActionResult ListaFollow() {

            ListaFollowwings l = new ListaFollowwings() {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId)
            };

            return View(l);

        }

        private List<string> getTagsByCollection(int id)
        {

            UserCollection teste = db.UserCollections.Find(id);

            if (teste == null) return new List<string>();

            var tags = db.UserCollections
                     .SelectMany(
                        colecao => colecao.CollTags,
                        (colecao, tag) => new
                        {
                            NameTag = tag.Name,
                            ColId = colecao.CollId
                        }).Where(m => m.ColId == id).Select(m => m.NameTag).ToList();

            return tags;
        }

        private List<string> getTagsByArticle(int id)
        {

            UserArticle teste = db.UserArticles.Find(id);

            if (teste == null) return new List<string>();

            var tags = db.UserArticles
                     .SelectMany(
                        artigo => artigo.ArtTags,
                        (artigo, tag) => new
                        {
                            NameTag = tag.Name,
                            ArtId = artigo.ArticleId
                        }).Where(m => m.ArtId == id).Select(m => m.NameTag).ToList();

            return tags;
        }

        [HttpPost]
        public JsonResult Recomendacoes(){
            
            List<object> ret = new List<object>();

            // Procura um User

            var users = db.UserProfiles.ToList();
            Random rnd = new Random();
            int r = rnd.Next(users.Count());

            ret.Add(new
            {
                titulo = users[r].UserName,
                img = "/Images/avatars/" + users[r].UserAvatar,
                href = Url.Action("Index", "User", new { id = users[r].UserId })
            });

            // Procura um artigo 

            var arts = db.UserArticles.ToList();
            rnd = new Random();

            r = rnd.Next(arts.Count());

            ret.Add(new { 
                titulo=arts[r].Name,
                img="/Images/articles/"+arts[r].CollAvatar,
                href=Url.Action("Article", "User", new { id = arts[r].ArticleId })
            });

            // Procura uma Coleção

            var colls = db.UserCollections.ToList();
            rnd = new Random();

            r = rnd.Next(colls.Count());

            ret.Add(new
            {
                titulo = colls[r].Name,
                img = "/Images/collections/" + colls[r].CollAvatar,
                href = Url.Action("Collection", "User", new { id = colls[r].CollId })
            });


            return Json(ret);

        }

    }
}
