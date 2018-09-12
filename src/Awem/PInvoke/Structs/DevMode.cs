using System.Runtime.InteropServices;

namespace Awem.PInvoke.Structs
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct DevMode
	{
		private const int CCHDEVICENAME = 0x20;
		private const int CCHFORMNAME = 0x20;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
		public readonly string dmDeviceName;
		public readonly short dmSpecVersion;
		public readonly short dmDriverVersion;
		public short dmSize;
		public readonly short dmDriverExtra;
		public readonly int dmFields;
		public readonly int dmPositionX;
		public readonly int dmPositionY;
		public readonly int dmDisplayOrientation;
		public readonly int dmDisplayFixedOutput;
		public readonly short dmColor;
		public readonly short dmDuplex;
		public readonly short dmYResolution;
		public readonly short dmTTOption;
		public readonly short dmCollate;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
		public readonly string dmFormName;
		public readonly short dmLogPixels;
		public readonly int dmBitsPerPel;
		public readonly int dmPelsWidth;
		public readonly int dmPelsHeight;
		public readonly int dmDisplayFlags;
		public readonly int dmDisplayFrequency;
		public readonly int dmICMMethod;
		public readonly int dmICMIntent;
		public readonly int dmMediaType;
		public readonly int dmDitherType;
		public readonly int dmReserved1;
		public readonly int dmReserved2;
		public readonly int dmPanningWidth;
		public readonly int dmPanningHeight;
	}
}
