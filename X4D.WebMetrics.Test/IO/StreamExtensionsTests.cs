using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using X4D.WebMetrics.IO;

namespace X4D.WebMetrics.Test.IO
{
    [TestClass]
    public class StreamExtensionsTests
    {
        /// <summary>
        /// Because there is an optimization for <see cref="MemoryStream"/>,
        /// we test this scenario to ensure there is no error in the
        /// optimized code.
        /// </summary>
        [TestMethod]
        public void StreamExtensions_CopyTo_WillCopyFromCurrentPositions()
        {
            var expectedBuffer = new byte[ushort.MaxValue];
            for (int i = 0; i < expectedBuffer.Length; i++)
            {
                expectedBuffer[i] = (byte)(i % 0x7E); // arbitrary check digit
            }
            using (var inputStream = new MemoryStream(expectedBuffer))
            {
                using (var outputStream = new MemoryStream())
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.WriteByte(0); // this verifies that CopyTo correctly copies from current position as described

                    var count = StreamExtensions.CopyTo(inputStream, outputStream);

                    Assert.AreEqual(expectedBuffer.Length, count);
                    var actualbuffer = outputStream.GetBuffer();
                    for (int i = 0; i < expectedBuffer.Length; i++)
                    {
                        if (expectedBuffer[i] != actualbuffer[i + 1])
                        {
                            Assert.Fail($"Data at buffer position {i} did not match, expected:<{expectedBuffer[i]}>, actual:<{actualbuffer[i]}>.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// We necessarily expect that streams other than <see
        /// cref="MemoryStream"/> can be copied, since other tests which use
        /// CopyTo tend to rely on MemoryStream we don't want some hidden bug
        /// cropping up with something other than MemoryStream is used.
        /// </summary>
        [TestMethod]
        public void StreamExtensions_CopyTo_CanCopyBetweenNonMemoryStreams()
        {
            var expectedBuffer = new byte[ushort.MaxValue];
            for (int i = 0; i < expectedBuffer.Length; i++)
            {
                expectedBuffer[i] = (byte)(i % 0x7E); // arbitrary check digit
            }
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var inputTempPath = $"{tempPath}.expected";
            using (var inputStream = new FileStream(inputTempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            {
                inputStream.Write(expectedBuffer, 0, expectedBuffer.Length);
                var outputTempPath = $"{tempPath}.actual";
                using (var outputStream = new FileStream(outputTempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.WriteByte(0); // this verifies that CopyTo correctly copies from current position as described

                    var count = StreamExtensions.CopyTo(inputStream, outputStream);

                    Assert.AreEqual(expectedBuffer.Length, count);
                    var actualbuffer = new byte[count + 1];
                    outputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.Read(actualbuffer, 0, (int)count + 1);
                    for (int i = 0; i < expectedBuffer.Length; i++)
                    {
                        if (expectedBuffer[i] != actualbuffer[i + 1])
                        {
                            Assert.Fail($"Data at buffer position {i} did not match, expected:<{expectedBuffer[i]}>, actual:<{actualbuffer[i]}>.");
                        }
                    }
                }
                File.Delete(outputTempPath);
            }
            File.Delete(inputTempPath);
        }
    }
}
