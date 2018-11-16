using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem.Windowing
{
	public static class MonitorScreens
	{
		private static int TimesOutput = 0;

		public static void EnumerateScreens()
		{
			TimesOutput++;

			Refresh();
			Console.Clear();
			Console.WriteLine($"Refreshed: {TimesOutput}");
			Console.WriteLine();
			foreach (var m in All())
			{
				Console.WriteLine($"{m.DisplayName} {m.Resolution} {m.ScaledResolution} {m.TaskBar?.Bounds} {m.Workspace}");
			}
		}

		[DllImport("user32")]
		private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
		private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

		private static ConcurrentDictionary<IntPtr, MonitorScreen> MonitorCache = new ConcurrentDictionary<IntPtr, MonitorScreen>();

		public static ICollection<MonitorScreen> All() => MonitorCache.Values;

		public static void Refresh()
		{
			TaskBars.Refresh();
			bool Callback(IntPtr monitorHandle, IntPtr hdc, ref Rect scaledResolution, int d)
			{
				var scaled = scaledResolution.Rectangle;
				var monitor = MonitorCache.GetOrAdd(monitorHandle, i => new MonitorScreen(monitorHandle));
				monitor.ScaledResolution = scaled;
				monitor.Refresh();
				return true;
			}
			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, 0);
		}
	}
}
