
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;

namespace CollectorsPlace
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "261318630671704",
                appSecret: "d8048f4f092ef6994b3502faaa58c5c6");

            //OAuthWebSecurity.RegisterGoogleClient("Google", 

            // Fix para outros campos:

            // Google:
            OAuthWebSecurity.RegisterClient(new CollectorsPlace.Auths.GoogleCustomClient(), "Google", null);

        }

    }
}
