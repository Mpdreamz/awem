using System;
using System.Linq;
using WindowsDesktop;
using Awem.PInvoke.Enums;
using Awem.Windowing;
using ReactiveUI;

namespace Awem
{
	public class DesktopManager : ReactiveObject, IDisposable
	{
		public DesktopManager()
		{
			var desktops = VirtualDesktop.GetDesktops();
			this.CountAtStart = desktops.Length;
			this.PreviousDesktop = Array.IndexOf(desktops, VirtualDesktop.Current);
			VirtualDesktop.CurrentChanged += (sender, args) =>
			{
				this.CurrentDesktop = Array.IndexOf(VirtualDesktop.GetDesktops(), args.NewDesktop);
				this.PreviousDesktop = Array.IndexOf(VirtualDesktop.GetDesktops(), args.OldDesktop);
			};
			this.EnsureDesktops(10);
		}
		private int previousDesktop;
		public int PreviousDesktop { get => previousDesktop; set => this.RaiseAndSetIfChanged(ref previousDesktop, value); }
		private int currentDesktop;
		public int CurrentDesktop { get => currentDesktop; set => this.RaiseAndSetIfChanged(ref currentDesktop, value); }

		public int CountAtStart { get; }

		public void GotoPreviousDesktop() => this.GotoDesktop(this.PreviousDesktop);

		public void MoveToDesktop(int desktop, ApplicationWindow window)
		{
			if (window == null) return;
			var desktops = VirtualDesktop.GetDesktops();
			if (desktop >= 0 && desktop < desktops.Length)
				VirtualDesktopHelper.MoveToDesktop(window.WindowHandler,desktops[desktop]);
		}

		public void GotoDesktop(int desktop)
		{
			var desktops = VirtualDesktop.GetDesktops();
			if (desktop >= 0 && desktop < desktops.Length)
				desktops[desktop].Switch();
		}
		public void CreateDesktop()
		{
			VirtualDesktop.Create();
			VirtualDesktop.GetDesktops().Last().Switch();
		}

		public void RemoveDesktop() => VirtualDesktop.Current.Remove();

		private void EnsureDesktops(int desired)
		{
			var currentCount = VirtualDesktop.GetDesktops().Length;
			var additional = desired - currentCount;
			if (additional > 0)
				foreach (var i in Enumerable.Range(0, additional))
					VirtualDesktop.Create();

			if (additional < 0)
			{
				var desktops = VirtualDesktop.GetDesktops();
				for (var i = desktops.Length - 1; additional < 0; i--, additional++)
					desktops[i].Remove();
			}
		}


		public void Dispose() => this.EnsureDesktops(this.CountAtStart);
	}
}
