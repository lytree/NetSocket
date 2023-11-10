using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

/// <summary>
/// 服务选项
/// </summary>
public sealed class ServiceOptions
{
	/// <summary>
	/// 启动参数
	/// </summary>
	public IEnumerable<Argument>? Arguments { get; set; }

	/// <summary>
	/// 工作目录
	/// </summary>
	public string? WorkingDirectory { get; set; }

	/// <summary>
	/// 服务描述
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// 获取仅适用于linux的选项
	/// </summary>
	public LinuxServiceOptions Linux { get; } = new LinuxServiceOptions();

	/// <summary>
	/// 获取仅适用于windows的选项
	/// </summary>
	public WindowsServiceOptions Windows { get; } = new WindowsServiceOptions();
}