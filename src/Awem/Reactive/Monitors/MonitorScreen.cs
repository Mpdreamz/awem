using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Awem.PInvoke.Structs;
using Awem.Reactive.TaskBars;

namespace Awem.Reactive.Monitors
{
	public class MonitorScreen
	{
		private readonly IReadOnlyCollection<TaskBarBase> _taskBars;
		public MonitorHandle MonitorHandle { get; }
		public Rectangle ScaledResolution { get; internal set; }
		public MonitorOrientation Orientation { get; private set; }
		public Rectangle Resolution { get; private set; }
		public string DisplayName { get; private set; }

		public Rectangle Workspace
		{
			get
			{
				var tb = this.TaskBar;
				if (tb.AutoHide) return this.Resolution;
				var s = this.Resolution;
				var t = this.TaskBar.Bounds;
				int x = s.X, y = s.Y, w = s.Width, h = s.Height;
				switch (tb.CalculatePosition(this.Resolution))
				{
					case TaskBarPosition.Bottom:
						h -= t.Height;
						break;
					case TaskBarPosition.Top:
						h -= t.Height;
						y += t.Height;
						break;
					case TaskBarPosition.Left:
						w -= t.Width;
						x += t.Width;
						break;
					case TaskBarPosition.Right:
						w -= t.Width;
						break;
				}
				return new Rectangle(x, y, w, h);
			}
		}

		public TaskBarBase TaskBar => _taskBars.FirstOrDefault(t => this.Resolution.Contains(t.Bounds));

		// https://docs.microsoft.com/en-gb/windows/desktop/api/wingdi/ns-wingdi-_devicemodea
		// mentions the primary monitor always starts at location 0,0 as all other monitors extend from it
		public bool IsPrimary => this.Resolution.X == 0 && this.Resolution.Y == 0;

		public MonitorScreen(MonitorHandle monitorHandle, IReadOnlyCollection<TaskBarBase> taskBars)
		{
			_taskBars = taskBars;
			this.MonitorHandle = monitorHandle;
			this.Refresh();
		}

		public void Refresh()
		{
			var mi = new MonitorInfo { Size = Marshal.SizeOf(typeof(MonitorInfo)) };
			if (!MonitorWin32.GetMonitorInfo(this.MonitorHandle, ref mi)) return;
			var deviceInfo = new DevMode { dmSize = (short) Marshal.SizeOf(typeof(DevMode)) };

			if (!MonitorWin32.EnumDisplaySettings(mi.DeviceName, MonitorWin32.ENUM_CURRENT_SETTINGS, ref deviceInfo)) return;

			var orientation = (MonitorOrientation) deviceInfo .dmDisplayOrientation;
			var actualResolution = new Rectangle(deviceInfo .dmPositionX, deviceInfo .dmPositionY, deviceInfo .dmPelsWidth, deviceInfo .dmPelsHeight);
			var displayName = deviceInfo.dmDeviceName;

			this.Orientation = orientation;
			this.Resolution = actualResolution;
			this.DisplayName = displayName;
		}


	}
}
