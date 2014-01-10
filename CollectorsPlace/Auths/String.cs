using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CollectorsPlace.Auths
{
    public static class String
    {
        public static string Substring(this string str, string StartString, string EndString)
        {
            if (str.Contains(StartString))
            {
                int iStart = str.IndexOf(StartString) + StartString.Length;
                int iEnd = str.IndexOf(EndString, iStart);
                return str.Substring(iStart, (iEnd - iStart));
            }
            return null;
        }
    }
}
