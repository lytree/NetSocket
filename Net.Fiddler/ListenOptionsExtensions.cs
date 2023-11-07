﻿using Net.Fiddler.Certs;
using Net.Fiddler.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    ///  ListenOptions扩展
    /// </summary>
    public static partial class ListenOptionsExtensions
    {
        /// <summary>
        /// 使用Fiddler的kestrel中间件
        /// </summary>
        /// <param name="listen"></param>
        public static ListenOptions UseFiddler(this ListenOptions listen)
        {
            // 代理协议中间件
            listen.Use<KestrelProxyMiddleware>();

            // tls侦测中间件
            listen.UseTlsDetection(tls =>
            {
                var certService = listen.ApplicationServices.GetRequiredService<CertService>();
                certService.CreateCaCertIfNotExists();
                certService.InstallAndTrustCaCert();
                tls.ServerCertificateSelector = (context, domain) => certService.GetOrCreateServerCert(domain);
            });

            // 隧道代理处理中间件
            listen.Use<KestrelTunnelMiddleware>();
            return listen;
        }
    }
}
