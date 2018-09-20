using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WindowsDesktop;
using Awem.PInvoke.Enums;

namespace Awem.Windowing
{
	public class ApplicationWindow
	{
		public IntPtr WindowHandler { get; }

		public ApplicationWindow(IntPtr hWnd) => this.WindowHandler = hWnd;

		public bool SufficientScreenRealEstate => this.HasSufficientScreenRealEstate();
		public bool IsWindows10CloakingWindow => IsInvisibleWin10BackgroundAppWindow(this.WindowHandler);
		public bool IsAltTabWindowOnDesktop(IntPtr desktopHandler) => IsAltTabWindowOnDesktop(this.WindowHandler, desktopHandler);
		public string ClassName => GetClassNameFromHwnd(this.WindowHandler);
		public string WindowTitle => GetWindowTitleFromHwnd(this.WindowHandler);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		public uint ProcessId => GetWindowThreadProcessId(this.WindowHandler, out var p) == 0 ? p : p;
		public string ProcessName => this.GetProcessName();

		public IntPtr Ancestor => GetAncestor(this.WindowHandler, GetAncestorFlags.GetRoot);
		public IntPtr LastVisiblePopUpRoot => GetLastVisibleActivePopUpOfWindow(this.Ancestor);
		public Rectangle WindowPlacement => GetWindowInfoFromHwnd(this.WindowHandler).rcWindow.Rectangle;
		public VirtualDesktop Desktop => VirtualDesktop.FromHwnd(this.WindowHandler);
		public bool IsActive => this.WindowHandler == GetForegroundWindow();

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
		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(int hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("User32.dll")]
		private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

		[DllImport("user32.dll")]
		private static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		[DllImport("user32.dll", SetLastError=true)]
		private static extern IntPtr SetActiveWindow(IntPtr hWnd);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport("User32.dll")]
		private static extern short GetAsyncKeyState(int vKey);
		private const int VK_MENU = 0x12;
		private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
		private const int KEYEVENTF_KEYUP = 0x0002;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		public void Activate()
		{
			// Routine taken from
			// https://github.com/robinwassen/forcefocus/blob/master/src/forcefocus_win.cc
			// Windows 10 has gained the abbility to not activate a window and steal focus
			// opting instead to selectively flash an application in the taskbar instead when it wants to grab focus.
			// This is brilliant for day to day use, however we want to force the focus reliably.
			// Another side effect of ForegroundLockTimeout is that when switching desktops the active window
			// randomly loses focus and starts to act as if its pinned flashing on the taskbar and travelling along the desktop switches
			// NOT a brilliant feature.
			// Googling fixes for this yields folks going as far as patching explorer.exe using AHK.
			// We (hopefully) get around doing this by activating the first window explicitly on each desktop switch.

			var pressed = false;
			if (!KeyboardStateManager.MenuKeyIsDown)
			{
				pressed = true;
				KeyboardStateManager.SimulateKeyDown(VirtualKeys.Menu);
			}

			SetForegroundWindow(this.WindowHandler);
			SetActiveWindow(this.WindowHandler);
			SetFocus(this.WindowHandler);


			if (pressed) {
				KeyboardStateManager.SimulateKeyUp(VirtualKeys.Menu);
			}

			return;

			//if (this.WindowHandler == GetForegroundWindow()) return;
			//cmd needs to be called synchronous.
			if (this.ProcessName.EndsWith("cmd.exe"))
			{
				if (IsIconic(this.WindowHandler)) ShowWindow(this.WindowHandler, (int)WindowState.SW_RESTORE);
				SetForegroundWindow(this.WindowHandler);
				SetActiveWindow(this.WindowHandler);
				SetFocus(this.WindowHandler);
				return;
			}

			var threadId1 = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
			var threadId2 = GetWindowThreadProcessId(this.WindowHandler, IntPtr.Zero);

			if (threadId1 != threadId2)
			{
				AttachThreadInput(threadId1, threadId2, 1);
				SetForegroundWindow(this.WindowHandler);
				SetActiveWindow(this.WindowHandler);
				SwitchToThisWindow(this.WindowHandler, true);
				SetFocus(this.WindowHandler);
				AttachThreadInput(threadId1, threadId2, 0);
			}
			else
			{
				SetForegroundWindow(this.WindowHandler);
				SetActiveWindow(this.WindowHandler);
				SwitchToThisWindow(this.WindowHandler, true);
				SetFocus(this.WindowHandler);
			}

			return;

			if (IsIconic(this.WindowHandler))
				ShowWindowAsync(this.WindowHandler, (int)WindowState.SW_RESTORE);
			else
			{
				if (IsZoomed(this.WindowHandler))
					ShowWindowAsync(this.WindowHandler, (int)WindowState.SW_SHOWMAXIMIZED);
				else
					ShowWindowAsync(this.WindowHandler, (int)WindowState.SW_SHOWNORMAL);
			}
		}
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);
		[DllImport("psapi.dll")]
		private static extern uint GetModuleFileNameEx(
			IntPtr hProcess, IntPtr hModule,
			[Out] StringBuilder lpBaseName,
			[In] [MarshalAs(UnmanagedType.U4)] int nSize
		);

		private string _processName;
		private string GetProcessName()
		{
			if (!string.IsNullOrEmpty(this._processName)) return this._processName;

			const int nChars = 1024;
			var filename = new StringBuilder(nChars);
			GetWindowThreadProcessId(this.WindowHandler, out var processId);
			var hProcess = OpenProcess(1040, 0, processId);
			GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);
			CloseHandle(hProcess);
			var r = filename.ToString();
			if (r.StartsWith("?"))
				r = "C" + r.Substring(1, r.Length - 1);
			this._processName = r;
			return r;
		}
		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
		private const int WM_KILLFOCUS = 0x0008;

		public void KillFocus()
		{
			SendMessage(this.WindowHandler, WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
