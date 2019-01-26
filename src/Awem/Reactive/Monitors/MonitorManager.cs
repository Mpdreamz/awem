using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Awem.EventNotifiers;
using Awem.Reactive.TaskBars;
using DynamicData;
using ReactiveUI;

namespace Awem.Reactive.Monitors
{
	public class MonitorManager : ReactiveObject, IDisposable
	{

		private UserPreferenceChangedNotifier UserPreferenceChangedNotifier { get; }
		private DisplaySettingsChangedNotifier DisplaySettingsChangedNotifier { get; }

		private readonly SourceCache<MonitorHandle, MonitorHandle> _monitorCache = new SourceCache<MonitorHandle, MonitorHandle>(v => v);
		private readonly ReadOnlyObservableCollection<MonitorScreen> _monitors;
		public ReadOnlyObservableCollection<MonitorScreen> Monitors => _monitors;

		private readonly SourceCache<TaskBarHandle, TaskBarHandle> _taskBarCache = new SourceCache<TaskBarHandle, TaskBarHandle>(v => v);
		private readonly ReadOnlyObservableCollection<TaskBarBase> _taskBars;
		public ReadOnlyObservableCollection<TaskBarBase> TaskBars => _taskBars;

		public IObservable<EventArgs> DisplayConfigationChanged { get; }

		private Subject<bool> TaskBarIsOnTop { get; } = new Subject<bool>();
		private Subject<bool> TaskBarIsAutoHide { get; } = new Subject<bool>();

		public MonitorManager()
		{
			this.UserPreferenceChangedNotifier = new UserPreferenceChangedNotifier();
			this.DisplaySettingsChangedNotifier = new DisplaySettingsChangedNotifier();

			this.DisplayConfigationChanged = this.WhenAnyObservable(
				w => w.UserPreferenceChangedNotifier.Changed,
				w => w.DisplaySettingsChangedNotifier.Changed
			);

			_taskBarCache.Connect()
				.Transform(this.CreateTaskBar)
				.OnItemRemoved(RemoveTaskBar)
				.Bind(out _taskBars);


			_monitorCache.Connect()
				.Transform(this.CreateMonitor)
				.OnItemRemoved(RemoveMonitor)
				.Bind(out _monitors);

			this.DisplayConfigationChanged.Subscribe(e =>
			{
				this.EnumerateTaskBars();
				this.EnumerateWindows();
			});
			this.EnumerateTaskBars();
			this.EnumerateWindows();
		}

		private void EnumerateWindows() =>
			_monitorCache.Edit(i =>
			{
				var currentMonitors = MonitorWin32.EnumerateDisplayMonitors();
				i.Clear();
				i.AddOrUpdate(currentMonitors);
			});

		private void EnumerateTaskBars() =>
			_taskBarCache.Edit(i =>
			{
				var taskBars = TaskBarWin32.EnumerateAllTaskBars();
				i.Clear();
				i.AddOrUpdate(taskBars);
			});


		private static readonly ConcurrentDictionary<MonitorHandle, MonitorScreen> CachedMonitors = new ConcurrentDictionary<MonitorHandle, MonitorScreen>();
		private static void RemoveMonitor(MonitorScreen monitor) => CachedMonitors.TryRemove(monitor.MonitorHandle, out var _);
		private MonitorScreen CreateMonitor(MonitorHandle monitorHandle)
		{
			var monitor = CachedMonitors.GetOrAdd(monitorHandle, (k) => new MonitorScreen(k, this.TaskBars));
			monitor.ScaledResolution = monitorHandle.Rectangle;
			monitor.Refresh();
			return monitor;
		}

		private static readonly ConcurrentDictionary<TaskBarHandle, TaskBarBase> CachedTaskBars = new ConcurrentDictionary<TaskBarHandle, TaskBarBase>();
		private static void RemoveTaskBar(TaskBarBase taskBar) => CachedTaskBars.TryRemove(taskBar.TaskBarHandle, out var _);
		private TaskBarBase CreateTaskBar(TaskBarHandle taskBarHandle)
		{
			var taskBar = CachedTaskBars.GetOrAdd(taskBarHandle,
				k => k.Primary ? new PrimaryTaskBar(k) : (TaskBarBase)new SecondaryTaskBar(k, this.TaskBarIsOnTop, this.TaskBarIsAutoHide)
			);
			if (taskBar is PrimaryTaskBar primaryTaskBar)
			{
				this.TaskBarIsOnTop.OnNext(primaryTaskBar.AlwaysOnTop);
				this.TaskBarIsAutoHide.OnNext(primaryTaskBar.AutoHide);
			}
			taskBar.Refresh();
			return taskBar;
		}

		public void Dispose()
		{
			this.UserPreferenceChangedNotifier.Dispose();
			this.DisplaySettingsChangedNotifier.Dispose();
		}
	}
}
