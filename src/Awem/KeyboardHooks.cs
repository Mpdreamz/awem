using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Awem.PInvoke;
using Awem.PInvoke.Enums;
using Awem.Windowing;

namespace Awem
{
	public class KeyboardStateManager
	{
		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(ushort vKey);

		[DllImport("user32.dll")]
		private static extern short GetKeyState(ushort vKey);

		private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
		private const int KEYEVENTF_KEYUP = 0x0002;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		public static bool KeyIsDown(VirtualKeys key)
		{
			//expand shift, menu, and control to include explicit left and right states
			switch (key)
			{
				case VirtualKeys.Shift:
					return (GetKeyState((ushort)key) & 0x8000) != 0
					       || KeyIsDown(VirtualKeys.LeftShift)
					       || KeyIsDown(VirtualKeys.RightShift);

				case VirtualKeys.Control:
					return (GetKeyState((ushort)key) & 0x8000) != 0
					       || KeyIsDown(VirtualKeys.LeftControl)
					       || KeyIsDown(VirtualKeys.RightControl);
				case VirtualKeys.Menu:
					return (GetKeyState((ushort)key) & 0x8000) != 0
					       || KeyIsDown(VirtualKeys.LeftMenu)
					       || KeyIsDown(VirtualKeys.RightMenu);

				default: return (GetKeyState((ushort)key) & 0x8000) != 0;
			}
		}
		public static bool MenuKeyIsDown => KeyIsDown(VirtualKeys.Menu);

		public static void SimulateKeyDown(VirtualKeys key) => keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);

		public static void SimulateKeyUp(VirtualKeys key) => keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
	}

	public class KeyboardCombination
	{
		private readonly string _errorMessage;
		public HashSet<VirtualKeys> Shortcut { get; }
		public HashSet<VirtualKeys>[] ShortcutAlternatives { get; }
		public bool IsPressed(KeyboardCombination combination) => ShortcutAlternatives.Any(p => p.SetEquals(combination.Shortcut));

		protected internal KeyboardCombination(HashSet<VirtualKeys> shortcut, string errorMessage = null)
		{
			this.Shortcut = shortcut ?? new HashSet<VirtualKeys>();
			this.ShortcutAlternatives = CreateShortCutAlternatives(shortcut).ToArray();
			_errorMessage = errorMessage;
		}

		private static IEnumerable<HashSet<VirtualKeys>> CreateShortCutAlternatives(HashSet<VirtualKeys> shortcut)
		{
			yield return shortcut;
			if (shortcut.Contains(VirtualKeys.Shift))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Shift, VirtualKeys.RightShift)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Shift, VirtualKeys.LeftShift)) yield return p;
			}
			else if (shortcut.Contains(VirtualKeys.Control))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Control, VirtualKeys.LeftControl)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Control, VirtualKeys.RightControl)) yield return p;
			}
			else if (shortcut.Contains(VirtualKeys.Menu))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Menu, VirtualKeys.LeftMenu)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Menu, VirtualKeys.RightMenu)) yield return p;
			}
		}

		private static IEnumerable<HashSet<VirtualKeys>> LocalAlternatives(HashSet<VirtualKeys> shortcut, VirtualKeys oldKey, VirtualKeys newKey)
		{
			var a = CreateAlternative(shortcut, oldKey, newKey);
			foreach (var al in CreateShortCutAlternatives(a)) yield return al;
		}

		private static HashSet<VirtualKeys> CreateAlternative(HashSet<VirtualKeys> shortcut, VirtualKeys oldKey, VirtualKeys newKey)
		{
			var temp = new VirtualKeys[shortcut.Count];
			shortcut.CopyTo(temp);
			var alternative = new HashSet<VirtualKeys>(temp);
			alternative.Remove(oldKey);
			alternative.Add(newKey);
			return alternative;
		}

		public static KeyboardCombination Parse(string combination)
		{
			var keys = combination.Split(new [] {"+"}, StringSplitOptions.RemoveEmptyEntries)
				.Select(key => key.Trim())
				.ToArray();

			var virtualKeys = keys.Select(k => new
			{
				parsed = TryParseSingleKey(k, out var key),
				key
			}).ToArray();
			if (virtualKeys.All(v => v.parsed))
			{
				var shortCut = new HashSet<VirtualKeys>(virtualKeys.Select(v => v.key).ToArray());
				shortCut.Add(VirtualKeys.RightWindows);
				return new KeyboardCombination(shortCut);
			}

			var invalidKeys = string.Join(", ", virtualKeys.Where(v => !v.parsed).Select(v => v.key));
			var errorMessage = $"The following keys could not be parsed:{invalidKeys}";
			return new KeyboardCombination(null, errorMessage);
		}

		private static bool TryParseSingleKey(string key, out VirtualKeys virtualKey)
		{
			if (int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var i))
				key = $"N{i}";
			return Enum.TryParse(key, ignoreCase: true, out virtualKey);
		}

		public override string ToString()
		{
			var keys = this.Shortcut.Select(k => Enum.GetName(typeof(VirtualKeys), k));
			return $"{string.Join("+", keys)}";
		}

		public static readonly KeyboardCombination[] Combinations = new[]
		{
			Parse("N0"),
			Parse("1"),
			Parse("N2"),
			Parse("3"),
			Parse("N4"),
			Parse("Shift+N0"),
			Parse("Shift+1"),
			Parse("Shift+N2"),
			Parse("Shift+3"),
			Parse("Shift+N4"),
			Parse("X"),
			Parse("Back"),
		}.OrderByDescending(p=>p.Shortcut.Count).ToArray();
	}

	public class KeyboardHooks : IDisposable
	{
		private readonly DesktopManager _desktopManager;
		private readonly LowLevelKeyboardProc _hookCallback;

		public KeyboardHooks(DesktopManager desktopManager)
		{
			_desktopManager = desktopManager;
			using (var curProcess = Process.GetCurrentProcess())
			using (var curModule = curProcess.MainModule)
			{
				_hookCallback = this.HookCallback;
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

		private const int VK_LAUNCH_APP2 = 0x5C;

		private static readonly IntPtr _preventProcessing = new IntPtr(1);

		private bool _hotKeyIsDown;
		private HashSet<VirtualKeys> PressedKeys = new HashSet<VirtualKeys>();

		private IntPtr HookCallback(int nCode, IntPtr wParam, ref LowLevelKeyboardEvent lParam)
		{
			if (nCode < 0) return CallNextHookEx(_keyboardHookHandle, nCode, wParam, ref lParam);

			const VirtualKeys leaderKey = VirtualKeys.RightWindows;
			var currentKey = (VirtualKeys)lParam.VirtualKeyCode;
			var isKeyUp = (wParam == WM_KEYUP || wParam == WM_SYSKEYUP);
			if (!isKeyUp) this.PressedKeys.Add(currentKey);
			var currentCombination = new KeyboardCombination(this.PressedKeys);

			var leaderKeyCurrent = currentKey == leaderKey;
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

			var combinationDown = KeyboardCombination.Combinations.FirstOrDefault(c => c.IsPressed(currentCombination));
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

			Console.WriteLine($"-> {combinationDown}");

			if (currentKey == VirtualKeys.X) EventLoop.Break();
//			// tab
//			else if (currentKey == VirtualKeys.Back)
//			{
//				_desktopManager.GotoPreviousDesktop();
//			}
//
//			// numeric
//			else if (currentKey >= VirtualKeys.N0 && currentKey <= VirtualKeys.N9)
//			{
//				var key = (int) currentKey - 48;
//				var nth = key - 1 < 0 ? 9 : key - 1;
//				if (shiftPressed)
//					_desktopManager.MoveToDesktop(nth, ApplicationWindows.Current);
//				else _desktopManager.GotoDesktop(nth);
//			}
			if (isKeyUp) this.PressedKeys.Remove(currentKey);
			return _preventProcessing;

			if (nCode >= 0 && (wParam == WM_SYSKEYUP || wParam == WM_SYSKEYDOWN))
			{
//				if (currentKey == 9 && lParam.flags == 32)
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
