using System;
using System.Runtime.InteropServices;

namespace Awem.CommandLine
{
	public static class EventLoop
	{
		public static void Run()
		{
			while (true)
			{
				if (!PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
				{
					if (BreakRequested) break;
					continue;
				}
				var m = msg.Message;
				if (m == WM_QUIT) break;
				if (m == WM_DISPLAYCHANGE)
				{
					Console.WriteLine("Display changed");
				}
				if (m == WM_DEVICECHANGE && (msg.WParam.ToInt32() == DBT_DEVICEARRIVAL
				    || msg.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE
				    || msg.WParam.ToInt32() == DBT_DEVNODES_CHANGED))
				{
					//lParam has no information to check wheter its actually a monitor change
					Console.WriteLine("Device change");
				}

				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
				if (BreakRequested) break;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Msg
		{
			public readonly IntPtr Hwnd;
			public readonly uint Message;
			public readonly IntPtr WParam;
			public readonly IntPtr LParam;
			public readonly uint Time;
			public readonly System.Drawing.Point Point;
		}

		const uint PM_REMOVE = 1;

		const uint WM_QUIT = 0x0012;
		const uint WM_DISPLAYCHANGE = 0x007e;
		const uint WM_DEVICECHANGE = 0x0219;
		const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
		const int DBT_DEVICEREMOVECOMPLETE = 0x8004; //device was removed
		const int DBT_DEVNODES_CHANGED = 0x0007; //device changed

		[DllImport("user32.dll")]
		private static extern bool PeekMessage(out Msg lpMsg, IntPtr hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
		[DllImport("user32.dll")]
		private static extern bool TranslateMessage(ref Msg lpMsg);
		[DllImport("user32.dll")]
		private static extern IntPtr DispatchMessage(ref Msg lpMsg);

		private static bool BreakRequested = false;
		public static void Break() => BreakRequested = true;
	}
}
