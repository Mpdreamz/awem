using System;

namespace Awem
{
	class Program
	{
		static void Main(string[] args)
		{
			var applicationWindows = ApplicationWindows.VisibleOnCurrentDesktop();
			using (new WindowForegroundChangedNotifier(i => Console.WriteLine("Foregound: {0}", i)))
			using (new DesktopSwitchNotifier(i => Console.WriteLine("Desktop {0}", i)))
			{
				EventLoop.Run();
			}
		}
	}
}
