using CollectorsPlace.Filters;
using CollectorsPlace.Models;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace CollectorsPlace.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {

        private UsersContext db = new UsersContext();

        public ActionResult Index()
        {

            UserProfile user = db.UserProfiles.Find(WebSecurity.CurrentUserId);

            List<ActivityModel> act = new List<ActivityModel>();

            foreach (var f in user.Following)
	        {

                // Pega nos ultimos 5 que está a seguir, se tiver tempo adicionar a data
                foreach (var u in f.Following.Skip(Math.Max(0, f.Following.Count() - 5)).Take(5)) {
                    act.Add(new ActivityModel() { 
                        User=f, 
                        Date=new DateTime(1,1,1),
                        Description =  f.UserName + " is now following " + ((user.UserId==u.UserId)?"you":u.UserName)
                    });
                }

                foreach (var c in f.UserCollection) {

                    act.Add(new ActivityModel()
                    {
                        User = f,
                        Date = c.CreateDate,
                        Description = f.UserName + " created the collecttion " + "<a href=\"" + Url.Action("Collection", "User", new { id=c.CollId}) + "\">" + c.Name + "</a>"
                    });

                }

                foreach (var c in f.UserCollection)
                {

                    if (DateTime.Compare(c.LastModifiedDate,c.CreateDate)!=0)
                    {

                        act.Add(new ActivityModel()
                        {
                            User = f,
                            Date = c.LastModifiedDate,
                            Description = f.UserName + " updated the collecttion " + "<a href=\"" + Url.Action("Collection", "User", new { id = c.CollId }) + "\">" + c.Name + "</a>"
                        });

                    }

                }

                

                foreach (var c in f.UserCollection) {

                    var articles = from a in db.UserArticles
                                   where a.CollId == c.CollId
                                   select a;

                    foreach (var a in articles) {

                        act.Add(new ActivityModel()
                        {
                            User = f,
                            Date = a.CreateDate,
                            Description = f.UserName + " added the article " + "<a href=\"" + Url.Action("Article", "User", new { id = a.ArticleId }) + "\">" + a.Name + "</a> to the collection <a href=\"" + Url.Action("Collection", "User", new { id = c.CollId }) + "\">" + c.Name + "</a>"
                        });

                        if (DateTime.Compare(a.CreateDate, a.LastModifiedDate) != 0)
                        {

                            act.Add(new ActivityModel()
                            {
                                User = f,
                                Date = a.LastModifiedDate,
                                Description = f.UserName + " updated the article " + "<a href=\"" + Url.Action("Article", "User", new { id = a.ArticleId }) + "\">" + a.Name + "</a> of the collection <a href=\"" + Url.Action("Collection", "User", new { id = c.CollId }) + "\">" + c.Name + "</a>"
                            });

                        }

                    }

                }


                // Todas as compras que fez
                var negocios = from p in db.Negocios
                               where (p.UserIdVendedor == f.UserId || p.UserIdComprador == f.UserId) && (p.Estado == 1)
                               select p;

                foreach (var neg in negocios)
                {

                    var descN = "";
                    UserArticle uaux = db.UserArticles.Find(neg.ArtId);
                    if (uaux == null) {
                        continue;
                    }

                    if (neg.UserIdComprador == f.UserId)
                    {
                        descN = "bought";
                    }
                    else {
                        descN = "sold";
                    }

                    act.Add(new ActivityModel()
                    {
                        User = f,
                        Date = neg.ProposalDate,
                        Description = f.UserName + " " + descN + " the article " + "<a href=\"" + Url.Action("Article", "User", new { id = neg.ArtId }) + "\">" + uaux.Name + "</a>"
                    });

                }


	        }

            RecentActivityModel passar = new RecentActivityModel() {
                Profile = user,
                Activity = act.OrderByDescending(m => m.Date).ToList()
            };

            return View(passar);

        }

        [HttpPost]
        public JsonResult Search(string q)
        {

            List<object> ret = new List<object>();

            var coll = from c in db.UserCollections
                       where c.Name.ToLower().Contains(q.ToLower())
                       select c;

            var users = from u in db.UserProfiles
                        where u.UserName.ToLower().Contains(q.ToLower()) || u.UserEmail.ToLower().Contains(q.ToLower())
                        select u;

            var arts = from u in db.UserArticles
                       where u.Name.ToLower().Contains(q.ToLower())
                       select u;

            foreach (var c in coll)
            {
                ret.Add(new { label = c.Name, category = "Collections", id=c.CollId });
            }

            foreach (var c in arts)
            {
                ret.Add(new { label = c.Name, category = "Articles", id = c.ArticleId });
            }

            foreach (var c in users)
            {
                if(c.UserId!=WebSecurity.CurrentUserId)
                    ret.Add(new { label = c.UserName, category = "Collectors", id = c.UserId });
            }

            return Json(ret.Take(12));
        }

    }
}
