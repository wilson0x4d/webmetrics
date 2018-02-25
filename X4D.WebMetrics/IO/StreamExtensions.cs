using System.IO;

namespace X4D.WebMetrics.IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Copies one stream to another.
        /// <para>
        /// Copies the input stream from current position to output stream
        /// current position.
        /// </para>
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <returns>Count of bytes written</returns>
        public static long CopyTo(Stream inputStream, Stream outputStream)
        {
            // perform a generic chunked write until no data:
            var buf = new byte[short.MaxValue];
            var totalCount = 0L;
            var count = inputStream.Read(buf, 0, buf.Length);
            while (count > 0)
            {
                outputStream.Write(buf, 0, count);
                totalCount += count;
                count = inputStream.Read(buf, 0, buf.Length);
            }
            return totalCount;
        }
    }
}
