using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using WindowsDesktop;

namespace Awem
{
	// see https://docs.microsoft.com/en-gb/windows/desktop/api/winuser/nc-winuser-wineventproc
	// and https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwineventhook
	// and https://docs.microsoft.com/en-gb/windows/desktop/WinAuto/event-constants
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	public abstract class WindowEventHookNotifier : IDisposable
	{
		private IntPtr _windowEventHook;

		protected delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetWinEventHook(
			int eventMin, int eventMax, IntPtr callbackProcess, WinEventProc callback, int idProcess, int idThread, int flags
		);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern int UnhookWinEvent(IntPtr hWinEventHook);

		private const int WINEVENT_OUTOFCONTEXT = 0;
		private const int WINEVENT_SKIPOWNPROCESS = 2;

		protected void CreateWinEventHook(int listenEvent, WinEventProc callback)
		{
			const int flags = WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS;
			this._windowEventHook = SetWinEventHook(listenEvent, listenEvent, IntPtr.Zero, callback, 0, 0, flags);

			if (this._windowEventHook == IntPtr.Zero)
				throw new Exception(Marshal.GetLastWin32Error().ToString());
		}
		public void Dispose()
		{
			if (this._windowEventHook == IntPtr.Zero) return;
			UnhookWinEvent(this._windowEventHook);
		}

	}

	public class DesktopSwitchNotifier : WindowEventHookNotifier
	{
		private readonly Action<IntPtr> _changedHandler;

		public DesktopSwitchNotifier(Action<IntPtr> changedHandler)
		{
			_changedHandler = changedHandler ?? (i => { });
			this.CreateWinEventHook(EVENT_SYSTEM_DESKTOPSWITCH, this.WindowEventCallback);
			VirtualDesktop.CurrentChanged += (sender, args) => _changedHandler(new IntPtr(1));
		}

		private void WindowEventCallback(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			=> _changedHandler(hwnd);

		private const int EVENT_SYSTEM_DESKTOPSWITCH = 0x0020;

	}

	public class WindowForegroundChangedNotifier : WindowEventHookNotifier
	{
		private readonly Action<IntPtr> _changedHandler;

		public WindowForegroundChangedNotifier(Action<IntPtr> changedHandler)
		{
			_changedHandler = changedHandler ?? (i => { });
			this.CreateWinEventHook(EVENT_SYSTEM_FOREGROUND, this.WindowEventCallback);
		}

		private void WindowEventCallback(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			=> _changedHandler(hwnd);

		private const int EVENT_SYSTEM_FOREGROUND = 3;

	}
}
