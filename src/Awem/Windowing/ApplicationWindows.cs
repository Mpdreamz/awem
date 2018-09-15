using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop;

namespace Awem.Windowing
{
	public static class ApplicationWindows
	{
		public static IEnumerable<ApplicationWindow> All() =>
			from hwnd in EnumerateAllTopLevelWindows()
			let window = new ApplicationWindow(hwnd)
			where !KnownNonDesktopApplicationFilter(window)
			where window.SufficientScreenRealEstate
			where window.Ancestor == window.LastVisiblePopUpRoot
			where window.Desktop != null
			select window;

		public static IEnumerable<ApplicationWindow> VisibleOnCurrentDesktop() => VisibleOnDesktop(VirtualDesktop.Current);
		public static IEnumerable<ApplicationWindow> VisibleOnDesktop(VirtualDesktop desktop) =>
			from window in All()
			where !window.IsWindows10CloakingWindow
			where window.Desktop == desktop
			select window;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		public static IEnumerable<IntPtr> EnumerateAllTopLevelWindows(string className = null)
		{
			var parentHandle = IntPtr.Zero;
			var childAfter = IntPtr.Zero;
			for (var i = 0; i < 10_000; i++)
			{
				childAfter = FindWindowEx(parentHandle, childAfter, className, null);
				if (childAfter == IntPtr.Zero) yield break;
				yield return childAfter;
			}
		}

		private static readonly string[] SkipClassNames =
		{
			"IME",
			//Windows 10 Hot Corner
			"EdgeUiInputWndClass", "EdgeUiInputTopWndClass",
			// Shell Tray
			"Shell_TrayWnd",
			// Progman,
			"WorkerW"
		};

		private static bool KnownNonDesktopApplicationFilter(ApplicationWindow window)
		{
			string className = window.ClassName, windowText = window.WindowTitle;
			if (string.IsNullOrEmpty(className) || SkipClassNames.Contains(className)) return true;
			if (className.StartsWith("Chrome_WidgetWin_") && string.IsNullOrEmpty(windowText)) return true;
			return false;
		}
	}
}
