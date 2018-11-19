using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using WindowsDesktop;
using Awem.EventNotifiers;
using Awem.KeyboardHandling;
using Awem.PInvoke.Enums;
using Awem.Windowing;
using ReactiveUI;

namespace Awem
{
	public class WindowManager : ReactiveObject, IDisposable
	{
		private UserPreferenceChangedNotifier UserPreferenceChangedNotifier { get; }
		private DisplaySettingsChangedNotifier DisplaySettingsChangedNotifier { get; }
		private WindowForegroundChangedNotifier WindowForegroundChangedNotifier { get; }
		private DesktopSwitchNotifier DesktopSwitchNotifier { get; }

		public KeyboardHooks KeyboardHooks { get; }
		public DesktopManager DesktopManager { get; }
		private LayoutManager LayoutManager { get; }
		public WindowManagerActions WindowManagerActions { get; }

		public ICollection<MonitorScreen> Monitors => MonitorScreens.All();
		public ICollection<ApplicationWindow> AllApplications => ApplicationWindows.All().ToList();
		public ICollection<ApplicationWindow> CurrentDesktopApplications => ApplicationWindows.VisibleOnCurrentDesktop().ToList();

		public WindowManager()
		{
			this.UserPreferenceChangedNotifier = new UserPreferenceChangedNotifier();
			this.DisplaySettingsChangedNotifier = new DisplaySettingsChangedNotifier();
			this.WindowForegroundChangedNotifier = new WindowForegroundChangedNotifier();
			this.DesktopSwitchNotifier = new DesktopSwitchNotifier();

			this.DesktopManager = new DesktopManager();
			var displayChanged = this.WhenAnyObservable(
				w => w.UserPreferenceChangedNotifier.Changed,
				w => w.DisplaySettingsChangedNotifier.Changed
			);
			var windowsChanged = this.WhenAnyObservable(
				w => w.WindowForegroundChangedNotifier.Changed,
				w => w.DesktopSwitchNotifier.Changed
			);
			var desktopChange = this.WhenAnyValue(
				vm => vm.DesktopManager.PreviousDesktop,
				vm => vm.DesktopManager.CurrentDesktop
			);
			this.LayoutManager = new LayoutManager(desktopChange);
			this.WindowManagerActions = new WindowManagerActions(this);

			var leaderKey = VirtualKeys.RightMenu;
			var keyboardParser = new KeyboardCombinationParser(leaderKey, this.WindowManagerActions, null);

			this.KeyboardHooks = new KeyboardHooks(keyboardParser);
		}


		private void Refresh() => MonitorScreens.Refresh();

		public void Dispose()
		{
			this.UserPreferenceChangedNotifier.Dispose();
			this.DisplaySettingsChangedNotifier.Dispose();
			this.WindowForegroundChangedNotifier.Dispose();
			this.DesktopSwitchNotifier.Dispose();
			this.KeyboardHooks.Dispose();
			this.DesktopManager.Dispose();
		}
	}
}
