﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Forwarder;

namespace Middleware.HttpProxy
{
	/// <summary>
	/// 普通http代理(非隧道代理)中间件
	/// </summary>
	public class HttpProxyMiddleware
	{
		private readonly RequestDelegate next;
		private readonly IHttpForwarder httpForwarder;
		private readonly ILogger<HttpProxyMiddleware> logger;
		private readonly HttpMessageInvoker httpClient = new(CreateSocketsHttpHandler());

		/// <summary>
		/// 普通http代理(非隧道代理)中间件
		/// </summary>
		/// <param name="next"></param>
		/// <param name="httpForwarder"></param>
		/// <param name="logger"></param>
		public HttpProxyMiddleware(
			RequestDelegate next,
			IHttpForwarder httpForwarder,
			ILogger<HttpProxyMiddleware> logger)
		{
			this.next = next;
			this.httpForwarder = httpForwarder;
			this.logger = logger;
		}

		/// <summary>
		/// 处理http代理
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task InvokeAsync(HttpContext context)
		{
			var feature = context.Features.Get<IProxyFeature>();
			if (feature == null)
			{
				await next(context);
			}
			else if (feature.ProxyProtocol == ProxyProtocol.None)
			{
				await context.Response.WriteAsJsonAsync(new { Error = "请使用http代理协议来访问" });
			}
			else
			{
				var scheme = context.Request.Scheme;
				var destinationPrefix = $"{scheme}://{feature.ProxyHost}";
				logger.LogInformation($"http代理到{destinationPrefix}");
				await httpForwarder.SendAsync(context, destinationPrefix, httpClient, ForwarderRequestConfig.Empty, HttpTransformer.Empty);
			}
		}

		private static SocketsHttpHandler CreateSocketsHttpHandler()
		{
			return new SocketsHttpHandler
			{
				Proxy = null,
				UseProxy = false,
				UseCookies = false,
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.None,
			};
		}
	}
}
