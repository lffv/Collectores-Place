using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CollectorsPlace.Models;
using WebMatrix.WebData;
using CollectorsPlace.Filters;

namespace CollectorsPlace.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class ArticlesController : Controller
    {
        private UsersContext db = new UsersContext();

        public ActionResult Index() {
            return RedirectToAction("Index", "Collections");
        }

        //
        // GET: /Articles/Details/5

        public ActionResult Details(int id = 0)
        {
            UserArticle userarticle = db.UserArticles.Find(id);
            if (userarticle == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Index", "Collections");
            }

            UserCollection coll = db.UserCollections.Find(userarticle.CollId);

            ListArticle article = new ListArticle() {
                Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId),
                Article = userarticle,
                Collection = coll
            };

            ViewBag.tags = getTagsByArticle(id);   

            return View(article);
        }

        //
        // GET: /Articles/Create

        public ActionResult Create()
        {

            RegisterArticlesModel r = new RegisterArticlesModel()
            {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId)
            };

            ViewBag.CollId = new SelectList(db.UserCollections.Where(u => u.UserId == WebSecurity.CurrentUserId), "CollId", "Name");

            ViewBag.tags = new string[1];

            return View(r);
        }

        //
        // POST: /Articles/Create
       
        [HttpPost]
        public ActionResult Create(RegisterArticlesModel userarticle)
        {

            string[] tags = Request.Form.GetValues("tags");

            if (ModelState.IsValid && tags != null)
            {
                
                int busdisp = 0;

                if (Request.Form["bdvenda"] != null && Request.Form["bdvenda"] == "1") {
                    busdisp++;
                }

                if (Request.Form["bdtroca"] != null && Request.Form["bdtroca"] == "1") {
                    busdisp=busdisp+2;
                }

                UserArticle novo = new UserArticle() {
                    Name = userarticle.Name,
                    Description = userarticle.Description,
                    CreateDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    AcquiredDate = userarticle.AcquiredDate.Date,
                    CollId = userarticle.CollId,
                    BusinessDisponibility = busdisp,
                    EstimatedValue = userarticle.EstimatedValue
                };

                if (Request.Form["imgtmp"] != null && Request.Form["imgtmp"] != "" )
                {
                    novo.CollAvatar = Request.Form["imgtmp"].Substring(Request.Form["imgtmp"].LastIndexOf('/') + 1);
                }
                else
                {
                    novo.CollAvatar = "noimage.jpg";
                }

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
                return RedirectToAction("Details", "Collections", new { id=novo.CollId });
            }

            ViewBag.CollId = new SelectList(db.UserCollections.Where(u => u.UserId == WebSecurity.CurrentUserId), "CollId", "Name", userarticle.CollId);
            userarticle.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);
            ViewBag.tags = tags;

            return View(userarticle);
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase Filedata, int id = 0)
        {

            var name = id + "." + Filedata.FileName.Substring(Filedata.FileName.LastIndexOf('.') + 1);
            string savePath = Server.MapPath(@"~\Images\articles\" + name);
            Filedata.SaveAs(savePath);

            UserArticle novo = new UserArticle();
            novo.ArticleId = id;

            db.UserArticles.Attach(novo);

            novo.CollAvatar = name;

            db.SaveChanges();

            return Content(Url.Content(@"~\Images\articles\" + name));

        }

        [HttpPost]
        public ActionResult UploadImagesd(HttpPostedFileBase Filedata)
        {

            int id = new Random().Next(10000, 1000000000);

            var name = id + "." + Filedata.FileName.Substring(Filedata.FileName.LastIndexOf('.') + 1);
            string savePath = Server.MapPath(@"~\Images\articles\" + name);
            Filedata.SaveAs(savePath);

            return Content(Url.Content(@"~\Images\articles\" + name));

        }

        //
        // GET: /Articles/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserArticle userarticle = db.UserArticles.Find(id);

            if (userarticle == null) {
                return RedirectToAction("Index", "Collections");
            }

            UserCollection u = db.UserCollections.Find(userarticle.CollId);

            if (u.UserId != WebSecurity.CurrentUserId)
            {
                //return HttpNotFound();
                return RedirectToAction("Index", "Collections");
            }
            ViewBag.CollId = new SelectList(db.UserCollections.Where(m => m.UserId == WebSecurity.CurrentUserId), "CollId", "Name", userarticle.CollId);

            EditArticle passar = new EditArticle() {
                Article = userarticle,
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                CollId = userarticle.CollId
            };

            ViewBag.tags = getTagsByArticle(id);

            return View(passar);
        }

        //
        // POST: /Articles/Edit/5

        [HttpPost]
        public ActionResult Edit(EditArticle userarticle)
        {

            string[] tags = Request.Form.GetValues("tags");

            UserCollection u = db.UserCollections.Find(userarticle.CollId);

            if (ModelState.IsValid && u.UserId == WebSecurity.CurrentUserId && tags!=null)
            {

                int busdisp = 0;

                if (Request.Form["bdvenda"] != null && Request.Form["bdvenda"] == "1")
                {
                    busdisp++;
                }

                if (Request.Form["bdtroca"] != null && Request.Form["bdtroca"] == "1")
                {
                    busdisp = busdisp + 2;
                }

                UserArticle nova = new UserArticle();
                nova.ArticleId = userarticle.Article.ArticleId;

                db.UserArticles.Attach(nova);

                nova.Name = userarticle.Article.Name;
                nova.Description = userarticle.Article.Description;
                nova.BusinessDisponibility = busdisp;
                nova.AcquiredDate = userarticle.Article.AcquiredDate;
                nova.EstimatedValue = userarticle.Article.EstimatedValue;
                nova.CollId = userarticle.CollId;

                nova.LastModifiedDate = DateTime.Now;

                // Elimina todas as tags existentes
                deleteTagsByArticle(userarticle.Article.ArticleId);

                // Adiciona as novas
                foreach (string tag in tags)
                {

                    var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                    if (ex == null)
                    {
                        nova.ArtTags.Add(new Tag() { Name = tag });
                    }
                    else
                    {
                        nova.ArtTags.Add(ex);
                    }

                }
                
                db.SaveChanges();

                return RedirectToAction("Details", "Collections", new { id = nova.CollId });

            }
            ViewBag.CollId = new SelectList(db.UserCollections.Where(uu => uu.UserId == WebSecurity.CurrentUserId), "CollId", "Name", userarticle.Article.CollId);

            EditArticle passar = new EditArticle()
            {
                Article = userarticle.Article,
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                CollId = userarticle.CollId
            };

            return View(passar);
        }

        //
        // GET: /Articles/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserArticle userarticle = db.UserArticles.Find(id);

            if (userarticle == null)
            {
                return RedirectToAction("Index", "Collections");
            }

            UserCollection u = db.UserCollections.Find(userarticle.CollId);

            if ( u.UserId != WebSecurity.CurrentUserId)
            {
                //return HttpNotFound();
                return RedirectToAction("Index", "Collections");
            }

            EditArticle passar = new EditArticle()
            {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Article = userarticle
            };

            return View(passar);
        }

        //
        // POST: /Articles/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UserArticle userarticle = db.UserArticles.Find(id);

            if (userarticle == null)
            {
                return RedirectToAction("Index", "Collections");
            }

            UserCollection u = db.UserCollections.Find(userarticle.CollId);

            int aux = userarticle.CollId;

            if (u.UserId != WebSecurity.CurrentUserId) {
                return RedirectToAction("Index", "Collections");
            }

            var negs = db.Negocios.Where(m => m.ArtId == id).ToList();

            foreach (var nn in negs)
            {
                db.Negocios.Remove(nn);
                db.SaveChanges();
            }

            db.UserArticles.Remove(userarticle);
            db.SaveChanges();
            return RedirectToAction("Details", "Collections", new { id = aux });
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

        private void deleteTagsByArticle(int id)
        {

            UserArticle artigo = null;

            using (var ctx = new UsersContext())
            {
                
                ctx.Configuration.LazyLoadingEnabled = false;

                artigo = (from s in ctx.UserArticles.Include("ArtTags")
                           where s.ArticleId == id
                           select s).FirstOrDefault<UserArticle>();

            }

            if (artigo == null) return;

            Tag ctags = artigo.ArtTags.FirstOrDefault<Tag>();

            artigo.ArtTags.Remove(ctags);

            using (var newCtx = new UsersContext())
            {
                var dbTags = (from s in newCtx.UserArticles.Include("ArtTags")
                                 where s.ArticleId == artigo.ArticleId
                                 select s).FirstOrDefault<UserArticle>();

                var deletedTags = dbTags.ArtTags.Except(artigo.ArtTags).ToList<Tag>();

                deletedTags.ForEach(cs => dbTags.ArtTags.Remove(cs));

                newCtx.SaveChanges();
            }

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}

