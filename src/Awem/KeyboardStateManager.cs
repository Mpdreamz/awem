﻿using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem
{
	public static class KeyboardStateManager
	{
		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(ushort vKey);

		[DllImport("user32.dll")]
		private static extern short GetKeyState(ushort vKey);

		private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
		private const int KEYEVENTF_KEYUP = 0x0002;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		public static bool KeyIsDown(VirtualKeys key) => (GetKeyState((ushort)key) & 0x8000) != 0;
		
		public static bool MenuKeyIsDown => KeyIsDown(VirtualKeys.Menu);

		public static void SimulateKeyDown(VirtualKeys key) => keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);

		public static void SimulateKeyUp(VirtualKeys key) => keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
	}
}