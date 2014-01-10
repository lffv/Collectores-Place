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

namespace CollectorsPlace.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class BusinessController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Business/

        public ActionResult Index()
        {

            var neg = from p in db.Negocios
                      where (p.UserIdVendedor==WebSecurity.CurrentUserId || p.UserIdComprador==WebSecurity.CurrentUserId) && (p.Estado==1)
                      select p;

            var neg2 = from p in db.Negocios
                      where (p.UserIdVendedor == WebSecurity.CurrentUserId || p.UserIdComprador == WebSecurity.CurrentUserId) && (p.Estado == 0)
                      select p;

            ListNegocios passar = new ListNegocios()
            {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Negocios = neg.ToList(),
                NegociosPending = neg2.ToList(),
                Articles = db.UserArticles,
                Users = db.UserProfiles
            };

            return View(passar);
        }

        public ActionResult Proposta(int id = 0) {

            var neg = db.Negocios.Include(p => p.ArtTroca).FirstOrDefault(a => a.NegId == id);

            if (neg == null) {
                return RedirectToAction("Index");
            }
            
            ListNegocio passar = new ListNegocio() { 
                Negocio = neg,
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                UserComprador = db.UserProfiles.Find(neg.UserIdComprador),
                UserVendedor = db.UserProfiles.Find(neg.UserIdVendedor)
            };

            passar.Articles = neg.ArtTroca;

            return View(passar);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptBusiness(int id = 0) {

            var neg = db.Negocios.Include(p => p.ArtTroca).FirstOrDefault(a => a.NegId == id);

            if (neg == null)
            {
                return RedirectToAction("Index");
            }

            UserCollection novacollcomp=null, novacollvend=null;

            if (neg.ArtTroca.Count() > 0)
            {

                // Cria coleção no utilizador que comprou, com os artigos
                var dtn = DateTime.Now;

                novacollcomp = new UserCollection()
                {
                    Name = "Artigos do Negocio nº " + neg.NegId,
                    Description = "Os artigos recebidos por Troca do negócio nº " + neg.NegId + " a " + neg.ProposalDate,
                    CreateDate = dtn,
                    LastModifiedDate = dtn,
                    PrivacyType = 0,
                    UserId = neg.UserIdVendedor,
                    CollAvatar="noimage.jpg"
                };

                db.UserCollections.Add(novacollcomp);
                db.SaveChanges();

                novacollvend = new UserCollection()
                {
                    Name = "Artigos do Negocio nº " + neg.NegId,
                    Description = "O artigo comprado no negócio nº " + neg.NegId + " a " + neg.ProposalDate,
                    CreateDate = dtn,
                    LastModifiedDate = dtn,
                    PrivacyType = 0,
                    UserId = neg.UserIdComprador,
                    CollAvatar = "noimage.jpg"
                };

                db.UserCollections.Add(novacollvend);
                db.SaveChanges();

                foreach (var art in neg.ArtTroca.ToList())
                {

                    UserArticle novo = new UserArticle()
                    {
                        AcquiredDate = DateTime.Now,
                        BusinessDisponibility = 0,
                        CollId = novacollcomp.CollId,
                        CreateDate = DateTime.Now,
                        Description = art.Description,
                        EstimatedValue = art.EstimatedValue,
                        LastModifiedDate = DateTime.Now,
                        Name = art.Name,
                        CollAvatar = art.CollAvatar
                    };

                    var tags = getTagsByArticle(id);

                    foreach (string tag in tags)
                    {

                        var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                        if (ex == null)
                        {

                            novo.ArtTags.Add(new Tag() { Name = tag });
                            novacollcomp.CollTags.Add(new Tag() { Name = tag });
                        }
                        else
                        {
                            novo.ArtTags.Add(ex);
                            novacollcomp.CollTags.Add(ex);
                        }

                    }

                    db.UserArticles.Add(novo);
                    db.UserArticles.Remove(art);

                }

                db.SaveChanges();

            }

            UserArticle artneg = db.UserArticles.Find(neg.ArtId);

            UserArticle novoartneg = new UserArticle() {
                
                AcquiredDate=DateTime.Now,
                BusinessDisponibility=0,
                CollAvatar=artneg.CollAvatar,
                CollId=novacollvend.CollId,
                CreateDate=DateTime.Now,
                Description=artneg.Description,
                EstimatedValue=artneg.EstimatedValue,
                LastModifiedDate=DateTime.Now,
                Name=artneg.Name,
                UserCollection=novacollvend
            };

            var ntags = getTagsByArticle(id);

            foreach (string tag in ntags)
            {

                var ex = db.Tags.FirstOrDefault(m => m.Name == tag);

                if (ex == null)
                {

                    novoartneg.ArtTags.Add(new Tag() { Name = tag });
                }
                else
                {
                    novoartneg.ArtTags.Add(ex);
                }

            }

            db.UserArticles.Add(novoartneg);
            db.UserArticles.Remove(artneg);

            db.SaveChanges();

            Negocio ngnovo = new Negocio()
            {
                ArtId = novoartneg.ArticleId,
                Estado = 1, // Foi vendido
                ProposalDate = DateTime.Now,
                descricao = neg.descricao,
                tipo = 1,
                valor = novoartneg.EstimatedValue,
                UserIdComprador = novacollvend.UserId,
                UserIdVendedor = novacollcomp.UserId,
                Article = novoartneg,
                UserProfileVendedor = db.UserProfiles.Find(neg.UserIdVendedor),
                UserProfileComprador = db.UserProfiles.Find(neg.UserIdVendedor)
            };

            db.Negocios.Add(ngnovo);
            db.Negocios.Remove(neg);

            db.SaveChanges();

            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveProposta(int id=0) {

            Negocio neg = db.Negocios.Find(id);

            if (neg == null)
            {
                return RedirectToAction("Index");
            }
            db.Negocios.Remove(neg);
            db.SaveChanges();

            return RedirectToAction("Index");

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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}