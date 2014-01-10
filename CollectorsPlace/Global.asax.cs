using CollectorsPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CollectorsPlace
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

        }



        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                const string sessionParamName = "SessionId";
                const string sessionCookieName = "ASP.NET_SessionId";

                if (HttpContext.Current.Request.Form[sessionParamName] != null)
                {
                    UpdateCookie(sessionCookieName, HttpContext.Current.Request.Form[sessionParamName]);
                }
                else if (HttpContext.Current.Request.QueryString[sessionParamName] != null)
                {
                    UpdateCookie(sessionCookieName, HttpContext.Current.Request.QueryString[sessionParamName]);
                }
            }
            catch
            {
            }

            try
            {
                const string authParamName = "Token";
                string authCookieName = FormsAuthentication.FormsCookieName;

                if (HttpContext.Current.Request.Form[authParamName] != null)
                {
                    UpdateCookie(authCookieName, HttpContext.Current.Request.Form[authParamName]);
                }
                else if (HttpContext.Current.Request.QueryString[authParamName] != null)
                {
                    UpdateCookie(authCookieName, HttpContext.Current.Request.QueryString[authParamName]);
                }
            }
            catch
            {
            }
        }

        private static void UpdateCookie(string cookieName, string cookieValue)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(cookieName) ?? new HttpCookie(cookieName);
            cookie.Value = cookieValue;
            HttpContext.Current.Request.Cookies.Set(cookie);
        }





    }
}
