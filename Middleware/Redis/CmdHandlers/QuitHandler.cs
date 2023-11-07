﻿using Middleware.Redis;
using System.Threading.Tasks;

namespace Middleware.Redis.CmdHandlers
{
	/// <summary>
	/// 退出
	/// </summary>
	sealed class QuitHandler : IRedisCmdHanler
	{
		public RedisCmd Cmd => RedisCmd.Quit;

		/// <summary>
		/// 处理请求
		/// </summary>
		/// <param name="context"></param> 
		/// <returns></returns>
		public ValueTask HandleAsync(RedisContext context)
		{
			context.Client.Close();
			return ValueTask.CompletedTask;
		}
	}
}
