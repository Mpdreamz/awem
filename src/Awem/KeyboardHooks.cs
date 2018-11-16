﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Awem.PInvoke;
using Awem.PInvoke.Enums;
using ReactiveUI;

namespace Awem
{

	public class KeyboardCombinationParser
	{
		private readonly WindowManagerActions _windowManagerActions;

		public KeyboardCombinationParser(WindowManagerActions windowManagerActions) => _windowManagerActions = windowManagerActions;

		private readonly string[] DefaultShortcuts = new[]
		{
			$"{nameof(WindowManagerActions.GotoDesktop)}0=N1",
			$"{nameof(WindowManagerActions.GotoDesktop)}1=2",
			$"{nameof(WindowManagerActions.GotoDesktop)}2=N3",
			$"{nameof(WindowManagerActions.GotoDesktop)}3=4",
			$"{nameof(WindowManagerActions.GotoDesktop)}4=N5",
			$"{nameof(WindowManagerActions.GotoDesktop)}5=N6",
			$"{nameof(WindowManagerActions.GotoDesktop)}6=N7",
			$"{nameof(WindowManagerActions.GotoDesktop)}7=N8",
			$"{nameof(WindowManagerActions.GotoDesktop)}8=N9",
			$"{nameof(WindowManagerActions.GotoDesktop)}9=N10",
			$"{nameof(WindowManagerActions.GotoDesktop)}0=N11",
			$"{nameof(WindowManagerActions.MoveToDesktop)}0=Shift+N1",
			$"{nameof(WindowManagerActions.MoveToDesktop)}1=Shift+2",
			$"{nameof(WindowManagerActions.MoveToDesktop)}2=Shift+N3",
			$"{nameof(WindowManagerActions.MoveToDesktop)}3=Shift+4",
			$"{nameof(WindowManagerActions.MoveToDesktop)}4=Shift+N5",
			$"{nameof(WindowManagerActions.MoveToDesktop)}5=Shift+6",
			$"{nameof(WindowManagerActions.MoveToDesktop)}6=Shift+7",
			$"{nameof(WindowManagerActions.MoveToDesktop)}7=Shift+8",
			$"{nameof(WindowManagerActions.MoveToDesktop)}8=Shift+9",
			$"{nameof(WindowManagerActions.MoveToDesktop)}9=Shift+10",
			$"{nameof(WindowManagerActions.MoveToDesktop)}0=Shift+11",
			$"{nameof(WindowManagerActions.Die)}=X",
			$"{nameof(WindowManagerActions.GotoPreviousDesktop)}=Back",
		};


		public KeyboardCombination[] ToKeyboardCombinations(IEnumerable<string> config = null)
		{
			var c = DefaultShortcuts.Union(config ?? Enumerable.Empty<string>());
			return c.Select(this.Parse).Where(p=>p.Shortcut != null).OrderByDescending(p=>p.Shortcut.Count).ToArray();
		}

		public KeyboardCombination Parse(string combination)
		{
			var tokens = combination.Split(new[] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length != 2)
				return new KeyboardCombination($"Could not parse a key and value from {combination}");

			var shortcut = tokens[1].Trim();
			var command = tokens[0].Trim();
			Action action = null;
			if (Regex.IsMatch(command, @"\d+$"))
			{
				var subTokens = Regex.Split(command, @"(?=\d)");
				command = subTokens[0];
				var numeric = int.Parse(subTokens[1]);
				if (this._windowManagerActions.NumericCommands.TryGetValue(command, out var r) && r != null)
					action = () => r(numeric);
				else
					return new KeyboardCombination($"{command}{numeric} is not a valid action");
			}
			else
			{
				if (this._windowManagerActions.Commands.TryGetValue(command, out var r) && r != null)
					action = r;
				else
					return new KeyboardCombination($"{command} is not a valid action");
			}
			return ParseShortcut(shortcut, action);

		}

		private static KeyboardCombination ParseShortcut(string shortcut, Action action)
		{
			var keys = shortcut.Split(new[] {"+"}, StringSplitOptions.RemoveEmptyEntries)
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
				return new KeyboardCombination(shortCut, action);
			}

			var invalidKeys = string.Join(", ", virtualKeys.Where(v => !v.parsed).Select(v => v.key));
			var errorMessage = $"The following keys could not be parsed:{invalidKeys}";
			return new KeyboardCombination(errorMessage);
		}

		private static bool TryParseSingleKey(string key, out VirtualKeys virtualKey)
		{
			if (int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var i))
				key = $"N{i}";
			return Enum.TryParse(key, ignoreCase: true, out virtualKey);
		}
	}



	public class KeyboardCombination
	{
		private readonly string _errorMessage;
		public HashSet<VirtualKeys> Shortcut { get; }
		public Action Action { get; }
		public HashSet<VirtualKeys>[] ShortcutAlternatives { get; }
		public bool IsPressed(KeyboardCombination combination) => ShortcutAlternatives.Any(p => p.SetEquals(combination.Shortcut));

		protected internal KeyboardCombination(string errorMessage = null) => this._errorMessage = errorMessage;

		protected internal KeyboardCombination(HashSet<VirtualKeys> shortcut, Action action = null)
		{
			this.Shortcut = shortcut ?? new HashSet<VirtualKeys>();
			this.ShortcutAlternatives = CreateShortCutAlternatives(shortcut).ToArray();
			this.Action = action;
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

		public override string ToString()
		{
			var keys = this.Shortcut.Select(k => Enum.GetName(typeof(VirtualKeys), k));
			return $"{string.Join("+", keys)}";
		}

	}


	public class KeyboardHooks : IDisposable
	{
		private readonly KeyboardCombinationParser _parser;
		private readonly KeyboardCombination[] _combinations;
		private readonly LowLevelKeyboardProc _hookCallback;

		public KeyboardHooks(KeyboardCombinationParser parser)
		{
			this._parser = parser;
			this._combinations = parser.ToKeyboardCombinations();
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

			var combinationDown = this._combinations.FirstOrDefault(c => c.IsPressed(currentCombination));
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
			combinationDown.Action.Invoke();

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
