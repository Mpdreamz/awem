// ReSharper disable UnusedMember.Global
// ReSharper disable file InconsistentNaming

using System.Drawing;
using System.Runtime.InteropServices;

namespace Awem.PInvoke.Enums
{
	internal struct WindowInfo
	{
		public uint cbSize;
		public Rect rcWindow;
		public Rect rcClient;
		public uint dwStyle;
		public WindowStylesEx dwExStyle;
		public uint dwWindowStatus;
		public uint cxWindowBorders;
		public uint cyWindowBorders;
		public ushort atomWindowType;
		public ushort wCreatorVersion;

		public WindowInfo(bool? filler) : this() => cbSize = (uint) (Marshal.SizeOf(typeof(WindowInfo)));
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Rect
	{
		public readonly int Left; // x position of upper-left corner
		public readonly int Top; // y position of upper-left corner
		public readonly int Right; // x position of lower-right corner
		public readonly int Bottom; // y position of lower-right corner

		public int Height => Bottom - Top;
		public int Width => Right - Left;
		public Size Size => new Size(this.Width, this.Height);
		public Rectangle Rectangle => new Rectangle(this.Left, this.Top, this.Width, this.Height);

		public Rect(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}
}
