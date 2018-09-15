using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop;
using Awem.PInvoke;
using Awem.Windowing;

namespace Awem
{
	public class KeyboardHooks : IDisposable
	{
		private readonly DesktopManager _desktopManager;
		private readonly LowLevelKeyboardProc _hookCallback;

		public KeyboardHooks(DesktopManager desktopManager)
		{
			this._desktopManager = desktopManager;
			using (var curProcess = Process.GetCurrentProcess())
			using (var curModule = curProcess.MainModule)
			{
				this._hookCallback = this.HookCallback;
				_keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private readonly IntPtr _keyboardHookHandle;

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref LowLevelKeyboardEvent lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref LowLevelKeyboardEvent lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		private struct LowLevelKeyboardEvent
		{
			public int VirtualKeyCode;
			int scanCode;
			public int flags;
			int time;
			int dwExtraInfo;
		}

		private const int WH_KEYBOARD_LL = 13;
		private static readonly IntPtr WM_KEYDOWN = new IntPtr(0x0100);
		private static readonly IntPtr WM_KEYUP = new IntPtr(0x0101);

		private static readonly IntPtr WM_SYSKEYDOWN = new IntPtr(0x0104);
		private static readonly IntPtr WM_SYSKEYUP = new IntPtr(0x0105);

		private const int VK_LAUNCH_APP2 = 0x5C;

		private static readonly IntPtr _preventProcessing = new IntPtr(1);

		private bool _hotKeyIsDown = false;
		private IntPtr HookCallback(int nCode, IntPtr wParam, ref LowLevelKeyboardEvent lParam)
		{
			if (nCode < 0) return CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);

			var vkCode = lParam.VirtualKeyCode;
			if (vkCode != VK_LAUNCH_APP2)
			{
				if (!this._hotKeyIsDown || wParam != WM_KEYUP) return _preventProcessing;
				// x
				if (vkCode == 88) EventLoop.Break();
				// tab
				if (vkCode == 8)
				{
					this._desktopManager.GotoPreviousDesktop();
					return _preventProcessing;
				}
				if (vkCode == 65)
				{
					var windows = ApplicationWindows.VisibleOnCurrentDesktop().ToList();
					var window = windows.FirstOrDefault();
					window?.Activate();
					Console.WriteLine($"Activate: {window?.WindowTitle}");
					return _preventProcessing;
				}

				// numeric
				if (vkCode >= 48 && vkCode <= 58)
				{
					var key = vkCode - 48;
					var nth = key - 1 < 0 ? 9 : key - 1;
					this._desktopManager.GotoDesktop(nth);
					return _preventProcessing;
				}

				else Console.WriteLine($"{nCode} {vkCode} {lParam.flags}");
				return _preventProcessing;
			}

			this._hotKeyIsDown = wParam == (IntPtr) WM_KEYDOWN;

			return _preventProcessing;


			if (nCode >= 0 && (wParam == (IntPtr)WM_SYSKEYUP || wParam == (IntPtr)WM_SYSKEYDOWN))
			{
//				if (vkCode == 9 && lParam.flags == 32)
//				{
//					if (User32KeyboardHook.AlternativeAltTabBehavior != null)
//						User32KeyboardHook.AlternativeAltTabBehavior();
//					return new IntPtr(1);
//				}
			}
			return CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		public void Dispose() => UnhookWindowsHookEx(_keyboardHookHandle);

	}
}
