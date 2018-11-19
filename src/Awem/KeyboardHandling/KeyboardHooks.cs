using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem.KeyboardHandling
{
	public class KeyboardHooks : IDisposable
	{
		private readonly KeyboardCombinationParser _parser;
		private readonly LowLevelKeyboardProc _hookCallback;

		public KeyboardHooks(KeyboardCombinationParser parser)
		{
			this._parser = parser;
			this.Combinations = parser.KeyboardCombinations;
			using (var curProcess = Process.GetCurrentProcess())
			using (var curModule = curProcess.MainModule)
			{
				_hookCallback = this.HookCallback;
				_keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, GetModuleHandle(curModule.ModuleName), 0);
			}
		}
		public IReadOnlyList<KeyboardCombination> Combinations { get; }

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
			public LowLevelKeyboardEventFlags flags;
			int time;
			int dwExtraInfo;
		}
		[Flags]
		private enum LowLevelKeyboardEventFlags : uint {
			LLKHF_EXTENDED = 0x01,
			LLKHF_INJECTED = 0x10,
			LLKHF_ALTDOWN = 0x20,
			LLKHF_UP = 0x80
		}

		private const int WH_KEYBOARD_LL = 13;
		private static readonly IntPtr WM_KEYDOWN = new IntPtr(0x0100);
		private static readonly IntPtr WM_KEYUP = new IntPtr(0x0101);

		private static readonly IntPtr WM_SYSKEYDOWN = new IntPtr(0x0104);
		private static readonly IntPtr WM_SYSKEYUP = new IntPtr(0x0105);

		private static readonly IntPtr _preventProcessing = new IntPtr(1);

		private bool _hotKeyIsDown;
		private HashSet<VirtualKeys> PressedKeys = new HashSet<VirtualKeys>();
		private bool _callingAction;

		private IntPtr HookCallback(int nCode, IntPtr wParam, ref LowLevelKeyboardEvent lParam)
		{
			if (nCode < 0) return CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);
			if (KeyboardStateManager.SimulatingKey) return _preventProcessing;

			var currentKey = (VirtualKeys)lParam.VirtualKeyCode;
			var isKeyUp = (wParam == WM_KEYUP || wParam == WM_SYSKEYUP);

			if (!isKeyUp) this.PressedKeys.Add(currentKey);

			var currentCombination = new KeyboardCombination(this.PressedKeys);

			var leaderKeyCurrent = currentKey == this._parser.LeaderKey;
			if (leaderKeyCurrent)
			{
				_hotKeyIsDown = !isKeyUp;
				if (isKeyUp) this.PressedKeys.Clear();

				return _preventProcessing;
			}

			if (!_hotKeyIsDown || this.PressedKeys.Count == 0)
			{
				this.PressedKeys.Clear();
				return CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);
			}

			var combinationDown = this.Combinations.FirstOrDefault(c => c.IsPressed(currentCombination));
			if (combinationDown == null || combinationDown.Shortcut.Count == 0)
			{
				if (isKeyUp) this.PressedKeys.Remove(currentKey);
				return !isKeyUp
					? _preventProcessing
					: CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);
			}

			//keyCombination is down, prevent from activating command unless we receive a keyup.
			//we could still be building up our key combination
			if (!isKeyUp) return _preventProcessing;

			combinationDown.Action.Invoke();

			if (isKeyUp) this.PressedKeys.Remove(currentKey);
			return _preventProcessing;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		public void Dispose() => UnhookWindowsHookEx(_keyboardHookHandle);

	}
}
