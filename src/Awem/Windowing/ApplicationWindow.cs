using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using WindowsDesktop;
using Awem.PInvoke.Enums;

namespace Awem.Windowing
{
	public struct ApplicationWindow
	{
		private IntPtr WindowHandler { get; }

		public ApplicationWindow(IntPtr hWnd) => this.WindowHandler = hWnd;

		public bool SufficientScreenRealEstate => this.HasSufficientScreenRealEstate();
		public bool IsWindows10CloakingWindow => IsInvisibleWin10BackgroundAppWindow(this.WindowHandler);
		public bool IsAltTabWindowOnDesktop(IntPtr desktopHandler) => IsAltTabWindowOnDesktop(this.WindowHandler, desktopHandler);
		public string ClassName => GetClassNameFromHwnd(this.WindowHandler);
		public string WindowTitle => GetWindowTitleFromHwnd(this.WindowHandler);

		public IntPtr Ancestor => GetAncestor(this.WindowHandler, GetAncestorFlags.GetRoot);
		public IntPtr LastVisiblePopUpRoot => GetLastVisibleActivePopUpOfWindow(this.Ancestor);
		public Rectangle WindowPlacement => GetWindowInfoFromHwnd(this.WindowHandler).rcWindow.Rectangle;
		public VirtualDesktop Desktop => VirtualDesktop.FromHwnd(this.WindowHandler);

		private bool HasSufficientScreenRealEstate()
		{
			var info = GetWindowInfoFromHwnd(this.WindowHandler);
			return !(info.rcWindow.Width <= 10 && info.rcWindow.Height <= 10);
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo pwi);
		private static WindowInfo GetWindowInfoFromHwnd(IntPtr hwnd)
		{
			var info = new WindowInfo();
			GetWindowInfo(hwnd, ref info);
			return info;
		}

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
		private static string GetClassNameFromHwnd(IntPtr hwnd)
		{
			var sb = new StringBuilder(256);
			var success = GetClassName(hwnd, sb, sb.Capacity);
			return (success != 0) ? sb.ToString() : string.Empty;
		}

		[DllImport("user32.dll")]
		private static extern int GetWindowTextLength(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		private static string GetWindowTitleFromHwnd(IntPtr hwnd)
		{
			var capacity = GetWindowTextLength(hwnd) * 2;
			var sb = new StringBuilder(capacity);
			var success = GetWindowText(hwnd, sb, sb.Capacity);
			return (success != 0) ? sb.ToString() : string.Empty;
		}

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);
		private static bool IsInvisibleWin10BackgroundAppWindow(IntPtr hwnd)
		{
			DwmGetWindowAttribute(hwnd, (int) DwmWindowAttribute.Cloaked, out var isCloaked, Marshal.SizeOf(typeof(bool)));
			return isCloaked;
		}

		[DllImport("user32.dll", ExactSpelling = true)]
		private static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);
		private static bool IsAltTabWindowOnDesktop(IntPtr window, IntPtr desktopHandler)
		{
			if (window == desktopHandler) return false;

			//http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
			//http://blogs.msdn.com/oldnewthing/archive/2007/10/08/5351207.aspx
			//1. For each visible window, walk up its owner chain until you find the root owner.
			//2. Then walk back down the visible last active popup chain until you find a visible window.
			//3. If you're back to where you're started, (look for exceptions) then put the window in the Alt+Tab list.
			var root = GetAncestor(window, GetAncestorFlags.GetRoot);
			return GetLastVisibleActivePopUpOfWindow(root) == window;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetLastActivePopup(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);
		private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
		{
			while (true)
			{
				var lastPopUp = GetLastActivePopup(window);
				if (IsWindowVisible(lastPopUp)) return lastPopUp;
				if (lastPopUp == window) return IntPtr.Zero;
				window = lastPopUp;
			}
		}
	}
}
