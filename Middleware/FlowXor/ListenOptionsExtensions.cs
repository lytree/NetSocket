﻿using Microsoft.AspNetCore.Server.Kestrel.Core;
using Middleware.FlowXor;
using Microsoft.AspNetCore.Connections;

namespace Microsoft.AspNetCore.Hosting
{
	/// <summary>
	/// ListenOptions扩展
	/// </summary>
	public static partial class ListenOptionsExtensions
    {
        /// <summary>
        /// 使用Xor处理流量
        /// </summary>
        /// <param name="listen"></param>
        /// <returns></returns>
        public static ListenOptions UseFlowXor(this ListenOptions listen)
        {
            listen.Use<XorMiddleware>();
            return listen;
        }
    }
}
