using System;
using System.Collections.ObjectModel;
using System.Linq;
using Awem.Windowing;
using DynamicData;

namespace Awem
{
	public class LayoutManager
	{
		private ReadOnlyObservableCollection<ApplicationWindow> _windows;
		public ReadOnlyObservableCollection<ApplicationWindow> CurrentDeskopWindows => _windows;


		public LayoutManager(IObservable<Tuple<int, int>> desktopChange)
		{
			var windowCollection = new SourceCache<ApplicationWindow, uint>(w=>w.ProcessId);

			desktopChange.Subscribe(d =>
			{
				var windows = ApplicationWindows.VisibleOnCurrentDesktop().ToList();
				var window = windows.FirstOrDefault();
				window?.Activate();
			});

		}
	}
}
