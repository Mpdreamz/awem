using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using WindowsDesktop;
using Awem.EventNotifiers;
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

		private KeyboardHooks KeyboardHooks { get; }
		private DesktopManager DesktopManager { get; }
		private LayoutManager LayoutManager { get; }

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
			this.KeyboardHooks = new KeyboardHooks(this.DesktopManager);
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
//			windowsChanged.Select(i => (object) i).Merge(displayChanged)
//				.Subscribe(i => MonitorScreens.EnumerateScreens());
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

	public class LayoutManager
	{
		public LayoutManager(IObservable<Tuple<int, int>> desktopChange)
		{
			desktopChange.Subscribe(d =>
			{
				var previous = VirtualDesktop.GetDesktops()[d.Item1];
//				var oldWindows = ApplicationWindows.VisibleOnDesktop(previous);
//				foreach(var oldWindow in oldWindows)
//					oldWindow.KillFocus();

				var windows = ApplicationWindows.VisibleOnCurrentDesktop().ToList();
				var window = windows.FirstOrDefault();
				window?.Activate();
			});

		}
	}
}
