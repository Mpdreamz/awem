using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem.Reactive.TaskBars
{
	public class PrimaryTaskBar : TaskBarBase
	{
		public PrimaryTaskBar(TaskBarHandle taskBarHandle) : base(taskBarHandle) => this.Refresh();

		public override void Refresh()
		{
			var data = new ApplicationBarData
			{
				cbSize = (uint) Marshal.SizeOf(typeof(ApplicationBarData)),
				hWnd = this.TaskBarHandle
			};
			var result = TaskBarWin32.SHAppBarMessage(ApplicationBarCommand.GetTaskbarPos, ref data);
			if (result == IntPtr.Zero) throw new InvalidOperationException();

			this.Position = (TaskBarPosition)data.uEdge;
			this.Bounds = Rectangle.FromLTRB(data.rc.Left, data.rc.Top, data.rc.Right, data.rc.Bottom);

			data.cbSize = (uint)Marshal.SizeOf(typeof(ApplicationBarData));
			result = TaskBarWin32.SHAppBarMessage(ApplicationBarCommand.GetState, ref data);
			var state = result.ToInt32();
			this.AlwaysOnTop = (state & ApplicationBarState.AlwaysOnTop) == ApplicationBarState.AlwaysOnTop;
			this.AutoHide = (state & ApplicationBarState.Autohide) == ApplicationBarState.Autohide;
		}
	}
}
