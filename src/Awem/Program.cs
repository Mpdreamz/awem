using System;
using WindowsDesktop;
using Awem.PInvoke;

namespace Awem
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			using(var wm = new WindowManager())
			{
				Console.WriteLine($"{VirtualDesktop.GetDesktops().Length} Desktops");
				EventLoop.Run();
			}
			Console.WriteLine($"{VirtualDesktop.GetDesktops().Length} Desktops");
			return 0;
		}
	}
}
