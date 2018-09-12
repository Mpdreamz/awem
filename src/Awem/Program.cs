using Awem.PInvoke;

namespace Awem
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			using(var wm = new WindowManager())
			{
				EventLoop.Run();
			}
			return 0;
		}
	}
}
