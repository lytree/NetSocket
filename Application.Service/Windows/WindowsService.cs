using Microsoft.Extensions.Hosting.Systemd;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;


namespace Application.Services;
[SupportedOSPlatform("windows5.1.2600")]
sealed class WindowsService : Service
{

	private const string WorkingDirArgName = "WD";


	public WindowsService(string name)
	   : base(name)
	{
	}
	/// <summary>
	/// 应用工作目录
	/// </summary>
	/// <param name="args">启动参数</param>
	/// <returns></returns>
	public static bool UseWorkingDirectory(string[] args)
	{
		if (Argument.TryGetValue(args, WorkingDirArgName, out var workingDir))
		{
			Environment.CurrentDirectory = workingDir;
			return true;
		}
		return false;
	}

	public override void CreateStart(string filePath, ServiceOptions options)
	{
		var hSCManager = Windows.Win32.PInvoke.OpenSCManager("", "", 0xF003F);
		if (hSCManager == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
		filePath = Path.GetFullPath(filePath);
		var hService = Windows.Win32.PInvoke.OpenService(hSCManager, this.Name, 0xF01FF);
		if (hService == IntPtr.Zero)
		{
			var newService = CreateService(hSCManager, filePath, options);
			StartService(newService);
		}
		else
		{
			var oldFilePath = QueryServiceFilePath(hService);
			if (oldFilePath.Length > 0 && oldFilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase) == false)
			{
				throw new InvalidOperationException("系统已存在同名但不同路径的服务");
			}
			StartService(hService);

		}
	}

	/// <summary>
	/// 停止并删除服务
	/// </summary>  
	public override void StopDelete()
	{
		var hSCManager = Windows.Win32.PInvoke.OpenSCManager("", "", 0xF003F);
		if (hSCManager == IntPtr.Zero)
		{
			throw new Win32Exception();
		}

		var hService = Windows.Win32.PInvoke.OpenService(hSCManager, this.Name, 0xF01FF);
		if (hService == IntPtr.Zero)
		{
			throw new Win32Exception();
		}

		StopService(hService, TimeSpan.FromSeconds(30d));

		if (Windows.Win32.PInvoke.DeleteService(hService) == false)
		{
			throw new Win32Exception();
		}
	}

	/// <summary>
	/// 尝试获取服务的进程id
	/// </summary>
	/// <param name="processId"></param>
	/// <returns></returns>
	protected override bool TryGetProcessId(out int processId)
	{
		processId = 0;
		var hSCManager = Windows.Win32.PInvoke.OpenSCManager("", "", 0xF003F);
		if (hSCManager == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
		var serviceHandle = Windows.Win32.PInvoke.OpenService(hSCManager, this.Name, 0xF01FF);
		if (serviceHandle == IntPtr.Zero)
		{
			return false;
		}

		var status = new SERVICE_STATUS_PROCESS();
		if (Windows.Win32.PInvoke.QueryServiceStatusEx(serviceHandle, Windows.Win32.System.Services.SC_STATUS_TYPE.SC_STATUS_PROCESS_INFO,null, out _) == false)
		{
			return false;
		}

		processId = (int)status.dwProcessId;
		return processId > 0;
	}




	private unsafe static void StartService(Windows.Win32.Security.SC_HANDLE serviceHandle)
	{
		if (Windows.Win32.PInvoke.QueryServiceStatus(serviceHandle, out var status) == false)
		{
			throw new Win32Exception();
		}

		if (status.dwCurrentState == Windows.Win32.System.Services.SERVICE_STATUS_CURRENT_STATE.SERVICE_RUNNING ||
			status.dwCurrentState == Windows.Win32.System.Services.SERVICE_STATUS_CURRENT_STATE.SERVICE_START_PENDING)
		{
			return;
		}

		if (Windows.Win32.PInvoke.StartService(serviceHandle, 0, null) == false)
		{
			throw new Win32Exception();
		}
	}
	private unsafe Windows.Win32.Security.SC_HANDLE CreateService(Windows.Win32.Security.SC_HANDLE hSCManager, string filePath, ServiceOptions options)
	{

		var arguments = options.Arguments ?? Enumerable.Empty<Argument>();
		arguments = string.IsNullOrEmpty(options.WorkingDirectory)
			? arguments.Append(new Argument(WorkingDirArgName, Path.GetDirectoryName(filePath)))
			: arguments.Append(new Argument(WorkingDirArgName, Path.GetFullPath(options.WorkingDirectory)));
		var serviceHandle = Windows.Win32.PInvoke.CreateService(
				hSCManager,
				this.Name,
				options.Windows.DisplayName,
				0xF01FF,
				Windows.Win32.System.Services.ENUM_SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
				Windows.Win32.System.Services.SERVICE_START_TYPE.SERVICE_AUTO_START,
				Windows.Win32.System.Services.SERVICE_ERROR.SERVICE_ERROR_NORMAL,
				$@"""{filePath}"" {string.Join(' ', arguments)}",
				lpLoadOrderGroup: null,
				lpdwTagId: (uint*)0,
				lpDependencies: options.Windows.Dependencies,
				lpServiceStartName: options.Windows.ServiceStartName,
				lpPassword: options.Windows.Password
				);

		if (serviceHandle == IntPtr.Zero)
		{
			throw new Win32Exception();
		}


		if (string.IsNullOrEmpty(options.Description) == false)
		{
			var desc = new { lpDescription = options.Description };
			var pDesc = Marshal.AllocHGlobal(Marshal.SizeOf(desc));
			Marshal.StructureToPtr(desc, pDesc, false);
			Windows.Win32.PInvoke.ChangeServiceConfig2W(serviceHandle, Windows.Win32.System.Services.SERVICE_CONFIG.SERVICE_CONFIG_DESCRIPTION, pDesc.ToPointer());
			Marshal.FreeHGlobal(pDesc);
		}
		var action = new SC_ACTION
		{
			Type = (SC_ACTION_TYPE)options.Windows.FailureActionType,
		};
		var failureAction = new SERVICE_FAILURE_ACTIONS
		{
			cActions = 1,
			lpsaActions = &action,
			dwResetPeriod = (int)TimeSpan.FromDays(1d).TotalSeconds
		};

		if (Windows.Win32.PInvoke.ChangeServiceConfig2W(serviceHandle, Windows.Win32.System.Services.SERVICE_CONFIG.SERVICE_CONFIG_DESCRIPTION, &failureAction) == false)
		{
			throw new Win32Exception();
		}



		return serviceHandle;
	}
	private unsafe static ReadOnlySpan<char> QueryServiceFilePath(Windows.Win32.Security.SC_HANDLE hSCManager)
	{
		const int ERROR_INSUFFICIENT_BUFFER = 122;
		if (Windows.Win32.PInvoke.QueryServiceConfig(hSCManager, null, 0, out var bytesNeeded) == false)
		{
			if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
			{
				throw new Win32Exception();
			}
		}

		var buffer = Marshal.AllocHGlobal(((int)bytesNeeded));
		try
		{
			if (Windows.Win32.PInvoke.QueryServiceConfig(hSCManager, null, bytesNeeded, out _) == false)
			{
				throw new Win32Exception();
			}

			var serviceConfig = Marshal.PtrToStructure<Windows.Win32.System.Services.QUERY_SERVICE_CONFIGW>(buffer);
			var binaryPathName = serviceConfig.lpBinaryPathName.AsSpan();
			if (binaryPathName.IsEmpty)
			{
				return ReadOnlySpan<char>.Empty;
			}

			if (binaryPathName[0] == '"')
			{
				binaryPathName = binaryPathName[1..];
				var index = binaryPathName.IndexOf('"');
				return index < 0 ? binaryPathName : binaryPathName[..index];
			}
			else
			{
				var index = binaryPathName.IndexOf(' ');
				return index < 0 ? binaryPathName : binaryPathName[..index];
			}
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}
	}


	private static unsafe void StopService(Windows.Win32.Security.SC_HANDLE hService, TimeSpan maxWaitTime)
	{
		if (Windows.Win32.PInvoke.QueryServiceStatus(hService, out var status) != true)
		{
			throw new Win32Exception();
		}
		if (status.dwCurrentState == Windows.Win32.System.Services.SERVICE_STATUS_CURRENT_STATE.SERVICE_STOPPED)
		{
			return;
		}

		if (status.dwCurrentState != Windows.Win32.System.Services.SERVICE_STATUS_CURRENT_STATE.SERVICE_STOP_PENDING)
		{
			if (Windows.Win32.PInvoke.ChangeServiceConfig2W(hService, Windows.Win32.System.Services.SERVICE_CONFIG.SERVICE_CONFIG_FAILURE_ACTIONS) == false)
			{
				throw new Win32Exception();
			}
			if (Windows.Win32.PInvoke.ControlService(hService, 0x00000001, out status) == false)
			{
				throw new Win32Exception();
			}

		}


		var stopwatch = Stopwatch.StartNew();
		var statusQueryDelay = TimeSpan.FromMilliseconds(100d);


		while (stopwatch.Elapsed < maxWaitTime)
		{
			if (status.dwCurrentState == Windows.Win32.System.Services.SERVICE_STATUS_CURRENT_STATE.SERVICE_STOPPED)
			{
				return;
			}

			Thread.Sleep(statusQueryDelay);
			if (Windows.Win32.PInvoke.QueryServiceStatus(hService, out status) == false)
			{
				throw new Win32Exception();
			}
		}

		throw new TimeoutException($"等待服务停止超过了{maxWaitTime.TotalSeconds}秒");

	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ServiceDescription
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string? lpDescription;
	}
	public enum SC_ACTION_TYPE : uint
	{
		SC_ACTION_NONE,
		SC_ACTION_RESTART,
		SC_ACTION_REBOOT,
		SC_ACTION_RUN_COMMAND,
		SC_ACTION_OWN_RESTART
	}
	[Flags]
	public enum ServiceType : uint
	{
		SERVICE_KERNEL_DRIVER = 0x00000001,
		SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
		SERVICE_DRIVER = SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER,
		SERVICE_ADAPTER = 0x00000004,
		SERVICE_RECOGNIZER_DRIVER = 0x00000008,
		SERVICE_WIN32_OWN_PROCESS = 0x00000010,
		SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
		SERVICE_WIN32 = SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS,
		SERVICE_INTERACTIVE_PROCESS = 0x00000100,
		SERVICE_NO_CHANGE = 0xFFFFFFFF,
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct SC_ACTION
	{
		public SC_ACTION_TYPE Type;
		public uint Delay;
	}


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct SERVICE_FAILURE_ACTIONS
	{
		public int dwResetPeriod;
		public unsafe char* lpRebootMsg;
		public unsafe char* lpCommand;
		public int cActions;
		public unsafe SC_ACTION* lpsaActions;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SERVICE_STATUS_PROCESS
	{
		public ServiceType dwServiceType;
		public ServiceState dwCurrentState;
		public uint dwControlsAccepted;
		public uint dwWin32ExitCode;
		public uint dwServiceSpecificExitCode;
		public uint dwCheckPoint;
		public uint dwWaitHint;
		public uint dwProcessId;
		public uint dwServiceFlags;
	}


}
