using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CollectorsPlace.Models;
using CollectorsPlace.Filters;
using WebMatrix.WebData;
using System.Web.UI.WebControls;
using System.IO;

namespace CollectorsPlace.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class CollectionsController : Controller
    {
        
        private UsersContext db = new UsersContext();

        //
        // GET: /Default1/

        public ActionResult Index()
        {

            UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            var usercollections = db.UserCollections.Where(us => us.UserProfile.UserId == user.UserId);

            ListCollections passar = new ListCollections();

            passar.Profile = user;
            passar.Collections = usercollections.ToList();

            return View(passar);
        }

        //
        // GET: /Default1/Details/5

        public ActionResult Details(int id = 0)
        {
            UserCollection usercollection = db.UserCollections.Find(id);
            if (usercollection == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Index");
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

        //
        // GET: /Default1/Create

        public ActionResult Create()
        {

            RegisterCollectionModel passar = new RegisterCollectionModel();

            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            ViewBag.tags = new string[1];

            return View(passar);
        }

        //
        // POST: /Default1/Create

        [HttpPost]
        public ActionResult Create(RegisterCollectionModel usercollection)
        {

            string[] tags = Request.Form.GetValues("tags");

            if (ModelState.IsValid && tags!=null)
            {

                UserCollection nova = new UserCollection();

                nova.Name = usercollection.Name;
                nova.Description = usercollection.Description;
                nova.CreateDate = nova.LastModifiedDate = DateTime.Now;
                nova.PrivacyType = 0;
                nova.UserId = WebSecurity.CurrentUserId;

                if (Request.Form["imgtmp"] != null && Request.Form["imgtmp"] != "")
                {
                    nova.CollAvatar = Request.Form["imgtmp"].Substring(Request.Form["imgtmp"].LastIndexOf('/') + 1);
                }
                else
                {
                    nova.CollAvatar = "noimage.jpg";
                }

                foreach (string tag in tags)
	            {

                    var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                    if (ex == null)
                    {
                        nova.CollTags.Add(new Tag() { Name = tag });
                    }
                    else {
                        nova.CollTags.Add(ex);
                    }
		            
	            }

                db.UserCollections.Add(nova);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            RegisterCollectionModel passar = new RegisterCollectionModel();

            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);
            passar.Name = usercollection.Name;
            passar.Description = usercollection.Description;
            ViewBag.tags = tags;

            return View(passar);
        }

        [HttpPost]
        public ActionResult UploadImagesd(HttpPostedFileBase Filedata)
        {

            int id = new Random().Next(10000,1000000000);

            var name = id + "." + Filedata.FileName.Substring(Filedata.FileName.LastIndexOf('.') + 1);
            string savePath = Server.MapPath(@"~\Images\collections\" + name);
            Filedata.SaveAs(savePath);

            return Content(Url.Content(@"~\Images\collections\" + name));

        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase Filedata, int id=0)
        {

            var name = id + "." + Filedata.FileName.Substring(Filedata.FileName.LastIndexOf('.') + 1);
            string savePath = Server.MapPath(@"~\Images\collections\" + name);
            Filedata.SaveAs(savePath);
            
            UserCollection novo = new UserCollection();
            novo.CollId = id;

            db.UserCollections.Attach(novo);

            novo.CollAvatar = name;

            db.SaveChanges();

            return Content(Url.Content(@"~\Images\collections\" + name));

        }

        //
        // GET: /Default1/Edit/5

        public ActionResult Edit(int id = 0)
        {

            UserCollection usercollection = db.UserCollections.Find(id);

            // Nao pode editar se nao existe ou se pertence a outro
            if (usercollection == null)
            {
                //return HttpNotFound(); // Se depois fizermos uma pagina 404
                return RedirectToAction("Index"); // Para já manda para o index
            }

            if (usercollection.UserId != WebSecurity.CurrentUserId) {
                //return HttpNotFound(); // Se depois fizermos uma pagina 404
                return RedirectToAction("Index"); // Para já manda para o index
            }

            var tags = getTagsByCollection(id);

            EditCollectionModel passar = new EditCollectionModel();
            passar.CollId = usercollection.CollId;
            passar.Name = usercollection.Name;
            passar.Description = usercollection.Description;
            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);
            passar.CollAvatar = usercollection.CollAvatar;

            ViewBag.tags = tags;

            return View(passar);
        }

        //
        // POST: /Default1/Edit/5

        [HttpPost]
        public ActionResult Edit(EditCollectionModel usercollection)
        {

            string[] tags = Request.Form.GetValues("tags");

            if (ModelState.IsValid && tags!=null)
            {

                UserCollection nova = new UserCollection();
                nova.CollId = usercollection.CollId;

                db.UserCollections.Attach(nova);

                // Actualiza os campos:

                nova.Name = usercollection.Name;
                nova.Description = usercollection.Description;
                nova.LastModifiedDate = DateTime.Now;

                // Elimina todas as tags existentes
                deleteTagsByCollection(usercollection.CollId);

                // Adiciona as novas
                foreach (string tag in tags)
                {

                    var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                    if (ex == null)
                    {
                        nova.CollTags.Add(new Tag() { Name = tag });
                    }
                    else
                    {
                        nova.CollTags.Add(ex);
                    }

                }

                db.SaveChanges();

                return RedirectToAction("Index");

            }

            EditCollectionModel passar = new EditCollectionModel();
            passar.CollId = usercollection.CollId;
            passar.Name = usercollection.Name;
            passar.Description = usercollection.Description;
            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            return View(passar);

        }

        //
        // GET: /Default1/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserCollection usercollection = db.UserCollections.Find(id);

            if (usercollection == null)
            {
                //return HttpNotFound(); // Se depois fizermos uma pagina 404
                return RedirectToAction("Index"); // Para já manda para o index
            }

            if (usercollection.UserId != WebSecurity.CurrentUserId)
            {
                //return HttpNotFound(); // Se depois fizermos uma pagina 404
                return RedirectToAction("Index"); // Para já manda para o index
            }

            EditCollectionModel passar = new EditCollectionModel();
            passar.CollId = usercollection.CollId;
            passar.Name = usercollection.Name;
            passar.Description = usercollection.Description;
            passar.Profile = db.UserProfiles.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            return View(passar);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            //UserCollection usercollection = db.UserCollections.Find(id);
            UserCollection usercollection = db.UserCollections.Include("CollArticles").FirstOrDefault(m => m.CollId == id);

            // Certifica-se que é do utilizador
            if (usercollection.UserId == WebSecurity.CurrentUserId)
            {

                var tmp = usercollection.CollArticles.ToList();

                foreach (var art in tmp)
                {

                    var tmp2 = db.Negocios.ToList();

                    foreach (var neg in tmp2) {

                        if (neg.ArtId == art.ArticleId) {
                            db.Negocios.Remove(neg);
                            db.SaveChanges();
                        }
                    }
                }

                db.UserCollections.Remove(usercollection);
                db.SaveChanges();
            }

            return RedirectToAction("Index");

        }

        [HttpPost, ActionName("gettags")]
        public JsonResult TagsExistentes(string term) {

            var tags = from p in db.Tags
                       where p.Name.Contains(term)
                       select p.Name;

            return Json(tags);

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

        private void deleteTagsByCollection(int id)
        {

            UserCollection colecao = null;

            using (var ctx = new UsersContext())
            {

                ctx.Configuration.LazyLoadingEnabled = false;

                colecao = (from s in ctx.UserCollections.Include("CollTags")
                           where s.CollId == id
                           select s).FirstOrDefault<UserCollection>();

            }

            if (colecao == null) return;

            Tag ctags = colecao.CollTags.FirstOrDefault<Tag>();

            colecao.CollTags.Remove(ctags);

            using (var newCtx = new UsersContext())
            {
                var dbTags = (from s in newCtx.UserCollections.Include("CollTags")
                                 where s.CollId == colecao.CollId
                                 select s).FirstOrDefault<UserCollection>();

                var deletedTags = dbTags.CollTags.Except(colecao.CollTags).ToList<Tag>();

                deletedTags.ForEach(cs => dbTags.CollTags.Remove(cs));

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
