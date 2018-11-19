using System;
using System.Linq;
using WindowsDesktop;
using Awem.PInvoke;

namespace Awem.CommandLine
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			Console.WriteLine("Initializing engine...");
			using(var wm = new WindowManager(null, EventLoop.Break))
			{
				Console.WriteLine($"Awem started!");
				PrintCleanExitCommad(wm);
				Console.WriteLine($"- Monitors: {wm.Monitors.Count}");
				Console.WriteLine($"- Desktops before: {wm.DesktopManager.CountAtStart}");
				Console.WriteLine($"- Desktops now: {wm.DesktopManager.GetDesktopsLength()}");
				Console.WriteLine($"- Applications: {wm.AllApplications.Count}");
				EventLoop.Run();
			}
			Console.WriteLine($"{VirtualDesktop.GetDesktops().Length} Desktops");
			return 0;
		}

		private static void PrintCleanExitCommad(WindowManager wm)
		{
			const string command = nameof(WindowManagerActions.Die);
			var shortcut = wm.KeyboardHooks.Combinations.FirstOrDefault(c => c.Command == command);
			if (shortcut == null) return;
			var restore = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write($" Press ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write($" {shortcut}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($" to gracefully shutdown");
			Console.ForegroundColor = restore;
		}
	}
}
