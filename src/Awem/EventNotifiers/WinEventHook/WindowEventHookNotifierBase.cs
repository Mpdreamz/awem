using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Awem
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	public abstract class WindowEventHookNotifierBase : IDisposable
	{
		private IntPtr _windowEventHook;
		private WinEventProc _hookCallback;

		protected delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetWinEventHook(
			int eventMin, int eventMax, IntPtr callbackProcess, WinEventProc callback, int idProcess, int idThread, int flags
		);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern int UnhookWinEvent(IntPtr hWinEventHook);

		private const int WINEVENT_OUTOFCONTEXT = 0;
		private const int WINEVENT_SKIPOWNPROCESS = 2;

		protected void CreateWinEventHook(int listenEvent, WinEventProc callback)
		{
			const int flags = WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS;
			//To prevent garbage collection on children before this base class UnHooks
			this._hookCallback = callback;
			this._windowEventHook = SetWinEventHook(listenEvent, listenEvent, IntPtr.Zero, this._hookCallback, 0, 0, flags);

			if (this._windowEventHook == IntPtr.Zero)
				throw new Exception(Marshal.GetLastWin32Error().ToString());
		}
		public virtual void Dispose()
		{
			if (this._windowEventHook == IntPtr.Zero) return;
			UnhookWinEvent(this._windowEventHook);
		}

	}
}
