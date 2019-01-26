using System;
using System.Drawing;

namespace Awem.Windowing
{
	public struct TaskBarHandle : IEquatable<TaskBarHandle>
	{
		public IntPtr Handle { get; }

		public bool Primary { get; }

		public TaskBarHandle(IntPtr handle, bool primary) => (this.Handle, this.Primary) = (handle, primary);

		public override bool Equals(object obj) => (obj is TaskBarHandle m) && this.Equals(m);

		public bool Equals(TaskBarHandle other) => (this.Handle, this.Primary) == (other.Handle, other.Primary);

		public override int GetHashCode() => (this.Handle, this.Primary).GetHashCode();

		public static bool operator ==(TaskBarHandle left, TaskBarHandle right) => left.Equals(right);

		public static bool operator !=(TaskBarHandle left, TaskBarHandle right) => !left.Equals(right);

		public static implicit operator IntPtr(TaskBarHandle handle) => handle.Handle;
	}
}
