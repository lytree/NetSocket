﻿using Net.Framework.Application;
using System.Threading.Tasks;

namespace Middleware.Telnet.Middleware
{
	sealed class EchoMiddleware : IApplicationMiddleware<TelnetContext>
	{
		public async Task InvokeAsync(ApplicationDelegate<TelnetContext> next, TelnetContext context)
		{
			await context.Response.WriteLineAsync($"Did you say '{context.Request}'?");
		}
	}
}
