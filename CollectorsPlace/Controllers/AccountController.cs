using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CollectorsPlace.Filters;
using CollectorsPlace.Models;
using System.Net.Mail;
using SendGridMail;
using System.Net;
using SendGridMail.Transport;
using CollectorsPlace.Auths;
using System.IO;

namespace CollectorsPlace.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserEmail, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserEmail, model.Password, new { UserName = model.UserName, UserAvatar = "noavatar.jpg" });
                    WebSecurity.Login(model.UserEmail, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ResetPasswordModel model)
        {

            ViewBag.conf = "";

            if (ModelState.IsValid)
            {

                //check user existance
                var user = Membership.GetUser(model.UserEmail);
                if (user == null)
                {
                    ModelState.AddModelError("", "User Email does not exists.");
                }
                else
                {
                    //generate password token
                    var token = WebSecurity.GeneratePasswordResetToken(model.UserEmail);
                    //create url with above token
                    var resetLink = "<a href='" + Url.Action("ResetPassword", "Account", new { un = model.UserEmail, rt = token }, "http") + "'>Reset Password</a>";
                    //get user emailid
                    var emailid = model.UserEmail;
                    //send mail
                    string subject = "Password Reset Instructions";
                    string body = "<b>Hi</b><br/ ><br/ >You request a Password Reset, if not, please ignore this email.<br /><br />To Reset your password, please visit the next link:<br /><br />" + resetLink + "<br /><br />Best Regards, <br /><br />Collectors Place";
                    try
                    {
                        SendEMail(emailid, subject, body);
                        ViewBag.conf = "An email with instructions was sent to you!";
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Error occured while sending email.");
                    }

                }

                return View();

            }

            return View(model);

        }

        //
        // GET: /Account/ResetPassword

        [AllowAnonymous]
        public ActionResult ResetPassword(string un, string rt)
        {
            UsersContext db = new UsersContext();
            //TODO: Check the un and rt matching and then perform following
            //get userid of received username
            var userid = (from i in db.UserProfiles
                          where i.UserEmail == un
                          select i.UserId).FirstOrDefault();
            //check userid and token matches
            bool any = (from j in db.webpages_Memberships
                        where (j.UserId == userid)
                        && (j.PasswordVerificationToken == rt)
                        //&& (j.PasswordVerificationTokenExpirationDate < DateTime.Now)
                        select j).Any();

            if (any == true)
            {
                //generate random password
                string newpassword = GenerateRandomPassword(18);
                //reset password
                bool response = WebSecurity.ResetPassword(rt, newpassword);
                if (response == true)
                {
                    //get user emailid to send password
                    var emailid = un;
                    //send email
                    string subject = "New Password";
                    string body = "<b>Hi</b><br/ ><br/ >Your new Password to access Collectors Place is:<br /><br />" + newpassword + "<br /><br />Please, change the password as soon as possible!<br /><br />Best Regards, <br /><br />Collectors Place";
                    try
                    {
                        SendEMail(emailid, subject, body);
                        ViewBag.conf = "Mail Sent.";
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Error occured while sending email.");
                    }

                    ViewBag.conf = "Your new Password was sent to your Email!";
                }
                else
                {
                    RedirectToAction("ForgotPassword"); // Sem acesso directo...
                }
            }
            else
            {
                ModelState.AddModelError("", "Error reseting your Password, please confirm your request link");
            }

            return View("ForgotPassword");
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase Filedata)
        {

            var name = WebSecurity.CurrentUserId + "." + Filedata.FileName.Substring(Filedata.FileName.LastIndexOf('.') + 1);
            string savePath = Server.MapPath(@"~\Images\avatars\" + name);
            Filedata.SaveAs(savePath);

            // Actualiza na base de dados:

            UsersContext db = new UsersContext();

            UserProfile novo = new UserProfile();
            novo.UserId = WebSecurity.CurrentUserId;

            db.UserProfiles.Attach(novo);

            novo.UserAvatar = name;

            db.SaveChanges();

            return Content(Url.Content(@"~\Images\avatars\" + name));

        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");

            LocalPasswordModel passar = new LocalPasswordModel()
            {
                Profile = new UsersContext().UserProfiles.Find(WebSecurity.CurrentUserId)
            };

            return View(passar);
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {

            // Fix para pedir tambem o email no facebook:
            if (provider.ToLower() == "facebook")
            {
                returnUrl = Url.Action("ExternalLoginCallbackFB", new { ReturnUrl = returnUrl });
                return Redirect("https://www.facebook.com/dialog/oauth?client_id=" + new FacebookOAuth().AppID + "&redirect_uri=" + HttpUtility.HtmlEncode("h" + System.Web.HttpContext.Current.Request.Url.ToString().Substring("h", "/Account") + returnUrl) + "%3F__provider__%3Dfacebook&scope=email");
            }

            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));

        }

        // Apenas para o Facebook
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallbackFB(string returnUrl)
        {

            string code = Request.QueryString["code"];
            string returnUrl1 = Url.Action("ExternalLoginCallbackFB", new { ReturnUrl = returnUrl });

            IDictionary<string, string> userData = new FacebookOAuth().GetUserData(code, HttpUtility.HtmlEncode("h" + System.Web.HttpContext.Current.Request.Url.ToString().Substring("h", "/Account") + returnUrl1));

            if(userData==null){
                return RedirectToAction("ExternalLoginFailure");
            }

            AuthenticationResult result = new AuthenticationResult(isSuccessful: true, provider: "facebook", providerUserId: userData["id"], userName: userData["username"], extraData: userData);

            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;

                var nome = decodeInternationl(userData["name"]);
                var email= userData["email"];

                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = nome, ExternalLoginData = loginData, UserEmail = email });


            }
        }

        // Apenas para o Google
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }
            
            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;

                var dados = result.ExtraData;
                var nome = dados["firstName"] + " " + dados["lastName"];
                var email = dados["email"];

                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = nome, ExternalLoginData = loginData, UserEmail = email });
                

            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserEmail.ToLower() == model.UserEmail.ToLower());
                    // Check if user email already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName, UserEmail = model.UserEmail });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserEmail);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserEmail", "User email already exists.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private string GenerateRandomPassword(int length)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-*&#+";
            char[] chars = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }

        private void SendEMail(string emailid, string subject, string body)
        {

            var sendFrom = new MailAddress("no-reply@collectorsplace.azurewebsites.net", "Collectors Place");
            var sendTo = new MailAddress(emailid);
            var email = SendGrid.GetInstance();
            email.To = new MailAddress[] { sendTo };
            email.From = sendFrom;
            email.Subject = subject;
            email.Html = body;
            var credentials = new NetworkCredential("azure_27c81faa67306eac404c72f664a9b9dd@azure.com", "jcti4o6o");
            var transport = SMTP.GetInstance(credentials);
            transport.Deliver(email);

        }

        private string decodeInternationl(string strInput)
        {

            string strOutput = "";

            strInput = strInput.Replace("\\u00c0", "À");
            strInput = strInput.Replace("\\u00c1", "Á");
            strInput = strInput.Replace("\\u00c2", "Â");
            strInput = strInput.Replace("\\u00c3", "Ã");
            strInput = strInput.Replace("\\u00c4", "Ä");
            strInput = strInput.Replace("\\u00c5", "Å");
            strInput = strInput.Replace("\\u00c6", "Æ");
            strInput = strInput.Replace("\\u00c7", "Ç");
            strInput = strInput.Replace("\\u00c8", "È");
            strInput = strInput.Replace("\\u00c9", "É");
            strInput = strInput.Replace("\\u00ca", "Ê");
            strInput = strInput.Replace("\\u00cb", "Ë");
            strInput = strInput.Replace("\\u00cc", "Ì");
            strInput = strInput.Replace("\\u00cd", "Í");
            strInput = strInput.Replace("\\u00ce", "Î");
            strInput = strInput.Replace("\\u00cf", "Ï");
            strInput = strInput.Replace("\\u00d1", "Ñ");
            strInput = strInput.Replace("\\u00d2", "Ò");
            strInput = strInput.Replace("\\u00d3", "Ó");
            strInput = strInput.Replace("\\u00d4", "Ô");
            strInput = strInput.Replace("\\u00d5", "Õ");
            strInput = strInput.Replace("\\u00d6", "Ö");
            strInput = strInput.Replace("\\u00d8", "Ø");
            strInput = strInput.Replace("\\u00d9", "Ù");
            strInput = strInput.Replace("\\u00da", "Ú");
            strInput = strInput.Replace("\\u00db", "Û");
            strInput = strInput.Replace("\\u00dc", "Ü");
            strInput = strInput.Replace("\\u00dd", "Ý");

            strInput = strInput.Replace("\\u00df", "ß");
            strInput = strInput.Replace("\\u00e0", "à");
            strInput = strInput.Replace("\\u00e1", "á");
            strInput = strInput.Replace("\\u00e2", "â");
            strInput = strInput.Replace("\\u00e3", "ã");
            strInput = strInput.Replace("\\u00e4", "ä");
            strInput = strInput.Replace("\\u00e5", "å");
            strInput = strInput.Replace("\\u00e6", "æ");
            strInput = strInput.Replace("\\u00e7", "ç");
            strInput = strInput.Replace("\\u00e8", "è");
            strInput = strInput.Replace("\\u00e9", "é");
            strInput = strInput.Replace("\\u00ea", "ê");
            strInput = strInput.Replace("\\u00eb", "ë");
            strInput = strInput.Replace("\\u00ec", "ì");
            strInput = strInput.Replace("\\u00ed", "í");
            strInput = strInput.Replace("\\u00ee", "î");
            strInput = strInput.Replace("\\u00ef", "ï");
            strInput = strInput.Replace("\\u00f0", "ð");
            strInput = strInput.Replace("\\u00f1", "ñ");
            strInput = strInput.Replace("\\u00f2", "ò");
            strInput = strInput.Replace("\\u00f3", "ó");
            strInput = strInput.Replace("\\u00f4", "ô");
            strInput = strInput.Replace("\\u00f5", "õ");
            strInput = strInput.Replace("\\u00f6", "ö");
            strInput = strInput.Replace("\\u00f8", "ø");
            strInput = strInput.Replace("\\u00f9", "ù");
            strInput = strInput.Replace("\\u00fa", "ú");
            strInput = strInput.Replace("\\u00fb", "û");
            strInput = strInput.Replace("\\u00fc", "ü");
            strInput = strInput.Replace("\\u00fd", "ý");
            strInput = strInput.Replace("\\u00ff", "ÿ");

            strOutput = strInput;

            return strOutput;

        }

        #endregion

    }

}
