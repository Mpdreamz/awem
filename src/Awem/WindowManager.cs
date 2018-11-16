using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using WindowsDesktop;
using Awem.EventNotifiers;
using Awem.PInvoke;
using Awem.PInvoke.Enums;
using Awem.Windowing;
using ReactiveUI;

namespace Awem
{
	public class WindowManagerActions
	{
		private readonly WindowManager _manager;

		public Action<int> MoveToDesktop { get; }
		public Action<int> GotoDesktop { get; }
		public Action GotoPreviousDesktop { get; }
		public Action CreateDesktop { get; }
		public Action RemoveDesktop { get; }
		public Action Die { get; }

		public IDictionary<string, Action> Commands { get;  } =
			new Dictionary<string, Action>(StringComparer.CurrentCultureIgnoreCase);

		public IDictionary<string, Action<int>> NumericCommands { get;  }=
			new Dictionary<string, Action<int>>(StringComparer.CurrentCultureIgnoreCase);

		public WindowManagerActions(WindowManager manager)
		{
			this._manager = manager;
			this.GotoDesktop = i => _manager.DesktopManager.GotoDesktop(i);
			this.MoveToDesktop = i => _manager.DesktopManager.MoveToDesktop(i, ApplicationWindows.Current);
			this.GotoPreviousDesktop = () => _manager.DesktopManager.GotoPreviousDesktop();
			this.CreateDesktop = () => _manager.DesktopManager.CreateDesktop();
			this.RemoveDesktop = () => _manager.DesktopManager.RemoveDesktop();
			this.Die = EventLoop.Break;

			var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var prop in props)
			{
				if (prop.PropertyType == typeof(Action<int>))
				{
					var v = prop.GetMethod.Invoke(this, null) as Action<int>;
					this.NumericCommands.Add(prop.Name, v);
				}
				else if (prop.PropertyType == typeof(Action))
				{
					var v = prop.GetMethod.Invoke(this, null) as Action;
					this.Commands.Add(prop.Name, v);
				}
			}
		}

	}


	public class WindowManager : ReactiveObject, IDisposable
	{
		private UserPreferenceChangedNotifier UserPreferenceChangedNotifier { get; }
		private DisplaySettingsChangedNotifier DisplaySettingsChangedNotifier { get; }
		private WindowForegroundChangedNotifier WindowForegroundChangedNotifier { get; }
		private DesktopSwitchNotifier DesktopSwitchNotifier { get; }

		private KeyboardHooks KeyboardHooks { get; }
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
			var keyboardParser = new KeyboardCombinationParser(leaderKey, this.WindowManagerActions);

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

	public class LayoutManager
	{
		public LayoutManager(IObservable<Tuple<int, int>> desktopChange)
		{
			desktopChange.Subscribe(d =>
			{
				var windows = ApplicationWindows.VisibleOnCurrentDesktop().ToList();
				var window = windows.FirstOrDefault();
				window?.Activate();
			});

		}
	}
}
