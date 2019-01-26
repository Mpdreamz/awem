using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Awem.PInvoke;
using Awem.Windowing;

namespace Awem
{
	public class WindowManagerActions
	{
		public Action<int> MoveToDesktop { get; }
		public Action<int> GotoDesktop { get; }
		public Action GotoPreviousDesktop { get; }
		public Action CreateDesktop { get; }
		public Action RemoveDesktop { get; }
		public Action Die { get; }
		public Action ToggleLauncher { get; }

		public IDictionary<string, Action> Commands { get;  }

		public IDictionary<string, Action<int>> NumericCommands { get;  }

		public WindowManagerActions(WindowManager manager, Action toggleLauncherUi, Action exit)
		{
			this.Commands = new Dictionary<string, Action>(StringComparer.CurrentCultureIgnoreCase);
			this.NumericCommands = new Dictionary<string, Action<int>>(StringComparer.CurrentCultureIgnoreCase);

			this.GotoDesktop = manager.DesktopManager.GotoDesktop;
			this.MoveToDesktop = i => manager.DesktopManager.MoveToDesktop(i, ApplicationWindows.Current);
			this.GotoPreviousDesktop = manager.DesktopManager.GotoPreviousDesktop;
			this.CreateDesktop = manager.DesktopManager.CreateDesktop;
			this.RemoveDesktop = manager.DesktopManager.RemoveDesktop;
			this.ToggleLauncher = toggleLauncherUi ?? (() => { });
			this.Die = exit ?? (() => { });

			var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var prop in props)
			{
				if (prop.PropertyType == typeof(Action<int>))
				{
					var v = prop.GetMethod.Invoke(this, null) as Action<int>;
					this.NumericCommands.Add(prop.Name, v);
				}
				else if (prop.PropertyType == typeof(Action))
				{
					var v = prop.GetMethod.Invoke(this, null) as Action;
					this.Commands.Add(prop.Name, v);
				}
			}
		}

	}
}
