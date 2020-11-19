using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Charlotte.Commons;
using Charlotte.Tests;

namespace Charlotte
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcMain.CUIMain(new Program().Main2);
		}

		private void Main2(ArgsReader ar)
		{
			// -- choose one --

			//TestMain(); // テスト
			ProductMain(); // 本番

			// --
		}

		private void TestMain()
		{
			// -- choose one --

			//new Test0001().Test01();
			//new Test0001().Test02();
			//new Test0001().Test03();
			new Test0001().Test01();

			// --

			Console.WriteLine("Press ENTER key.");
			Console.ReadLine();
		}

		private void ProductMain()
		{
			if (!Directory.Exists(Consts.REPOSITORIES_ROOT_DIR))
				throw new Exception("リポジトリのルート・ディレクトリが見つかりません。");

			foreach (string dir in Directory.GetDirectories(Consts.REPOSITORIES_ROOT_DIR))
				Solve(dir);
		}

		private void Solve(string dir)
		{
			SolveForVS2019(dir);
			SolveForFactory(dir);
			SolveGameResource(dir);
			SolveNonAsciiCharactersPaths(dir);
			SolveEmptyFolders(dir);
		}

		private void SolveForVS2019(string dir)
		{
			foreach (string file in Common.GetRepositoryFiles(dir))
			{
				if (SCommon.ContainsIgnoreCase(file, "\\packages\\"))
				{
					SFVS2019_Mask(file);
				}
			}
		}

		private void SFVS2019_Mask(string file)
		{
			const string FILE_SUFFIX = "_ghrs-vs2019-ignore.txt";
			const string MASKED_TEXT = "//// ghrs-vs2019-ignore ////";

			if (SCommon.EndsWithIgnoreCase(file, FILE_SUFFIX)) // ? マスク済み
				return;

			SCommon.DeletePath(file);
			File.WriteAllText(file + FILE_SUFFIX, MASKED_TEXT);
		}

		private void SolveNonAsciiCharactersPaths(string dir)
		{
			foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(subDir), ".git"))
					continue;

				SolveNonAsciiCharactersPaths(subDir);
				SNACP_Path(subDir);
			}
			foreach (string file in Directory.GetFiles(dir))
			{
				SNACP_Path(file);
			}
		}

		private void SNACP_Path(string path)
		{
			string localPath = Path.GetFileName(path);
			string localPathNew = ToAsciiCharactersLocalPath(localPath);

			if (!SCommon.EqualsIgnoreCase(localPath, localPathNew))
			{
				string pathNew = Path.Combine(Path.GetDirectoryName(path), localPathNew);

				if (File.Exists(pathNew) || Directory.Exists(pathNew))
					throw new Exception("変更後のパス名は既に存在します。");

				if (File.Exists(path))
					File.Move(path, pathNew);
				else
					Directory.Move(path, pathNew);
			}
		}

		private string ToAsciiCharactersLocalPath(string localPath)
		{
			StringBuilder buff = new StringBuilder();

			foreach (char chr in localPath)
			{
				if (chr < 0x100)
					buff.Append(chr);
				else
					buff.Append(((ushort)chr).ToString("x4"));
			}
			return buff.ToString();
		}

		private void SolveEmptyFolders(string dir)
		{
			if (
				Directory.GetDirectories(dir).Length == 0 &&
				Directory.GetFiles(dir).Length == 0
				)
			{
				string outFile = Path.Combine(dir, "____EMPTY____");

				File.WriteAllBytes(outFile, SCommon.EMPTY_BYTES);

				return;
			}

			foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(subDir), ".git"))
					continue;

				SolveEmptyFolders(subDir);
			}
		}

		private void SolveForFactory(string dir)
		{
			foreach (string file in Common.GetRepositoryFiles(dir))
			{
				string lwrExt = Path.GetExtension(file).ToLower();

				if (
					lwrExt == ".exe" ||
					lwrExt == ".obj"
					)
					SCommon.DeletePath(file);
			}
			foreach (string file in Common.GetRepositoryFiles(dir))
			{
				if (
					SCommon.ContainsIgnoreCase(file, "\\tmp\\") ||
					SCommon.ContainsIgnoreCase(file, "\\tmp_")
					)
					SCommon.DeletePath(file);
			}
		}

		private void SolveGameResource(string dir)
		{
			foreach (string file in Common.GetRepositoryFiles(dir))
			{
				if (
					SCommon.ContainsIgnoreCase(file, "\\dat\\") ||
					SCommon.ContainsIgnoreCase(file, "\\res\\") ||
					SCommon.ContainsIgnoreCase(file, ".rum\\files\\") // .rum には dat, res 配下のファイルも収録されている。
					)
					SGR_Mask(file);
			}
		}

		private void SGR_Mask(string file)
		{
			const string FILE_SUFFIX = "_ghrs-secret.txt";
			const string MASKED_TEXT = "//// ghrs-secret ////";

			if (SCommon.EndsWithIgnoreCase(file, FILE_SUFFIX)) // ? マスク済み
				return;

			if (IsLikeASourceFile(file)) // ? ソースファイルっぽい -> 除外
				return;

			SCommon.DeletePath(file);
			File.WriteAllText(file + FILE_SUFFIX, MASKED_TEXT);
		}

		private bool IsLikeASourceFile(string file)
		{
			return
				IsLikeASourceFile_C(file) ||
				IsLikeASourceFile_CS(file);
		}

		public bool IsLikeASourceFile_C(string file)
		{
			bool insideOfComment = false;
			int lineCount = 0;

			foreach (string line in Common.ReadAllLines_SJIS(file))
			{
				if (line != "")
				{
					if (insideOfComment)
					{
						if (line == "*/")
							insideOfComment = false;
					}
					else
					{
						if (line == "/*")
							insideOfComment = true;
						else if (line.StartsWith("#include <"))
							return true;
						else if (line.StartsWith("#include \""))
							return true;
					}
				}
				lineCount++;

				if (1000 <= lineCount) // ? #include までの行が多すぎる。
					break;
			}
			return false;
		}

		private bool IsLikeASourceFile_CS(string file)
		{
			// .cs ファイルの想定開始パターン
			// -- BOM + "using "
			byte[] csStPtn = new byte[] { 0xef, 0xbb, 0xbf, 0x75, 0x73, 0x69, 0x6e, 0x67, 0x20 };

			using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
				foreach (byte bChr in csStPtn)
					if ((int)bChr != reader.ReadByte())
						return false;

			return true;
		}
	}
}
