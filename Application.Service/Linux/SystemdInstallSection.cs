﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

/// <summary>
/// Install章节
/// </summary>
public sealed class SystemdInstallSection : SystemdSection
{
	/// <summary>
	/// Install章节
	/// </summary>
	public SystemdInstallSection()
		: base("[Install]")
	{
	}

	/// <summary>
	/// Install章节
	/// </summary>
	/// <param name="section"></param>
	public SystemdInstallSection(SystemdInstallSection section)
		: this()
	{
		foreach (var item in section)
		{
			this[item.Key] = item.Value;
		}
	}

	/// <summary>
	/// 将单元链接到提供特定功能的.target单元
	/// </summary>
	public string? WantedBy
	{
		get => Get(nameof(WantedBy));
		set => Set(nameof(WantedBy), value);
	}

	/// <summary>
	/// 在启动时强制依赖于单个或多个其他单元
	/// </summary>
	public string? RequiredBy
	{
		get => Get(nameof(RequiredBy));
		set => Set(nameof(RequiredBy), value);
	}
}