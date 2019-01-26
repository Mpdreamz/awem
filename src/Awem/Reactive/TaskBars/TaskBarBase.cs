using System.Drawing;

namespace Awem.Windowing
{
	public abstract class TaskBarBase
	{
		protected TaskBarBase(TaskBarHandle handle) => TaskBarHandle = handle;

		public TaskBarHandle TaskBarHandle { get; }

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
}
