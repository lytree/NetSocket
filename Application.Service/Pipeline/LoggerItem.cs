﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public partial class LoggerItem
{
	/// <summary>
	/// 日志级别
	/// </summary>
	public LogLevel LogLevel => (LogLevel)this.Level;

	/// <summary>
	/// 是否匹配过滤器
	/// </summary>
	/// <param name="filter">过滤器</param>
	/// <returns></returns>
	public bool IsMatch(ReadOnlySpan<char> filter)
	{
		return filter.IsEmpty ||
			this.LogLevel.ToString().AsSpan().Equals(filter, StringComparison.InvariantCulture) ||
			this.LoggerName.AsSpan().Contains(filter, StringComparison.InvariantCulture) ||
			this.Message.AsSpan().Contains(filter, StringComparison.InvariantCulture);
	}

	/// <summary>
	/// 写入指定的TextWriter
	/// </summary>
	/// <param name="writer"></param>
	public void WriteTo(TextWriter writer)
	{
		writer.WriteLine($"{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ss.fffK} [{this.LogLevel}]");
		writer.WriteLine(this.LoggerName);
		writer.WriteLine(this.Message);
		writer.WriteLine();
	}
}
