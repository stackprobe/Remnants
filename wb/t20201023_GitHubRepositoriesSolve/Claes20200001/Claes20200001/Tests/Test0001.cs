using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			Test01_a(@"C:\Factory\Tools\7z.c");
			Test01_a(@"C:\Factory\Tools\AutoPwOff.bat");
			Test01_a(@"C:\Factory\Tools\Backup.bat");
			Test01_a(@"C:\Factory\Tools\Backup_Main.c");
		}

		private void Test01_a(string file)
		{
			Console.WriteLine(new Program().IsLikeASourceFile_C(file) ? "IS-C" : "IS-NOT-C");
		}
	}
}
