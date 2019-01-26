using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WindowsDesktop;

namespace Awem.EventNotifiers.WinEventHook
{
	public class DesktopSwitchNotifier : WindowEventHookNotifierBase
	{
		private const int EVENT_SYSTEM_DESKTOPSWITCH = 0x0020;

		public DesktopSwitchNotifier()
		{
			this.CreateWinEventHook(EVENT_SYSTEM_DESKTOPSWITCH, this.WindowEventCallback);
			VirtualDesktop.CurrentChanged += this.OnChanged;
		}

		private readonly Subject<IntPtr> _changed = new Subject<IntPtr>();
		public IObservable<IntPtr> Changed => _changed.AsObservable();
		private void OnChanged(object sender, EventArgs e) => this._changed.OnNext(new IntPtr(1));

		private void WindowEventCallback(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			=> this._changed.OnNext(hwnd);

		public override void Dispose()
		{
			VirtualDesktop.CurrentChanged -= this.OnChanged;
			base.Dispose();
		}
	}
}
