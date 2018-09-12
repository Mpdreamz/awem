using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Awem.PInvoke.Enums;

namespace Awem.Windowing
{
	public abstract class TaskBarBase
	{
		protected TaskBarPosition Position { get; set; }
		public Rectangle Bounds { get; protected set; }
		public bool AlwaysOnTop { get; protected set; }
		public bool AutoHide { get; protected set; }

		public abstract void Refresh();

		public TaskBarPosition CalculatePosition(Rectangle monitor)
		{
			// if its not unknown we already received the position from SHAppBarMessage
			// this does not work for secondary monitors though
			if (this.Position != TaskBarPosition.Unknown) return this.Position;

			var onTheSide = this.Bounds.Width < this.Bounds.Height;
			switch (onTheSide)
			{
				case true when this.Bounds.X > monitor.X: return TaskBarPosition.Right;
				case true when this.Bounds.X == monitor.X: return TaskBarPosition.Left;
				case false when this.Bounds.Y > monitor.Y: return TaskBarPosition.Bottom;
				case false when this.Bounds.Y == monitor.Y: return TaskBarPosition.Top;
			}

			return TaskBarPosition.Unknown;

		}
	}

	public sealed class SecondaryTaskBar : TaskBarBase
	{
		private readonly ApplicationWindow _window;

		public SecondaryTaskBar(ApplicationWindow window, bool alwaysOnTop, bool autoHide)
		{
			_window = window;
			this.AlwaysOnTop = alwaysOnTop;
			this.AutoHide = autoHide;
			this.Refresh();
		}

		public override void Refresh()
		{
			this.Bounds = _window.WindowPlacement;
			this.Position = TaskBarPosition.Unknown;
		}
	}

    public class PrimaryTaskBar : TaskBarBase
    {
	    private readonly IntPtr _taskBarHandle;

        public PrimaryTaskBar(IntPtr taskBarHandle)
        {
	        _taskBarHandle = taskBarHandle;
	        this.Refresh();
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr SHAppBarMessage(ApplicationBarCommand dwMessage, [In] ref ApplicationBarData pData);
	    public override void Refresh()
	    {
	        var data = new ApplicationBarData
	        {
		        cbSize = (uint) Marshal.SizeOf(typeof(ApplicationBarData)),
		        hWnd = this._taskBarHandle
	        };
	        var result = SHAppBarMessage(ApplicationBarCommand.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero) throw new InvalidOperationException();

            this.Position = (TaskBarPosition)data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.Left, data.rc.Top, data.rc.Right, data.rc.Bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(ApplicationBarData));
            result = SHAppBarMessage(ApplicationBarCommand.GetState, ref data);
            var state = result.ToInt32();
            this.AlwaysOnTop = (state & ApplicationBarState.AlwaysOnTop) == ApplicationBarState.AlwaysOnTop;
            this.AutoHide = (state & ApplicationBarState.Autohide) == ApplicationBarState.Autohide;
	    }
    }
}
