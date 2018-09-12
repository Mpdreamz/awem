using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Win32;

namespace Awem
{
	// see https://docs.microsoft.com/en-gb/windows/desktop/api/winuser/nc-winuser-wineventproc
	// and https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwineventhook
	// and https://docs.microsoft.com/en-gb/windows/desktop/WinAuto/event-constants

	public class WindowForegroundChangedNotifier : WindowEventHookNotifierBase
	{
		private const int EVENT_SYSTEM_FOREGROUND = 3;

		public WindowForegroundChangedNotifier() => this.CreateWinEventHook(EVENT_SYSTEM_FOREGROUND, this.WindowEventCallback);

		private readonly Subject<IntPtr> _changed = new Subject<IntPtr>();
		public IObservable<IntPtr> Changed => _changed.AsObservable();

		private void WindowEventCallback(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			=> this._changed.OnNext(hwnd);
	}
}
