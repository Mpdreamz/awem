using System;
using WindowsDesktop;

namespace Awem
{
	public class DesktopSwitchNotifier : WindowEventHookNotifierBase
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
}
