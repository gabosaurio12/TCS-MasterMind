using System;
using System.Web;

namespace MasterMind_Client.Helpers
{
    public static class RouteQuery
    {
        public static int GetInt(Uri uri, string key)
        {
            string queryString = uri.IsAbsoluteUri ? uri.Query : GetQueryFromRelativeUri(uri);
            var query = HttpUtility.ParseQueryString(queryString);

            return int.TryParse(query[key], out int value) ? value : 0;
        }

        private static string GetQueryFromRelativeUri(Uri uri)
        {
            string original = uri.OriginalString;
            int queryIndex = original.IndexOf('?');

            return queryIndex >= 0 ? original.Substring(queryIndex) : string.Empty;
        }
    }
}
