using System;
using System.Linq;
using Awem.Windowing;

namespace Awem
{
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