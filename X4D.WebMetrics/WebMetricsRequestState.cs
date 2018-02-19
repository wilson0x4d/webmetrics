using System.Diagnostics;
using System.Threading;
using System.Web;
using X4D.WebMetrics.IO;

namespace X4D.WebMetrics
{
	/// <summary>
	/// A state-container used to gather various request metrics during
	/// request processing.
	/// <para>
	/// Instances of <see cref="WebMetricsRequestState"/> should not be re-used.
	/// </para>
	/// </summary>
	public sealed class WebMetricsRequestState
	{
		/// <summary>
		/// a counter/sequencer variable used to derive a unique id for each
		/// <see cref="WebMetricsRequestState"/> instance.
		/// </summary>
		private static long s_requestStateCounter;

		private readonly Stopwatch _requestHandlerStopwatch;

		private readonly long _requestStateId;

		private readonly Stopwatch _requestStopwatch;

		private string _requestUri;
		private long _responseBodyLength;

		public WebMetricsRequestState(
			string requestUri)
		{
			_requestUri = requestUri;
			_requestStopwatch = new Stopwatch();
			_requestHandlerStopwatch = new Stopwatch();
			_requestStateId = Interlocked.Increment(ref s_requestStateCounter);
		}

		/// <summary>
		/// Total time spent in HTTP Handler, expressed as Milliseconds.
		/// </summary>
		public long RequestHandlerMilliseconds =>
			_requestHandlerStopwatch.ElapsedMilliseconds;

		/// <summary>
		/// Total time spent in processing they request, expressed as Milliseconds.
		/// </summary>
		public long RequestMilliseconds =>
			_requestStopwatch.ElapsedMilliseconds;

		/// <summary>
		/// a Request ID, it is considered unique within the scope of a
		/// single AppDomain.
		/// </summary>
		public long RequestStateId => _requestStateId;

		/// <summary>
		/// the original Request URL
		/// </summary>
		public string RequestUri => _requestUri;

		/// <summary>
		/// The Response Body Length, expressed as bytes.
		/// </summary>
		public long ResponseBodyLength =>
			_responseBodyLength;

		/// <summary>
		/// Observe the start of the request.
		/// </summary>
		internal void ObserveBeginRequest()
		{
			_requestStopwatch.Start();
		}

		/// <summary>
		/// Observe the start of HTTP Handler execution.
		/// </summary>
		internal void ObserveBeginRequestHandler()
		{
			_requestHandlerStopwatch.Start();
		}

		/// <summary>
		/// Observe the response to the request.
		/// </summary>
		/// <param name="response"></param>
		internal void ObserveResponse(HttpResponse response)
		{
			_responseBodyLength = GetResponseBodyLength(response);
		}

		/// <summary>
		/// Gets the Response Body Length (aka. Content Length)
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private long GetResponseBodyLength(HttpResponse response)
		{
			if (response.Filter is HtmlRewriteFilterStream filterStream)
			{
				return filterStream.ContentLength;
			}
			else
			{
				return 0L;
			}
		}
	}
}
