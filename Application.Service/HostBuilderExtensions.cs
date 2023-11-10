using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

/// <summary>
/// IHostBuilder扩展
/// </summary>
public static class HostBuilderExtensions
{
	/// <summary>
	/// 为主机使用WindowsService和Systemd的生命周期
	/// 同时选择是否启用日志记录和监听功能
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="enableLogs">是否启用日志功能，设置为true后调用logs子命令时才有日志输出</param>
	/// <returns></returns>
	public static IHostBuilder UseServiceSelf(this IHostBuilder hostBuilder, bool enableLogs = true)
	{
		if (enableLogs == true)
		{
			hostBuilder.ConfigureLogging(builder =>
			{
				var descriptor = ServiceDescriptor.Singleton<ILoggerProvider, NamedPipeLoggerProvider>();
				builder.Services.TryAddEnumerable(descriptor);
			});
		}
		return hostBuilder.UseWindowsService().UseSystemd();
	}
}
