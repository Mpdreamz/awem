using System;
using System.Drawing;
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

		public ApplicationWindow(IntPtr hWnd) => this.WindowHandler = hWnd;

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
}