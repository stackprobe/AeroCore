using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using HLTStudio.Commons;
using HLTStudio.Tools;

namespace HLTStudio.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			Test01_a(
				@"C:\temp\Format.txt",
				@"C:\temp\Parameters.csv",
				@"C:\temp\Prompts"
				);
		}

		private void Test01_a(string formatTextFile, string parametersCsvFile, string outputDir)
		{
			SCommon.DeleteAndCreateDir(outputDir);

			int promptCount;

			Test01_a1(formatTextFile, parametersCsvFile, outputDir, out promptCount);
			Test01_a2(promptCount, outputDir);
		}

		private void Test01_a1(string formatTextFile, string parametersCsvFile, string outputDir, out int promptCount)
		{
			string format = File.ReadAllText(formatTextFile, SCommon.ENCODING_SJIS);
			string[][] parameters = CsvFileReader.ReadToEnd(parametersCsvFile);

			parameters = ParametersFilter(parameters);

			string[] texts = GetTexts(format, parameters);

			int index = 0;
			foreach (string text in texts)
				File.WriteAllText(Path.Combine(outputDir, $"Prompt_{++index:D2}.txt"), text, SCommon.ENCODING_SJIS);

			promptCount = index;
		}

		private string[][] ParametersFilter(string[][] parameters)
		{
			return parameters
				.Select(p => p.Select(c => c.Trim()).ToArray())
				.Where(p => p.Any(c => c != ""))
				.ToArray();
		}

		private string[] GetTexts(string format, string[][] parameters)
		{
			return parameters
				.Select(p => GetText(format, p))
				.ToArray();
		}

		private string GetText(string format, string[] parameter)
		{
			List<string> dest = new List<string>();

			for (; ; )
			{
				string[] encl = SCommon.ParseEnclosed(format, "{{", "}}");

				if (encl == null)
					break;

				int index = int.Parse(encl[2].Trim());
				string p = index <= parameter.Length ? parameter[index - 1] : "";

				dest.Add(encl[0]);
				dest.Add(p);

				format = encl[4];
			}
			dest.Add(format);
			return string.Join("", dest);
		}

		private void Test01_a2(int promptCount, string outputDir)
		{
			List<string> allRunLines = new List<string>();

			for (int index = 0; index < promptCount; index++)
			{
				string batchFile = Path.Combine(outputDir, $"Batch_{index + 1:D2}.bat");
				string promptFile = Path.Combine(outputDir, $"Prompt_{index + 1:D2}.txt");

				File.WriteAllText(
					batchFile,
					$@"

Codex -C ""C:\temp"" exec ^
	--skip-git-repo-check ^
	--sandbox workspace-write ^
	""{promptFile} (SJIS) を読み、その内容を今回の作業指示として実行してください。""

",
					SCommon.ENCODING_SJIS
					);

				allRunLines.Add($@"CALL ""{batchFile}""");
			}

			File.WriteAllLines(
				Path.Combine(outputDir, "runall.bat"),
				allRunLines,
				SCommon.ENCODING_SJIS
				);
		}
	}
}
