using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;
using Awem.PInvoke.Structs;

namespace Awem.Windowing
{
	internal static class MonitorWin32
	{
		[DllImport("user32")]
		private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
		private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

		public static IEnumerable<MonitorHandle> EnumerateDisplayMonitors()
		{
			var intPtrs = new List<MonitorHandle>();
			bool Callback(IntPtr monitorHandle, IntPtr hdc, ref Rect scaledResolution, int d)
			{
				intPtrs.Add(new MonitorHandle(monitorHandle, scaledResolution.Rectangle));
				return true;
			}
			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, 0);
			return intPtrs;
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DevMode devMode);

		public const int ENUM_CURRENT_SETTINGS = -1;

		//TODO Create method to rotate the monitor orientation
		private const int DMDO_DEFAULT = 0;
		private const int DMDO_90 = 1;
		private const int DMDO_180 = 2;
		private const int DMDO_270 = 3;

	}
}
