using System;
using System.Collections.Generic;
using System.Linq;
using Awem.EventNotifiers.WinEventHook;
using Awem.KeyboardHandling;
using Awem.PInvoke.Enums;
using Awem.Reactive.Monitors;
using Awem.Windowing;
using ReactiveUI;

namespace Awem
{
	public class WindowManager : ReactiveObject, IDisposable
	{
		private WindowForegroundChangedNotifier WindowForegroundChangedNotifier { get; }
		private DesktopSwitchNotifier DesktopSwitchNotifier { get; }


		public DesktopManager DesktopManager { get; }
		public LayoutManager LayoutManager { get; }
		public MonitorManager MonitorManager { get; }

		public KeyboardHooks KeyboardHooks { get; }
		public WindowManagerActions WindowManagerActions { get; }

		public ICollection<MonitorScreen> Monitors => this.MonitorManager.Monitors;
		public ICollection<ApplicationWindow> AllApplications => ApplicationWindows.All().ToList();
		public ICollection<ApplicationWindow> CurrentDesktopApplications => ApplicationWindows.VisibleOnCurrentDesktop().ToList();

		public WindowManager(Action toggleLauncherUi, Action exit)
		{
			this.WindowForegroundChangedNotifier = new WindowForegroundChangedNotifier();
			this.DesktopSwitchNotifier = new DesktopSwitchNotifier();

			this.DesktopManager = new DesktopManager();
			this.MonitorManager = new MonitorManager();

			var windowsChanged = this.WhenAnyObservable(
				w => w.WindowForegroundChangedNotifier.Changed,
				w => w.DesktopSwitchNotifier.Changed
			);
			var desktopChange = this.WhenAnyValue(
				vm => vm.DesktopManager.PreviousDesktop,
				vm => vm.DesktopManager.CurrentDesktop
			);
			this.LayoutManager = new LayoutManager(desktopChange);

			this.WindowManagerActions = new WindowManagerActions(this, toggleLauncherUi, exit);

			var leaderKey = VirtualKeys.RightMenu;
			var keyboardParser = new KeyboardCombinationParser(leaderKey, this.WindowManagerActions, null);

			this.KeyboardHooks = new KeyboardHooks(keyboardParser);
		}

		public void Dispose()
		{
			this.WindowForegroundChangedNotifier.Dispose();
			this.DesktopSwitchNotifier.Dispose();
			this.KeyboardHooks.Dispose();
			this.DesktopManager.Dispose();
		}
	}
}
