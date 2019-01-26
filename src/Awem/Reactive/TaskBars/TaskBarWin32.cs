using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;
using Awem.Windowing;

namespace Awem.Reactive.TaskBars
{
	public static class TaskBarWin32
	{

		[DllImport("shell32.dll", SetLastError = true)]
		internal static extern IntPtr SHAppBarMessage(ApplicationBarCommand dwMessage, [In] ref ApplicationBarData pData);

		private const string ClassName = "Shell_TrayWnd";
		private const string SecondaryClassName = "Shell_SecondaryTrayWnd";

		public static IEnumerable<TaskBarHandle> EnumeratePrimaries() =>
			from w in ApplicationWindows.EnumerateAllTopLevelWindows(ClassName)
			select new TaskBarHandle(w, primary: true);

		public static IEnumerable<TaskBarHandle> EnumerateSecondaries() =>
			from w in ApplicationWindows.EnumerateAllTopLevelWindows(SecondaryClassName)
			select new TaskBarHandle(w, primary: false);

		public static IEnumerable<TaskBarHandle> EnumerateAllTaskBars() =>
			EnumeratePrimaries().Union(EnumerateSecondaries());
	}
}
