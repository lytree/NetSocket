﻿using Microsoft.Extensions.Logging;
using System;

namespace Application.Services;

/// <summary>
/// 命名管道日志
/// </summary>
sealed class NamedPipeLogger : ILogger
{
	private readonly string categoryName;
	private readonly NamedPipeClient pipeClient;

	public NamedPipeLogger(string categoryName, NamedPipeClient pipeClient)
	{
		this.categoryName = categoryName;
		this.pipeClient = pipeClient;
	}

	public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel != LogLevel.None && this.pipeClient.CanWrite;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (this.IsEnabled(logLevel))
		{
			var logItem = new LoggerItem()
			{
				LoggerName = this.categoryName,
				Level = (int)logLevel,
				Message = formatter(state, exception)
			};

			this.pipeClient.Write(logItem);
		}
	}

	private class NullScope : IDisposable
	{
		public static NullScope Instance { get; } = new();

		public void Dispose()
		{
		}
	}
}
