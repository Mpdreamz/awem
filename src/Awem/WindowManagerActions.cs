﻿using System;
using System.Collections.Generic;
using System.Reflection;
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

		public IDictionary<string, Action> Commands { get;  }

		public IDictionary<string, Action<int>> NumericCommands { get;  }

		public WindowManagerActions(WindowManager manager)
		{
			this.Commands = new Dictionary<string, Action>(StringComparer.CurrentCultureIgnoreCase);
			this.NumericCommands = new Dictionary<string, Action<int>>(StringComparer.CurrentCultureIgnoreCase);

			this.GotoDesktop = i => manager.DesktopManager.GotoDesktop(i);
			this.MoveToDesktop = i => manager.DesktopManager.MoveToDesktop(i, ApplicationWindows.Current);
			this.GotoPreviousDesktop = () => manager.DesktopManager.GotoPreviousDesktop();
			this.CreateDesktop = () => manager.DesktopManager.CreateDesktop();
			this.RemoveDesktop = () => manager.DesktopManager.RemoveDesktop();
			this.Die = EventLoop.Break;

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
