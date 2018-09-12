using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Awem.Windowing
{
	public static class TaskBars
	{
		private static ConcurrentDictionary<IntPtr, TaskBarBase> TaskBarsCache = new ConcurrentDictionary<IntPtr, TaskBarBase>();

		public static ICollection<TaskBarBase> All() => TaskBarsCache.Values;

		private const string ClassName = "Shell_TrayWnd";
		private const string SecondaryClassName = "Shell_SecondaryTrayWnd";
		public static void Refresh()
		{
			var primaryWindowHandler = ApplicationWindows.EnumerateAllTopLevelWindows(ClassName).First();
			var primary = (PrimaryTaskBar)TaskBarsCache.GetOrAdd(primaryWindowHandler, i => new PrimaryTaskBar(primaryWindowHandler));
			primary.Refresh();

			var windows = ApplicationWindows.EnumerateAllTopLevelWindows(SecondaryClassName);
			foreach (var taskBarHandle in windows)
				TaskBarsCache.GetOrAdd(taskBarHandle, i => new SecondaryTaskBar(new ApplicationWindow(taskBarHandle), primary.AlwaysOnTop, primary.AutoHide))
					.Refresh();
		}
	}
}