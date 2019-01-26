using System;
using System.Drawing;

namespace Awem.Windowing
{
	public struct MonitorHandle : IEquatable<MonitorHandle>
	{
		public IntPtr Handle { get; }

		public Rectangle Rectangle { get; }

		public MonitorHandle(IntPtr handle, Rectangle rectangle) => (this.Handle, this.Rectangle) = (handle, rectangle);

		public override bool Equals(object obj) => (obj is MonitorHandle m) && this.Equals(m);

		public bool Equals(MonitorHandle other) => (this.Handle) == (other.Handle);

		public override int GetHashCode() => (this.Handle).GetHashCode();

		public static bool operator ==(MonitorHandle left, MonitorHandle right) => left.Equals(right);

		public static bool operator !=(MonitorHandle left, MonitorHandle right) => !left.Equals(right);

		public static implicit operator IntPtr(MonitorHandle handle) => handle.Handle;
	}
}