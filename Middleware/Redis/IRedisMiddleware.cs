using Net.Framework.Application;

namespace Middleware.Redis
{
	/// <summary>
	/// redis中间件
	/// </summary>
	interface IRedisMiddleware : IApplicationMiddleware<RedisContext>
	{
	}
}
