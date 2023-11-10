using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

/// <summary>
/// windows服务操作类型
/// </summary>
public enum WindowsServiceActionType
{
	/// <summary>
	/// 表示不执行任何操作
	/// </summary>
	None = 0,

	/// <summary>
	/// 表示重新启动服务
	/// </summary>
	Restart = 1,

	/// <summary>
	/// 表示重新启动计算机
	/// </summary>
	Reboot = 2,
}