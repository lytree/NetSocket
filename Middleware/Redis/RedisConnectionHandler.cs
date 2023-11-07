using Net.Framework.Application;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Middleware.Redis.Middlewares;
using System;
using System.Threading.Tasks;

namespace Middleware.Redis
{
	/// <summary>
	/// 表示Redis连接处理者
	/// </summary>
	public sealed class RedisConnectionHandler : ConnectionHandler
	{
		private readonly ILogger<RedisConnectionHandler> logger;
		private readonly ApplicationDelegate<RedisContext> application;

		/// <summary>
		/// Redis连接处理者
		/// </summary> 
		/// <param name="appServices"></param> 
		/// <param name="logger"></param>
		public RedisConnectionHandler(
			IServiceProvider appServices,
			ILogger<RedisConnectionHandler> logger)
		{
			this.logger = logger;
			application = new ApplicationBuilder<RedisContext>(appServices)
				.Use<AuthMiddleware>()
				.Use<CmdMiddleware>()
				.Use<FallbackMiddlware>()
				.Build();
		}

		/// <summary>
		/// 处理Redis连接
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async override Task OnConnectedAsync(ConnectionContext context)
		{
			try
			{
				await HandleRequestsAsync(context);
			}
			catch (Exception ex)
			{
				logger.LogDebug(ex.Message);
			}
			finally
			{
				await context.DisposeAsync();
			}
		}

		/// <summary>
		/// 处理redis请求
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleRequestsAsync(ConnectionContext context)
		{
			var input = context.Transport.Input;
			var client = new RedisClient(context);
			var response = new RedisResponse(context.Transport.Output);

			while (context.ConnectionClosed.IsCancellationRequested == false)
			{
				var result = await input.ReadAsync();
				if (result.IsCanceled)
				{
					break;
				}

				var requests = RedisRequest.Parse(result.Buffer, out var consumed);
				if (requests.Count > 0)
				{
					foreach (var request in requests)
					{
						var redisContext = new RedisContext(client, request, response, context.Features);
						await application.Invoke(redisContext);
					}
					input.AdvanceTo(consumed);
				}
				else
				{
					input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
				}

				if (result.IsCompleted)
				{
					break;
				}
			}
		}

	}
}
