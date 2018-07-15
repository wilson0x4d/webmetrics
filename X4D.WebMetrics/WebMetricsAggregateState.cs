using X4D.Diagnostics.Counters;

namespace X4D.WebMetrics
{
    /// <summary>
    /// A state-container used to gether aggregate metrics during request processing.
    /// </summary>
    public sealed class WebMetricsAggregateState :
        CounterCategoryBase<WebMetricsAggregateState>
    {
        /// <summary>
        /// Average observed time spent in HTTP Handler, expressed in Milliseconds.
        /// </summary>
        public readonly MeanAverage RequestHandlerMillisecondsAverage;

        /// <summary>
        /// Maximum observed time spent in HTTP Handler, expressed in Milliseconds.
        /// </summary>
        public readonly ObservedValue RequestHandlerMillisecondsMaximum;

        /// <summary>
        /// Minimum observed time spent in HTTP Handler, expressed in Milliseconds.
        /// </summary>
        public readonly ObservedValue RequestHandlerMillisecondsMinimum;

        /// <summary>
        /// Average observed time spent processing HTTP Request, expressed in Milliseconds.
        /// </summary>
        public readonly MeanAverage RequestMillisecondsAverage;

        /// <summary>
        /// Maximum observed time spent processing HTTP Request, expressed in Milliseconds.
        /// </summary>
        public readonly ObservedValue RequestMillisecondsMaximum;

        /// <summary>
        /// Minimum observed time spent processing HTTP Request, expressed in Milliseconds.
        /// </summary>
        public readonly ObservedValue RequestMillisecondsMinimum;

        /// <summary>
        /// Average observed Response Body Length, expressed in Bytes.
        /// </summary>
        public readonly MeanAverage ResponseBodyLengthAverage;

        /// <summary>
        /// Maximum observed Response Body Length, expressed in Bytes.
        /// </summary>
        public readonly ObservedValue ResponseBodyLengthMaximum;

        /// <summary>
        /// Minimum observed Response Body Length, expressed in Bytes.
        /// </summary>
        public readonly ObservedValue ResponseBodyLengthMinimum;

        /// <summary>
        /// Total Count of Observed Requests.
        /// </summary>
        public readonly SumTotal TotalObservedRequests;

        /// <summary>
        /// Total Time spent Observing Requests, expressed as a <see cref="TimeSpan"/>.
        /// </summary>
        public readonly ElapsedTime TotalObservedTime;

        /// <summary>
        /// a lock which ensures atomicity of interdependent values.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// a Composite Counter for observing response bodies
        /// </summary>
        private readonly CompositeCounter _responseBodyLengthComposite;

        /// <summary>
        /// a Composite Counter for observing response bodies
        /// </summary>
        private readonly CompositeCounter _requestHandlerMillisecondsComposite;

        /// <summary>
        /// a Composite Counter for observing response bodies
        /// </summary>
        private readonly CompositeCounter _requestMillisecondsComposite;

        public WebMetricsAggregateState(string instanceName)
            : base(instanceName)
        {
            _lock = new object();

            TotalObservedTime = new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Milliseconds);

            TotalObservedRequests = new SumTotal(_lock);

            ResponseBodyLengthMinimum = new ObservedValue(ObservationType.Minimum);
            ResponseBodyLengthMaximum = new ObservedValue(ObservationType.Maximum);
            ResponseBodyLengthAverage = new MeanAverage(TotalObservedRequests, _lock);
            _responseBodyLengthComposite = new CompositeCounter(
                new ICounter<long>[]
                {
                    ResponseBodyLengthMinimum,
                    ResponseBodyLengthMaximum,
                    ResponseBodyLengthAverage
                },
                _lock);

            RequestMillisecondsMinimum = new ObservedValue(ObservationType.Minimum);
            RequestMillisecondsMaximum = new ObservedValue(ObservationType.Maximum);
            RequestMillisecondsAverage = new MeanAverage(TotalObservedRequests, _lock);
            _requestMillisecondsComposite = new CompositeCounter(
                new ICounter<long>[]
                {
                    RequestMillisecondsMinimum,
                    RequestMillisecondsMaximum,
                    RequestMillisecondsAverage
                },
                _lock);

            RequestHandlerMillisecondsMinimum = new ObservedValue(ObservationType.Minimum);
            RequestHandlerMillisecondsMaximum = new ObservedValue(ObservationType.Maximum);
            RequestHandlerMillisecondsAverage = new MeanAverage(TotalObservedRequests, _lock);
            _requestHandlerMillisecondsComposite = new CompositeCounter(
                new ICounter<long>[]
                {
                    RequestHandlerMillisecondsMinimum,
                    RequestHandlerMillisecondsMaximum,
                    RequestHandlerMillisecondsAverage
                },
                _lock);
        }

        /// <summary>
        /// Observes request state, updated aggregate metrics.
        /// </summary>
        /// <param name="requestState"></param>
        internal void ObserveRequestState(WebMetricsRequestState requestState)
        {
            lock (_lock)
            {
                _requestHandlerMillisecondsComposite.Increment(requestState.RequestHandlerMilliseconds);
                _requestMillisecondsComposite.Increment(requestState.RequestMilliseconds);
                _responseBodyLengthComposite.Increment(requestState.ResponseBodyLength);
                TotalObservedRequests.Increment();
            }
        }
    }
}
