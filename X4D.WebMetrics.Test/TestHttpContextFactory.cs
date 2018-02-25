using System.IO;
using System.Web;

namespace X4D.WebMetrics.Test
{
    public static class TestHttpContextFactory
    {
        /// <summary>
        /// Creates an HttpContext that can be reached via
        /// HttpContext.Curremt within dependent test code.
        /// </summary>
        public static HttpContext Create(
            string uri = "http://localhost",
            Stream responseStream = null)
        {
            HttpContext.Current =
                new HttpContext(
                    new HttpRequest("", uri, ""),
                    new HttpResponse(
                        new StreamWriter(responseStream ?? Stream.Null)));

            return HttpContext.Current;
        }
    }
}
