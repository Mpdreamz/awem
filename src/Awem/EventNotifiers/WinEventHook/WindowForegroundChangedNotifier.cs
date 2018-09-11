using System;

namespace Awem
{
	// see https://docs.microsoft.com/en-gb/windows/desktop/api/winuser/nc-winuser-wineventproc
	// and https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwineventhook
	// and https://docs.microsoft.com/en-gb/windows/desktop/WinAuto/event-constants

	public class WindowForegroundChangedNotifier : WindowEventHookNotifierBase
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
