using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Win32;

namespace Awem.EventNotifiers
{
	public class UserPreferenceChangedNotifier : IDisposable
	{
		private readonly Subject<UserPreferenceChangedEventArgs> _changed = new Subject<UserPreferenceChangedEventArgs>();
		public IObservable<UserPreferenceChangedEventArgs> Changed => _changed.AsObservable();

		public UserPreferenceChangedNotifier() => SystemEvents.UserPreferenceChanged += this.OnChanged;

		public void Dispose() => SystemEvents.UserPreferenceChanged -= this.OnChanged;

		private void OnChanged(object sender, UserPreferenceChangedEventArgs e) => this._changed.OnNext(e);
	}
}
