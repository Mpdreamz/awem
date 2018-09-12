using System;
using System.Runtime.InteropServices;

namespace Awem.PInvoke.Enums
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ApplicationBarData
	{
		public uint cbSize;
		public IntPtr hWnd;
		public uint uCallbackMessage;
		public ApplicationBarEdge uEdge;
		public WindowsDesktop.Interop.Rect rc;
		public int lParam;
	}
}
