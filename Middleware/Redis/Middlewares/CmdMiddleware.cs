using Net.Framework.Application;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Redis.Middlewares
{
	/// <summary>
	/// 命令处理中间件
	/// </summary>
	sealed class CmdMiddleware : IRedisMiddleware
	{
		private readonly Dictionary<RedisCmd, IRedisCmdHanler> cmdHandlers;

		public CmdMiddleware(IEnumerable<IRedisCmdHanler> cmdHanlers)
		{
			cmdHandlers = cmdHanlers.ToDictionary(item => item.Cmd, item => item);
		}

		public async Task InvokeAsync(ApplicationDelegate<RedisContext> next, RedisContext context)
		{
			if (cmdHandlers.TryGetValue(context.Reqeust.Cmd, out var hanler))
			{
				await hanler.HandleAsync(context);
			}
			else
			{
				await next(context);
			}
		}
	}
}
