using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WindowsDesktop;
using Awem.PInvoke;
using Awem.PInvoke.Enums;

namespace Awem
{
	public struct ApplicationWindow
	{
		public IntPtr WindowHandler { get; }

		public ApplicationWindow(IntPtr hWnd)
		{
			this.WindowHandler = hWnd;
		}

		public bool SufficientScreenRealEstate => this.HasSufficientScreenRealEstate();
		public bool IsWindows10CloakingWindow => IsInvisibleWin10BackgroundAppWindow(this.WindowHandler);
		public bool IsAltTabWindowOnDesktop(IntPtr desktopHandler) => IsAltTabWindowOnDesktop(this.WindowHandler, desktopHandler);
		public string ClassName => GetClassNameFromHwnd(this.WindowHandler);
		public string WindowTitle => GetWindowTitleFromHwnd(this.WindowHandler);

		public IntPtr Ancestor => User32.GetAncestor(this.WindowHandler, GetAncestorFlags.GetRoot);
		public IntPtr LastVisiblePopUpRoot => GetLastVisibleActivePopUpOfWindow(this.Ancestor);
		public Rectangle WindowPlacement => GetWindowInfoFromHwnd(this.WindowHandler).rcWindow.Rectangle;
		public VirtualDesktop Desktop => VirtualDesktop.FromHwnd(this.WindowHandler);

		private bool HasSufficientScreenRealEstate()
		{
			var info = GetWindowInfoFromHwnd(this.WindowHandler);
			return !(info.rcWindow.Width <= 10 && info.rcWindow.Height <= 10);
		}

		private static WindowInfo GetWindowInfoFromHwnd(IntPtr hwnd)
		{
			var info = new WindowInfo();
			User32.GetWindowInfo(hwnd, ref info);
			return info;
		}

		private static string GetClassNameFromHwnd(IntPtr hwnd)
		{
			var sb = new StringBuilder(256);
			var success = User32.GetClassName(hwnd, sb, sb.Capacity);
			return (success != 0) ? sb.ToString() : string.Empty;
		}

		private static string GetWindowTitleFromHwnd(IntPtr hwnd)
		{
			var capacity = User32.GetWindowTextLength(hwnd) * 2;
			var sb = new StringBuilder(capacity);
			var success = User32.GetWindowText(hwnd, sb, sb.Capacity);
			return (success != 0) ? sb.ToString() : string.Empty;
		}

		private static bool IsInvisibleWin10BackgroundAppWindow(IntPtr hwnd)
		{
			User32.DwmGetWindowAttribute(hwnd, (int) DwmWindowAttribute.Cloaked, out var isCloaked, Marshal.SizeOf(typeof(bool)));
			return isCloaked;
		}

		private static bool IsAltTabWindowOnDesktop(IntPtr window, IntPtr desktopHandler)
		{
			if (window == desktopHandler) return false;

			//http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
			//http://blogs.msdn.com/oldnewthing/archive/2007/10/08/5351207.aspx
			//1. For each visible window, walk up its owner chain until you find the root owner.
			//2. Then walk back down the visible last active popup chain until you find a visible window.
			//3. If you're back to where you're started, (look for exceptions) then put the window in the Alt+Tab list.
			var root = User32.GetAncestor(window, GetAncestorFlags.GetRoot);
			return GetLastVisibleActivePopUpOfWindow(root) == window;
		}

		private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
		{
			while (true)
			{
				var lastPopUp = User32.GetLastActivePopup(window);
				if (User32.IsWindowVisible(lastPopUp)) return lastPopUp;
				if (lastPopUp == window) return IntPtr.Zero;
				window = lastPopUp;
			}
		}
	}


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

		public static IEnumerable<ApplicationWindow> VisibleOnCurrentDesktop() =>
			from window in All()
			where !window.IsWindows10CloakingWindow
			where window.Desktop == VirtualDesktop.Current
			select window;

		private static IEnumerable<IntPtr> EnumerateAllTopLevelWindows()
		{
			var parentHandle = IntPtr.Zero;
			var childAfter = IntPtr.Zero;
			for (var i = 0; i < 10_000; i++)
			{
				childAfter = User32.FindWindowEx(parentHandle, childAfter, null, null);
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
