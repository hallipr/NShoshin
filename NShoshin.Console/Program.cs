using System;

namespace NShoshin.Console
{
	public static class Program
	{
		[STAThread]
		public static void Main()
		{
			var runner = new WatinRunner();
			runner.Solve("http://www.websudoku.com/?level=4");
		}
	}
}
