﻿using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Middleware.HttpProxy
{
	/// <summary>
	/// 代理Feature
	/// </summary>
	public interface IProxyFeature
	{
		/// <summary>
		/// 代理主机
		/// </summary>
		HostString ProxyHost { get; }

		/// <summary>
		/// 代理协议
		/// </summary>
		ProxyProtocol ProxyProtocol { get; }

		/// <summary>
		/// Proxy-Authorization
		/// </summary>
		AuthenticationHeaderValue? ProxyAuthorization { get; }
	}
}
