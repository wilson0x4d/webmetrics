using System;
using System.Web;

namespace X4D.WebMetrics
{
    public sealed class WebMetricsHttpModule :
        IHttpModule
    {
        /// <summary>
        /// An aggregate view of statistics gathered by this instance of the
        /// module in the current appdomain.
        /// </summary>
        private static readonly WebMetricsAggregateState _aggregateState =
            new WebMetricsAggregateState();

        /// <summary>
        /// the <see cref="HttpContext.Items"/> key used to ferry request state
        /// </summary>
        private static readonly string HTTPCONTEXT_WEBMETRICS_REQUESTSTATE_KEY = @"X4D_WMRS";

        /// <summary>
        /// Boilerplate, no unmanaged resources are owned by this module and
        /// therefore no disposal logic is implemented.
        /// </summary>
        public void Dispose()
        {
            // NOP
        }

        /// <summary>
        /// Standard <see cref="Init(HttpApplication)"/>, responsible for
        /// wiring up various event handlers which gather metrics.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        /// <summary>
        /// We observe the `BeginRequest` event to allocate request state and
        /// capture the request start time.
        /// </summary>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            // NOTE: we remove Accept-Encoding and replace with "identity" to
            //       avoid opportunity for response filter to receive
            //       compressed content
            if (AppDomain.CurrentDomain.FriendlyName.Contains("/W3SVC/"))
            {
                HttpContext.Current.Request.Headers.Remove(
                    "Accept-Encoding");
                HttpContext.Current.Request.Headers.Add(
                    "Accept-Encoding",
                    "identity");
            }
            var requestState = new WebMetricsRequestState(
                HttpContext.Current.Request.RawUrl);
            HttpContext.Current.Items[HTTPCONTEXT_WEBMETRICS_REQUESTSTATE_KEY] = requestState;
            HttpContext.Current.Response.BufferOutput = true;
            HttpContext.Current.Response.Filter = new IO.HtmlRewriteFilterStream(
                HttpContext.Current.Response,
                requestState,
                _aggregateState);
            requestState.ObserveBeginRequest();
        }

        /// <summary>
        /// Hook into "Request Handler" pre-execute completion to capture a
        /// start time for Request Handler processing.
        /// </summary>
        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Items[HTTPCONTEXT_WEBMETRICS_REQUESTSTATE_KEY]
                is WebMetricsRequestState requestState)
            {
                requestState.ObserveBeginRequestHandler();
            }
        }
    }
}
