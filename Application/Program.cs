using Net.Framework.Middleware.TlsDetection;
using Net.Framework.Middleware.TlsDetection;
using Serilog;

namespace Application
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			//builder.Services.AddAuthorization();




			builder.Host.UseSerilog((hosting, logger) =>
			{
				logger.ReadFrom
					.Configuration(hosting.Configuration)
					.Enrich.FromLogContext().WriteTo.Console(outputTemplate: "{Timestamp:O} [{Level:u3}]{NewLine}{SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
			});
			builder.Services.AddModbusTcpSlave();
			builder.WebHost.UseKestrel((context, kestrel) =>
			{
				var section = context.Configuration.GetSection("Kestrel");
				kestrel.Configure(section)
					//// ��ͨTelnet������,ʹ��telnet�ͻ��˾Ϳ��Խ���
					//.Endpoint("Telnet", endpoint => endpoint.ListenOptions.UseTelnet())

					//// xor(α)���ܴ����Telnet������, telnet�ͻ��˲��ܽ���
					//.Endpoint("XorTelnet", endpoint => endpoint.ListenOptions.UseFlowXor().UseTelnet())

					//// XorTelnet�����������telnet���ӵ��˷�����֮����������xor֮�����XorTelnet������������������TelnetЭ�鴦��
					//.Endpoint("XorTelnetProxy", endpoint => endpoint.ListenOptions.UseFlowXor().UseXorTelnetProxy())

					//// http������������ܴ����������ĳ���
					//.Endpoint("HttpProxy", endpoint => endpoint.ListenOptions.UseHttpProxy())

					//// http��https���˿�˫Э�������
					//.Endpoint("HttpHttps", endpoint => endpoint.ListenOptions.UseTlsDetection())

					//// echo��echo over tlsЭ�������
					//.Endpoint("Echo", endpoint => endpoint.ListenOptions.UseTlsDetection().UseEcho())

					//// redisЭ�������
					//.Endpoint("Redis", endpoint => endpoint.ListenOptions.UseRedis())

					// redisЭ�������
					.Endpoint("Modbus", endpoint => endpoint.ListenOptions.UseModbus());
			});
			var app = builder.Build();

			// Configure the HTTP request pipeline.

			//app.UseHttpsRedirection();

			//app.UseAuthorization();
			app.Run();

		}
	}
}