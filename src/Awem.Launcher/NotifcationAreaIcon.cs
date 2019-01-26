using System;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Windows.Forms;
using Awem;

namespace Tabalt
{
	public class NotifcationAreaIcon : IDisposable
	{
		private readonly NotifyIcon _notificationIcon;
		private readonly ContextMenu _notificationMenu;

		public NotifcationAreaIcon(WindowManagerActions actions)
		{
			this._notificationMenu = this.CreateContextMenu(actions.Die);
			this._notificationIcon = this.CreateIcon(actions.ToggleLauncher);
		}

		private ContextMenu CreateContextMenu(Action shutdown)
		{
			var exitMenuItem = new MenuItem("&Quit", (s, e) => shutdown());
			var menu = new ContextMenu();
			menu.MenuItems.Add(exitMenuItem);
			return menu;
		}

		private NotifyIcon CreateIcon(Action click)
		{
			var x = Assembly.GetExecutingAssembly().Location;
			var ico = Path.Combine(Path.GetDirectoryName(x), "logo.ico");
			var icon = new NotifyIcon
			{
				Text = "tabalt - An alternative ALT TAB implementation",
				Icon = new Icon(ico),
				ContextMenu = this._notificationMenu,
				Visible = true
			};
			icon.Click += (e,s) => click();
			return icon;
		}

		public void Dispose()
		{
			this._notificationIcon.Visible = false;
			this._notificationIcon.Dispose();
			this._notificationMenu.Dispose();
		}
	}
}
