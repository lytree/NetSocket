﻿using Net.Framework.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Middleware.Redis.Middlewares
{
	/// <summary>
	/// 认证中间件
	/// </summary>
	sealed class AuthMiddleware : IRedisMiddleware
	{
		private readonly ILogger<AuthMiddleware> logger;
		private readonly IOptionsMonitor<RedisOptions> options;

		public AuthMiddleware(
			ILogger<AuthMiddleware> logger,
			IOptionsMonitor<RedisOptions> options)
		{
			this.logger = logger;
			this.options = options;
		}

		public async Task InvokeAsync(ApplicationDelegate<RedisContext> next, RedisContext context)
		{
			if (context.Client.IsAuthed == false)
			{
				await context.Response.WriteAsync(ResponseContent.Err);
			}
			else if (context.Client.IsAuthed == true)
			{
				await next(context);
			}
			else if (context.Reqeust.Cmd != RedisCmd.Auth)
			{
				if (string.IsNullOrEmpty(options.CurrentValue.Auth))
				{
					context.Client.IsAuthed = true;
					await next(context);
				}
				else
				{
					logger.LogWarning("需要客户端Auth");
					await context.Response.WriteAsync(ResponseContent.Err);
				}
			}
			else
			{
				await next(context);
			}
		}
	}
}
