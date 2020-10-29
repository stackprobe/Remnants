using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte
{
	public static class Common
	{
		public static string[] GetAllFiles(string dir)
		{
			return E_GetAllFiles(dir).ToArray();
		}

		private static IEnumerable<string> E_GetAllFiles(string dir)
		{
			foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(subDir), ".git"))
					continue;

				foreach (string file in Directory.GetFiles(subDir, "*", SearchOption.AllDirectories))
					yield return file;
			}
			foreach (string file in Directory.GetFiles(dir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(file), ".gitattributes"))
					continue;

				yield return file;
			}
		}
	}
}
