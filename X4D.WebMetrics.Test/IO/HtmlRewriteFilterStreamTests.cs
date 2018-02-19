using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using X4D.WebMetrics.IO;

namespace X4D.WebMetrics.Test.IO
{
	[TestClass]
	public class HtmlRewriteFilterStreamTests
	{
		/// <summary>
		/// There may be some edge cases where `body` end-tag appears in
		/// quoted strings/etc and is not an actual DOM element tag. This
		/// test attempts to verify that such a string is not inadvertently rewritten.
		/// <para>
		/// The current implementation does not guard against unbalanced
		/// content, ie. <![CDATA[`</body>\r\n</body></html>`]]> , in such a
		/// scenario we will observe two rewrites.
		/// </para>
		/// </summary>
		[TestMethod]
		public void HtmlRewriteFilterStream_WriteBufferedInput_WillNotRewriteNonDOMBodyEndTag()
		{
			TestHttpContextFactory.Create();

			var expectedBuffer = Encoding.UTF8.GetBytes($"const foo = \"</body>\";</body>{Environment.NewLine}</html>");
			using (var actualStream = new MemoryStream())
			using (var responseFilterStream = new HtmlRewriteFilterStream(
				new System.Web.HttpResponse(new StreamWriter(actualStream)),
				new WebMetricsRequestState(@"http://localhost"),
				new WebMetricsAggregateState()))
			{
				responseFilterStream.Write(expectedBuffer, 0, expectedBuffer.Length);
				responseFilterStream.Flush();
				Assert.AreNotEqual(0, actualStream.Length);
				var actualBuffer = new byte[actualStream.Length];
				actualStream.Seek(0, SeekOrigin.Begin);
				actualStream.Read(actualBuffer, 0, actualBuffer.Length);
				var actualString = Encoding.UTF8.GetString(actualBuffer);
				if (!actualString.Contains("x4d-webmetrics"))
				{
					Assert.Fail("The resulting string did not contain expected content.");
				}
				if (!actualString.Contains("\"</body>\""))
				{
					Assert.Fail("The resulting string did not contain expected quoted `</body>` text.");
				}
			}
		}

		/// <summary>
		/// When <see cref="Stream.Flush"/> is called we expect <see
		/// cref="HtmlRewriteFilterStream"/> to rewrite `body` end-tag with
		/// our web metrics.
		/// </summary>
		[TestMethod]
		public void HtmlRewriteFilterStream_WriteBufferedInput_WillRewriteBodyEndTag()
		{
			TestHttpContextFactory.Create();

			var expectedBuffer = Encoding.UTF8.GetBytes($"</body>{Environment.NewLine}</html>");
			using (var actualStream = new MemoryStream())
			using (var responseFilterStream = new HtmlRewriteFilterStream(
				new System.Web.HttpResponse(new StreamWriter(actualStream)),
				new WebMetricsRequestState(@"http://localhost"),
				new WebMetricsAggregateState()))
			{
				responseFilterStream.Write(expectedBuffer, 0, expectedBuffer.Length);
				responseFilterStream.Flush();
				Assert.AreNotEqual(0, actualStream.Length);
				var actualBuffer = new byte[actualStream.Length];
				actualStream.Seek(0, SeekOrigin.Begin);
				actualStream.Read(actualBuffer, 0, actualBuffer.Length);
				var actualString = Encoding.UTF8.GetString(actualBuffer);
				if (!actualString.Contains("x4d-webmetrics"))
				{
					Assert.Fail("The resulting string did not contain expected content.");
				}
				if (!actualString.Contains("</html>"))
				{
					Assert.Fail("The resulting string did not contain expected `</html>` end-tag.");
				}
			}
		}
	}
}
