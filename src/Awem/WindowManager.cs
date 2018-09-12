using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

		public ICollection<MonitorScreen> Monitors => MonitorScreens.All();
		public ICollection<ApplicationWindow> AllApplications => ApplicationWindows.All().ToList();
		public ICollection<ApplicationWindow> CurrentDesktopApplications => ApplicationWindows.VisibleOnCurrentDesktop().ToList();

		public WindowManager()
		{
			this.UserPreferenceChangedNotifier = new UserPreferenceChangedNotifier();
			this.DisplaySettingsChangedNotifier = new DisplaySettingsChangedNotifier();
			this.WindowForegroundChangedNotifier = new WindowForegroundChangedNotifier();
			this.DesktopSwitchNotifier = new DesktopSwitchNotifier();
			var displayChanged = this.WhenAnyObservable(
				w => w.UserPreferenceChangedNotifier.Changed,
				w => w.DisplaySettingsChangedNotifier.Changed
			);
			var windowsChanged = this.WhenAnyObservable(
				w => w.WindowForegroundChangedNotifier.Changed,
				w => w.DesktopSwitchNotifier.Changed
			);
			windowsChanged.Select(i => (object) i).Merge<object>(displayChanged)
				.Subscribe(i => MonitorScreens.EnumerateScreens());
		}

		private void Refresh() => MonitorScreens.Refresh();

		public void Dispose()
		{
			this.UserPreferenceChangedNotifier.Dispose();
			this.DisplaySettingsChangedNotifier.Dispose();
			this.WindowForegroundChangedNotifier.Dispose();
			this.DesktopSwitchNotifier.Dispose();
		}

	}
}
