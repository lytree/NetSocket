﻿using Microsoft.AspNetCore.Http;

namespace Net.Fiddler.Middlewares
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
    }
}
