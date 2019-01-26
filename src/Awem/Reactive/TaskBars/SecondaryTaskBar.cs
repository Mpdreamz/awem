using System;
using System.Collections.Generic;
using Awem.Windowing;

namespace Awem.Reactive.TaskBars
{
	public sealed class SecondaryTaskBar : TaskBarBase, IDisposable
	{
		private readonly ApplicationWindow _window;
		private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

		public SecondaryTaskBar(TaskBarHandle taskBarHandle, IObservable<bool> alwaysOnTop, IObservable<bool> autoHide)
			: base(taskBarHandle)
		{
			_window = new ApplicationWindow(taskBarHandle);
			_subscriptions.Add(alwaysOnTop.Subscribe(b => this.AlwaysOnTop = b));
			_subscriptions.Add(autoHide.Subscribe(b => this.AutoHide = b));
			this.Refresh();
		}

		public override void Refresh()
		{
			this.Bounds = _window.WindowPlacement;
			this.Position = TaskBarPosition.Unknown;
		}

		public void Dispose()
		{
			foreach(var s in _subscriptions) s.Dispose();
		}
	}
}
