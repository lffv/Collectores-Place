using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;

namespace CollectorsPlace.Auths
{
    public static class Web
    {
        //
        // Get HTML 
        // Author : Murugan Andezuthu Dharmaratnam 
        // Date : Oct 04 2012 
        // Modified : Oct 08 2012 
        // Changed To Static Method 
        //
        public static string GetHTML(string URL)
        {
            string connectionString = URL;

            try
            {
                System.Net.HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(connectionString);
                myRequest.Credentials = CredentialCache.DefaultCredentials;
                //// Get the response
                WebResponse webResponse = myRequest.GetResponse();
                Stream respStream = webResponse.GetResponseStream();
                ////
                StreamReader ioStream = new StreamReader(respStream);
                string pageContent = ioStream.ReadToEnd();
                //// Close streams
                ioStream.Close();
                respStream.Close();
                return pageContent;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
