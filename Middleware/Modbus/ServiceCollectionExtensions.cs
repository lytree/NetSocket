using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Middleware.HttpProxy;
using Net.Middleware.Modbus;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// ServiceCollection扩展
	/// </summary>
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>
		/// 添加http代理
		/// </summary>
		/// <param name="services"></param> 
		/// <returns></returns>
		public static IServiceCollection AddModbusTcpSlave(this IServiceCollection services)
		{
			services.TryAddSingleton<ModbusConnectionHandler>();
			services.TryAddSingleton<ModbusSlaveNetwork>(new ModbusTcpSlaveNetwork(new ModbusFactory(), NullLogger<ModbusLogger>.Instance));
			return services;
		}
	}
}
