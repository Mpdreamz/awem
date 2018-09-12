using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem.PInvoke.Structs
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct MonitorInfo
	{
		public int Size;
		private Rect Monitor;
		private Rect WorkArea;
		private uint Flags;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DeviceName;
	}
}
