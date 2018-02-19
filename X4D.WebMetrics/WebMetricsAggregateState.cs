using System;

namespace X4D.WebMetrics
{
	/// <summary>
	/// A state-container used to gether aggregate metrics during request processing.
	/// </summary>
	public sealed class WebMetricsAggregateState
	{
		private DateTime _aggregationStartTime = DateTime.UtcNow;

		/// <summary>
		/// a lock which ensures atomicity of cardinal and averaged values.
		/// </summary>
		private object _lock = new object();

		private long _requestHandlerMillisecondsAverage = 0L;

		private long _requestHandlerMillisecondsMaximum = 0L;

		private long _requestHandlerMillisecondsMinimum = 0L;

		private long _requestMillisecondsAverage = 0L;

		private long _requestMillisecondsMaximum = 0L;

		private long _requestMillisecondsMinimum = 0L;

		private long _responseBodyLengthAverage = 0L;

		private long _responseBodyLengthMaximum = 0L;

		private long _responseBodyLengthMinimum = 0L;

		private long _totalObservedRequests = 0L;

		/// <summary>
		/// Average observed time spent in HTTP Handler, expressed in Milliseconds.
		/// </summary>
		public long RequestHandlerMillisecondsAverage =>
			_requestHandlerMillisecondsAverage;

		/// <summary>
		/// Maximum observed time spent in HTTP Handler, expressed in Milliseconds.
		/// </summary>
		public long RequestHandlerMillisecondsMaximum =>
			_requestHandlerMillisecondsMaximum;

		/// <summary>
		/// Minimum observed time spent in HTTP Handler, expressed in Milliseconds.
		/// </summary>
		public long RequestHandlerMillisecondsMinimum =>
			_requestHandlerMillisecondsMinimum;

		/// <summary>
		/// Average observed time spent processing HTTP Request, expressed in Milliseconds.
		/// </summary>
		public long RequestMillisecondsAverage =>
			_requestMillisecondsAverage;

		/// <summary>
		/// Maximum observed time spent processing HTTP Request, expressed in Milliseconds.
		/// </summary>
		public long RequestMillisecondsMaximum =>
			_requestMillisecondsMaximum;

		/// <summary>
		/// Minimum observed time spent processing HTTP Request, expressed in Milliseconds.
		/// </summary>
		public long RequestMillisecondsMinimum =>
			_requestMillisecondsMinimum;

		/// <summary>
		/// Average observed Response Body Length, expressed in Bytes.
		/// </summary>
		public long ResponseBodyLengthAverage =>
			_responseBodyLengthAverage;

		/// <summary>
		/// Maximum observed Response Body Length, expressed in Bytes.
		/// </summary>
		public long ResponseBodyLengthMaximum =>
			_responseBodyLengthMaximum;

		/// <summary>
		/// Minimum observed Response Body Length, expressed in Bytes.
		/// </summary>
		public long ResponseBodyLengthMinimum =>
			_responseBodyLengthMinimum;

		/// <summary>
		/// Total Count of Observed Requests.
		/// </summary>
		public long TotalObservedRequests =>
			_totalObservedRequests;

		/// <summary>
		/// Total Time spent Observing Requests, expressed as a <see cref="TimeSpan"/>.
		/// </summary>
		public TimeSpan TotalObservedTime =>
			DateTime.UtcNow - _aggregationStartTime;

		/// <summary>
		/// Observes request state, updated aggregate metrics.
		/// </summary>
		/// <param name="requestState"></param>
		internal void ObserveRequestState(
			WebMetricsRequestState requestState)
		{
			lock (_lock)
			{
				if (_totalObservedRequests == 0)
				{
					// response body metrics
					_responseBodyLengthMaximum = requestState.ResponseBodyLength;
					_responseBodyLengthMinimum = requestState.ResponseBodyLength;
					_responseBodyLengthAverage = requestState.ResponseBodyLength;
					// request metrics
					_requestMillisecondsMaximum = requestState.RequestMilliseconds;
					_requestMillisecondsMinimum = requestState.RequestMilliseconds;
					_requestMillisecondsAverage = requestState.RequestMilliseconds;
					// request handler metrics
					_requestHandlerMillisecondsMaximum = requestState.RequestHandlerMilliseconds;
					_requestHandlerMillisecondsMinimum = requestState.RequestHandlerMilliseconds;
					_requestHandlerMillisecondsAverage = requestState.RequestHandlerMilliseconds;
				}
				else
				{
					// response body metrics
					if (requestState.ResponseBodyLength > _responseBodyLengthMaximum)
					{
						_responseBodyLengthMaximum = requestState.ResponseBodyLength;
					}
					if (requestState.ResponseBodyLength < _responseBodyLengthMinimum)
					{
						_responseBodyLengthMinimum = requestState.ResponseBodyLength;
					}
					_responseBodyLengthAverage = (_responseBodyLengthAverage + requestState.ResponseBodyLength) / 2;
					// request metrics
					if (requestState.RequestMilliseconds > _requestMillisecondsMaximum)
					{
						_requestMillisecondsMaximum = requestState.RequestMilliseconds;
					}
					if (requestState.RequestMilliseconds < _requestMillisecondsMinimum)
					{
						_requestMillisecondsMinimum = requestState.RequestMilliseconds;
					}
					_requestMillisecondsAverage = (_requestMillisecondsAverage + requestState.RequestMilliseconds) / 2;
					// request handler metrics
					if (requestState.RequestHandlerMilliseconds > _requestHandlerMillisecondsMaximum)
					{
						_requestHandlerMillisecondsMaximum = requestState.RequestHandlerMilliseconds;
					}
					if (requestState.RequestHandlerMilliseconds < _requestHandlerMillisecondsMinimum)
					{
						_requestHandlerMillisecondsMinimum = requestState.RequestHandlerMilliseconds;
					}
					_requestHandlerMillisecondsAverage = (_requestHandlerMillisecondsAverage + requestState.RequestHandlerMilliseconds) / 2;
				}
				_totalObservedRequests++;
			}
		}
	}
}
