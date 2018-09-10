using System;
using System.Runtime.InteropServices;
using System.Text;
using Awem.PInvoke.Enums;

namespace Awem.PInvoke
{
	public static class User32
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern IntPtr GetShellWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetLastActivePopup(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo pwi);

		[DllImport("dwmapi.dll")]
		public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);
		
	}
}
