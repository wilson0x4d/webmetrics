using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using X4D.WebMetrics.IO;

namespace X4D.WebMetrics.Test
{
	/// <summary>
	/// A few HTTP Module tests that confirm critical behavior.
	/// </summary>
	/// <remarks>
	/// These can be considered "brittle" tests in that they rely on
	/// reflection to reach into the HTTP Module and ASP.NET Components to
	/// perform test execution, as an alternative to running Web-tier tests
	/// against a CI server for the same effect.
	/// <para>
	/// It's possible that MOQ or a similar framework could be used, which
	/// may yield more familiar code for some developers, but it would still
	/// arrive at a test which contains brittle code (code which is dependent
	/// on the internal/private functions of ASP.NET Components, either
	/// directly or through explicit mocking.)
	/// </para>
	/// <para>
	/// The choice to use reflection was made recognizing that there are a
	/// limited number of touch points, and also, that if these touch points
	/// were to change in a way that the test is affected, it is reasonable
	/// that the module implementation should be reviewed (and the test
	/// corrected if/as necessary.)
	/// </para>
	/// </remarks>
	[TestClass]
	public class WebMetricsHttpModuleTests
	{
		/// <summary>
		/// Verifies that html content is rewritten as expected.
		/// </summary>
		[TestMethod]
		public void WebMetricsHttpModule_WillRewriteBodyEndTag()
		{
			// arrange
			var inputBuffer = Encoding.UTF8.GetBytes(
				$"const foo = \"</body>\";</body>{Environment.NewLine}</html>");
			var httpApplication = new HttpApplication();
			var httpContext = TestHttpContextFactory.Create();
			var httpModule = new WebMetricsHttpModule();

			httpModule.Init(httpApplication);

			var staticBindingFlags =
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Static;

			var beginRequestKey = typeof(HttpApplication)
				.GetField("EventBeginRequest", staticBindingFlags)
				.GetValue(httpApplication);

			var preRequestHandlerExecuteKey = typeof(HttpApplication)
				.GetField("EventPreRequestHandlerExecute", staticBindingFlags)
				.GetValue(httpApplication);

			var postRequestHandlerExecuteKey = typeof(HttpApplication)
				.GetField("EventPostRequestHandlerExecute", staticBindingFlags)
				.GetValue(httpApplication);

			var instanceBindingFlags =
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance;

			var httpApplicationEvents = typeof(HttpApplication)
				.GetProperty("Events", instanceBindingFlags)
				.GetMethod
				.Invoke(httpApplication, null)
				as EventHandlerList;

			var initResponseWriter = typeof(HttpResponse)
				.GetMethod("InitResponseWriter", instanceBindingFlags);

			var typeOfHttpResponseStreamFilterSink = typeof(HttpApplication)
				.Assembly
				.GetTypes()
				.FirstOrDefault(e => e.FullName.StartsWith("System.Web.HttpResponseStreamFilterSink"));

			var filteringField = typeOfHttpResponseStreamFilterSink
				.GetField("_filtering", instanceBindingFlags);

			initResponseWriter.Invoke(HttpContext.Current.Response, null);
			var httpResponseStreamFilterSink = HttpContext.Current
				.Response
				.Filter;

			// execute
			httpApplicationEvents[beginRequestKey].DynamicInvoke(httpApplication, new EventArgs());
			httpApplicationEvents[preRequestHandlerExecuteKey].DynamicInvoke(httpApplication, new EventArgs());

			// swap in a stream we can observe
			var actualStream = new MemoryStream();
			typeof(HtmlRewriteFilterStream)
				.GetField("_outputStream", instanceBindingFlags)
				.SetValue(HttpContext.Current.Response.Filter, actualStream);

			HttpContext.Current
				.Response
				.Filter
				.Write(inputBuffer, 0, inputBuffer.Length);

			filteringField.SetValue(
				httpResponseStreamFilterSink,
				true);

			HttpContext.Current
				.Response
				.Filter
				.Flush();

			// verify
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

		/// <summary>
		/// Verifies that unsupported content types are not rewritten/modified.
		/// </summary>
		[TestMethod]
		public void WebMetricsHttpModule_WillNotRewriteUnsupportedContentType()
		{
			// arrange
			var inputBuffer = Encoding.UTF8.GetBytes(
				$"const foo = \"</body>\";</body>{Environment.NewLine}</html>");
			var httpApplication = new HttpApplication();
			var httpContext = TestHttpContextFactory.Create();
			var httpModule = new WebMetricsHttpModule();

			httpModule.Init(httpApplication);

			var staticBindingFlags =
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Static;

			var beginRequestKey = typeof(HttpApplication)
				.GetField("EventBeginRequest", staticBindingFlags)
				.GetValue(httpApplication);

			var preRequestHandlerExecuteKey = typeof(HttpApplication)
				.GetField("EventPreRequestHandlerExecute", staticBindingFlags)
				.GetValue(httpApplication);

			var postRequestHandlerExecuteKey = typeof(HttpApplication)
				.GetField("EventPostRequestHandlerExecute", staticBindingFlags)
				.GetValue(httpApplication);

			var instanceBindingFlags =
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance;

			var httpApplicationEvents = typeof(HttpApplication)
				.GetProperty("Events", instanceBindingFlags)
				.GetMethod
				.Invoke(httpApplication, null)
				as EventHandlerList;

			var initResponseWriter = typeof(HttpResponse)
				.GetMethod("InitResponseWriter", instanceBindingFlags);

			var typeOfHttpResponseStreamFilterSink = typeof(HttpApplication)
				.Assembly
				.GetTypes()
				.FirstOrDefault(e => e.FullName.StartsWith("System.Web.HttpResponseStreamFilterSink"));

			var filteringField = typeOfHttpResponseStreamFilterSink
				.GetField("_filtering", instanceBindingFlags);

			initResponseWriter.Invoke(HttpContext.Current.Response, null);
			var httpResponseStreamFilterSink = HttpContext.Current
				.Response
				.Filter;

			// execute
			httpApplicationEvents[beginRequestKey].DynamicInvoke(httpApplication, new EventArgs());

			HttpContext.Current.Response.ContentType = "image/png";

			httpApplicationEvents[preRequestHandlerExecuteKey].DynamicInvoke(httpApplication, new EventArgs());

			// swap in a stream we can observe
			var actualStream = new MemoryStream();
			typeof(HtmlRewriteFilterStream)
				.GetField("_outputStream", instanceBindingFlags)
				.SetValue(HttpContext.Current.Response.Filter, actualStream);

			HttpContext.Current
				.Response
				.Filter
				.Write(inputBuffer, 0, inputBuffer.Length);

			filteringField.SetValue(
				httpResponseStreamFilterSink,
				true);

			HttpContext.Current
				.Response
				.Filter
				.Flush();

			// verify
			var actualBuffer = new byte[actualStream.Length];
			actualStream.Seek(0, SeekOrigin.Begin);
			actualStream.Read(actualBuffer, 0, actualBuffer.Length);
			var actualString = Encoding.UTF8.GetString(actualBuffer);
			Assert.AreEqual(inputBuffer.Length, actualBuffer.Length);
			for (int i = 0; i < actualBuffer.Length; i++)
			{
				if (inputBuffer[i] != actualBuffer[i])
				{
					Assert.Fail($"Data at buffer position {i} did not match, expected:<{inputBuffer[i]}>, actual:<{actualBuffer[i]}>.");
				}
			}
		}

	}
}
