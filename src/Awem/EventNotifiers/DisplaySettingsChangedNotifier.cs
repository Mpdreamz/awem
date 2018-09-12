using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Awem.Windowing;
using Microsoft.Win32;

namespace Awem.EventNotifiers
{
	public class DisplaySettingsChangedNotifier : IDisposable
	{
		private readonly Subject<EventArgs> _changed = new Subject<EventArgs>();
		public IObservable<EventArgs> Changed => _changed.AsObservable();

		public DisplaySettingsChangedNotifier() => SystemEvents.DisplaySettingsChanged += this.OnChanged;

		public void Dispose() => SystemEvents.DisplaySettingsChanged -= this.OnChanged;

		private void OnChanged(object sender, EventArgs e) => this._changed.OnNext(e);
	}
}
