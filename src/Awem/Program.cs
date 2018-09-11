using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Awem
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (sender, eventArgs) => MonitorScreens.EnumerateScreens();
			Microsoft.Win32.SystemEvents.UserPreferenceChanged += (sender, eventArgs) => MonitorScreens.EnumerateScreens();

			using (new WindowForegroundChangedNotifier(i => { }))
			using (new DesktopSwitchNotifier(i => Console.WriteLine("Desktop {0}", i)))
			{
				MonitorScreens.EnumerateScreens();
//				var applicationWindows = ApplicationWindows.VisibleOnCurrentDesktop();
//				foreach (var w in applicationWindows)
//				{
//					Console.WriteLine($"{w.ClassName} \t {w.WindowTitle}");
//				}
				EventLoop.Run();
			}

			return 0;
		}
	}

	public enum MonitorOrientation
	{
		Default = 0,
		Rotated90 = 1,
		Rotated180 = 2,
		Rotated270 = 3,
	}

	public class MonitorScreen
	{
		public IntPtr MonitorHandle { get; }
		public MonitorOrientation Orientation { get; }
		public Rectangle ScaledResolution { get; }
		public Rectangle Resolution { get; }
		public string DisplayName { get; }

		public MonitorScreen(IntPtr monitorHandle, MonitorOrientation orientation, Rectangle scaled, Rectangle actual, string displayName)
		{
			this.MonitorHandle = monitorHandle;
			this.Orientation = orientation;
			this.ScaledResolution = scaled;
			this.Resolution = actual;
			this.DisplayName = displayName;
		}
	}

	public static class MonitorScreens
	{
		public static void EnumerateScreens()
		{
			Console.Clear();
			foreach (var m in All())
			{
				Console.WriteLine($"{m.DisplayName} {m.Orientation} {m.Resolution} {m.ScaledResolution}");
			}
		}

		public static List<MonitorScreen> All()
		{
			var monitors = new List<MonitorScreen>();
			bool Callback(IntPtr hMonitor, IntPtr hdc, ref Rect scaledResolution, int d)
			{
				var mi = new MonitorInfo
				{
					Size = Marshal.SizeOf(typeof(MonitorInfo))
				};
				if (!GetMonitorInfo(hMonitor, ref mi)) return true;
				var vDevMode = new DEVMODE
				{
					dmSize = (short) Marshal.SizeOf(typeof(DEVMODE))
				};

				if (!EnumDisplaySettings(mi.DeviceName, ENUM_CURRENT_SETTINGS, ref vDevMode)) return true;
				var orientation = (MonitorOrientation) vDevMode.dmDisplayOrientation;
				var actualResolution = new Rectangle(vDevMode.dmPositionX, vDevMode.dmPositionY, vDevMode.dmPelsWidth, vDevMode.dmPelsHeight);
				var m = new MonitorScreen(hMonitor, orientation, scaledResolution.Rectangle, actualResolution, vDevMode.dmDeviceName);
				monitors.Add(m);
				return true;
			}

			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, 0);
			return monitors;
		}

		[DllImport("user32")]
		private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

		private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MonitorInfo
		{
			public int Size;
			private Rect Monitor;
			private Rect WorkArea;
			private uint Flags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DeviceName;
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
		const int ENUM_CURRENT_SETTINGS = -1;

		public const int DMDO_DEFAULT = 0;
		public const int DMDO_90 = 1;
		public const int DMDO_180 = 2;
		public const int DMDO_270 = 3;

		[StructLayout(LayoutKind.Sequential)]
		public struct DEVMODE
		{
			private const int CCHDEVICENAME = 0x20;
			private const int CCHFORMNAME = 0x20;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}
	}
}
