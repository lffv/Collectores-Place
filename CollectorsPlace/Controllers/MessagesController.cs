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
    public class MessagesController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Messages/

        public ActionResult Index()
        {

            var tmsgs = db.Messages.Include("UserProfileSender").Where(u => u.UserIdReceiver == WebSecurity.CurrentUserId);

            ListarMensagens passar = new ListarMensagens() {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Messages = tmsgs.ToList()
            };

            return View(passar);
        }

        //
        // GET: /Messages/Details/5

        public ActionResult Details(int id = 0)
        {
            Message message = db.Messages.Include("UserProfileSender").Include("UserProfileReceiver").FirstOrDefault(m => m.MsgId == id);
            if (message == null || message.UserProfileReceiver.UserId!=WebSecurity.CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            ListarMensagem passar = new ListarMensagem() {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Message = message
            };

            if (message.Read == 0) {

                message.Read = 1;

                db.SaveChanges();

            }

            return View(passar);
        }

        //
        // GET: /Messages/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Messages/Create

        [HttpPost]
        public ActionResult Create(Message message)
        {
            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(message);
        }

        //
        // GET: /Messages/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Message message = db.Messages.Include("UserProfileSender").Include("UserProfileReceiver").FirstOrDefault(m => m.MsgId == id);
            if (message == null || message.UserProfileReceiver.UserId != WebSecurity.CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            ListarMensagem passar = new ListarMensagem()
            {
                Profile = db.UserProfiles.Find(WebSecurity.CurrentUserId),
                Message = message
            };

            return View(passar);
        }

        //
        // POST: /Messages/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {

            Message message = db.Messages.Include("UserProfileSender").Include("UserProfileReceiver").FirstOrDefault(m => m.MsgId == id);
            if (message == null || message.UserProfileReceiver.UserId != WebSecurity.CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            db.Messages.Remove(message);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}