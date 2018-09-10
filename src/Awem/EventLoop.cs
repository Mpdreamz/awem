using System;
using System.Runtime.InteropServices;

namespace Awem
{
	public static class EventLoop
	{
		public static void Run()
		{
			while (true)
			{
				if (!PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE)) continue;
				if (msg.Message == WM_QUIT) break;

				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
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

		[DllImport("user32.dll")]
		private static extern bool PeekMessage(out Msg lpMsg, IntPtr hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
		[DllImport("user32.dll")]
		private static extern bool TranslateMessage(ref Msg lpMsg);
		[DllImport("user32.dll")]
		private static extern IntPtr DispatchMessage(ref Msg lpMsg);
	}
}