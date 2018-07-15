using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace X4D.WebMetrics.IO
{
    /// <summary>
    /// A filter stream that will rewrite HTML content as it is written to the output stream.
    /// </summary>
    public sealed class HtmlRewriteFilterStream :
        Stream
    {
        /// <summary>
        /// a Regex applied to stream content to locate HTML
        /// <![CDATA[`</body>`]]> tag for rewriting.
        /// </summary>
        private static readonly Regex s_matchRegex =
            new Regex(
                @"(\</body\>[\s]*\</html\>|\</body\>)[\s]*$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// a set of supported Content Types, stream content is not rewritten
        /// unless the Content Type matches one of these elements
        /// </summary>
        private static string[] s_supportedContentTypes =
            new[]
            {
                "text/html",
                "text/plain"
            };

        /// <summary>
        /// the Aggregate State used during rewrite
        /// </summary>
        private readonly WebMetricsAggregateState _aggregateState;

        /// <summary>
        /// the un-altered Input Stream that writes are buffered into
        /// </summary>
        private readonly MemoryStream _inputStream;

        /// <summary>
        /// the Request State used during rewrite
        /// </summary>
        private readonly WebMetricsRequestState _requestState;

        /// <summary>
        /// the Content Length of data written to the Input Stream
        /// <para>tracked independently to not rely on Input Stream state</para>
        /// </summary>
        private long _contentLength = 0L;

        /// <summary>
        /// the <see cref="HttpResponse"/> this filter stream has been
        /// associated with
        /// </summary>
        private HttpResponse _httpResponse;

        /// <summary>
        /// a <see cref="StreamReader"/> used for ingesting the Input Stream
        /// </summary>
        private StreamReader _inputStreamReader;

        /// <summary>
        /// indicates that this stream has been Disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// the Output Stream that altered content is written to
        /// </summary>
        private Stream _outputStream;

        /// <summary>
        /// the current Input Stream read position
        /// <para>tracked independently to not rely on Input Stream state</para>
        /// </summary>
        private long _readPosition = 0L;

        /// <summary>
        /// Constructs an instance of <see cref="HtmlRewriteFilterStream"/>.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="requestState"></param>
        /// <param name="aggregateState"></param>
        public HtmlRewriteFilterStream(
            HttpResponse httpResponse,
            WebMetricsRequestState requestState,
            WebMetricsAggregateState aggregateState)
        {
            // NOTE: this coallesce only exists for unit-test scenarios where
            //       `Filter` property is not initialized by ASP.NET pipeline
            _outputStream = httpResponse.Filter
                ?? (httpResponse.Output as StreamWriter).BaseStream;
            _httpResponse = httpResponse;
            _requestState = requestState;
            _aggregateState = aggregateState;
            _inputStream = new MemoryStream();
        }

        /// <summary>
        /// Gets a value indicating that <see cref="Read"/> is supported.
        /// <para>Returns false.</para>
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Gets a value indicating that <see cref="Seek"/> is supported.
        /// <para>Returns false.</para>
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating that <see cref="Write"/> is supported.
        /// <para>Returns true.</para>
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the Input Stream Content Length.
        /// </summary>
        public long ContentLength => _contentLength;

        /// <summary>
        /// Gets the current Input Stream Position
        /// <para>Setter is Not Supported</para>
        /// </summary>
        public override long Position
        {
            get => _inputStream.Position;
            set => throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.set_{nameof(Position)}");
        }

        /// <summary>
        /// Gets the Input Stream Length
        /// </summary>
        public override long Length => _inputStream.Length;

        /// <summary>
        /// Closes the Stream, after calling <see cref="Flush"/>.
        /// <para>Does not perform closure of Input nor Output streams.</para>
        /// </summary>
        public override void Close()
        {
            Flush();
            base.Close();
        }

        /// <summary>
        /// Flushes Input and Ouput streams, performing a rewrite of
        /// yet-unritten buffered input.
        /// </summary>
        public override void Flush()
        {
            if (!_isDisposed)
            {
                _inputStream.Flush();
                WriteBufferedInputToOutput(true);
                _outputStream.Flush();
            }
        }

        /// <summary>
        /// Not Supported
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.{nameof(Read)}");
        }

        /// <summary>
        /// Not Supported
        /// </summary>
        public override int ReadByte()
        {
            throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.{nameof(ReadByte)}");
        }

        /// <summary>
        /// Not Supported
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.{nameof(Seek)}");
        }

        /// <summary>
        /// Not Supported
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.{nameof(SetLength)}");
        }

        /// <summary>
        /// Writes data to buffered Input Stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inputStream.Write(buffer, offset, count);
            _contentLength = _inputStream.Length;
            WriteBufferedInputToOutput();
        }

        /// <summary>
        /// Not Supported
        /// </summary>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException($"{nameof(HtmlRewriteFilterStream)}.{nameof(Write)}");
        }

        /// <summary>
        /// Disposes this object, the Input Stream and the Output Stream.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Trace.WriteLine("Filter Stream Disposed");
            _isDisposed = true;
            if (disposing)
            {
                if (_inputStreamReader != null)
                {
                    GC.SuppressFinalize(_inputStreamReader);
                }
                GC.SuppressFinalize(_inputStream);
                GC.SuppressFinalize(_outputStream);
                GC.SuppressFinalize(this);
            }
            if (_inputStreamReader != null)
            {
                _inputStreamReader.Dispose();
            }
            else
            {
                _inputStream.Dispose();
            }
            _outputStream.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Generates an HTML Fragment based on Request State and Aggregate
        /// State, to be injected into HTML Body for display in-browser.
        /// </summary>
        /// <param name="matchValue"></param>
        /// <param name="requestState"></param>
        /// <param name="aggregateState"></param>
        /// <returns></returns>
        private string GenerateHtmlFragment(
            string matchValue,
            WebMetricsRequestState requestState,
            WebMetricsAggregateState aggregateState)
        {
            requestState.ObserveResponse(HttpContext.Current.Response);
            aggregateState.ObserveRequestState(requestState);
            var avgRequestsPerMinute = aggregateState.TotalObservedRequests.Value / (aggregateState.TotalObservedTime.Value + 1);
            return
                $@"<div class=""x4d-webmetrics"" style=""position:fixed;bottom:8px;right:8px;z-index:93600;text-align:right;font-size:0.7em"">" +
                $@"<div class=""x4d-timemetrics"">Request {requestState.RequestMilliseconds}ms, " +
                $@"Handler {requestState.RequestHandlerMilliseconds}ms</div>" +
                $@"<div class=""x4d-sizemetrics"">Min {aggregateState.ResponseBodyLengthMinimum}bytes, " +
                $@"Max {aggregateState.ResponseBodyLengthMaximum}bytes, " +
                $@"Avg. {aggregateState.ResponseBodyLengthAverage}bytes</div>" +
                $@"<div class=""x4d-miscmetrics"">{aggregateState.TotalObservedRequests} requests @ " +
                $"{(avgRequestsPerMinute > aggregateState.TotalObservedRequests.Value ? aggregateState.TotalObservedRequests.Value : avgRequestsPerMinute)}req/min</div></div></body>" +
                $"{(matchValue.Contains("</html>") ? "</html>" : default)}";
        }

        /// <summary>
        /// Gets or Creates an Input Stream <see cref="StreamReader"/>.
        /// </summary>
        /// <returns></returns>
        private StreamReader GetOrCreateInputStreamReader()
        {
            if (_inputStreamReader == null)
            {
                _inputStreamReader = new StreamReader(
                    _inputStream,
                    _httpResponse.ContentEncoding);
            }
            return _inputStreamReader;
        }

        /// <summary>
        /// Determines of the response content type is supported by this filter.
        /// </summary>
        /// <returns>True if supported, otherwise False.</returns>
        private bool IsContentTypeSupported()
        {
            var requestContentType = _httpResponse.ContentType;
            for (int i = 0; i < s_supportedContentTypes.Length; i++)
            {
                if (requestContentType.StartsWith(s_supportedContentTypes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// reads unaltered content from Input Stream, attempts to perform
        /// rewrites, and then writes rewritten content to Output Stream.
        /// </summary>
        /// <param name="allowReadToEnd"></param>
        private void WriteBufferedInputToOutput(bool allowReadToEnd = false)
        {
            var writePosition = _inputStream.Position;
            if (_readPosition == writePosition)
            {
                // nothing to write, or already written
                return;
            }
            _inputStream.Seek(_readPosition, SeekOrigin.Begin);
            try
            {
                if (!IsContentTypeSupported())
                {
                    // for unsupported content types, perform verbatim copy
                    StreamExtensions.CopyTo(_inputStream, _outputStream);
                    _readPosition = _inputStream.Position;
                }
                else
                {
                    // TODO: there is room for improvement in how rewrites
                    //       are performed, this approach is simply an
                    //       initial "robust" option that is less error
                    //       prone; requiring less debugging and testing.
                    //
                    // 01. we could avoid reliance on `StreamReader` is not a
                    //     hard requirement, this is just a convenient way to
                    //     ingest the stream content without having to deal
                    //     with discrete differences in encodings.
                    //
                    // 02. we could rework how the matching and replacement
                    //     is performed to eliminate the reliance on a regex,
                    //     a regex is just a convenient way to locate matches
                    //     in a way that minimizes "false positives" for
                    //     content that may not be DOM contnet (ie. JS
                    //     strings contianing a `body` tag or similar.)
                    //
                    // 03. we could perform chunked writes, and the Write()
                    //     and Flush() methods are coded to support such a
                    //     scenario, however the current implementation of
                    //     this method brutishly ingests, scans, modifies and
                    //     then outputs the entire input string in one shot.

                    var inputStreamReader = GetOrCreateInputStreamReader();
                    var replacementString = default(string);
                    // TODO: support "chunked" rewrite, which would have less
                    //       overhead for very large documents, but which
                    //       requires additional debugging.
                    if (allowReadToEnd && _readPosition != writePosition)
                    {
                        var line = inputStreamReader.ReadToEnd();
                        if (line != null)
                        {
                            _readPosition = _inputStream.Position;
                            var matches = s_matchRegex.Matches(line);
                            if (matches.Count > 0)
                            {
                                var match = matches[matches.Count - 1];
                                replacementString = replacementString ?? GenerateHtmlFragment(
                                    match.Value,
                                    _requestState,
                                    _aggregateState);
                                line = line.Replace(match.Value, replacementString);
                            }
                            var lineBytes = _httpResponse.ContentEncoding.GetBytes(line);

                            _outputStream.Write(lineBytes, 0, lineBytes.Length);
                        }
                    }
                }
            }
            finally
            {
                _inputStream.Seek(writePosition, SeekOrigin.Begin);
            }
        }
    }
}
