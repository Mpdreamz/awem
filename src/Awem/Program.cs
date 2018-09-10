using System;
using System.Collections.Generic;
using System.Linq;
using WindowsDesktop;

namespace Awem
{
	class Program
	{
		static void Main(string[] args)
		{
			var applicationWindows = ApplicationWindows.VisibleOnCurrentDesktop();

			foreach (var window in applicationWindows)
			{
				Console.WriteLine($"-> {window.Desktop.Id}\t ({window.ClassName}) {window.WindowTitle}");
			}
		}
	}
}
