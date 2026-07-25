using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: 参照の追加 -> アセンブリ -> System.IO.Compression.FileSystem

	public static class ExcelTools
	{
		public static void View(string xlsxFile, Action<string> routine)
		{
			using (WorkingDir wd = new WorkingDir())
			{
				// 展開先ディレクトリ
				string extractedDir = wd.MakePath();

				// 展開
				SCommon.CreateDir(extractedDir);
				ZipFile.ExtractToDirectory(xlsxFile, extractedDir);

				// 編集
				routine(extractedDir);
			}
		}

		public static void Edit(string xlsxFile, Action<string> routine)
		{
			Edit(xlsxFile, xlsxFile, routine);
		}

		public static void Edit(string inputXlsxFile, string outputXlsxFile, Action<string> routine)
		{
			using (WorkingDir wd = new WorkingDir())
			{
				// 展開先ディレクトリ
				string extractedDir = wd.MakePath();

				// 展開
				SCommon.CreateDir(extractedDir);
				ZipFile.ExtractToDirectory(inputXlsxFile, extractedDir);

				// 編集
				routine(extractedDir);

				// 再圧縮
				SCommon.DeletePath(outputXlsxFile);
				ZipFile.CreateFromDirectory(extractedDir, outputXlsxFile, CompressionLevel.Optimal, false);
			}
		}

		private static string[] PICTURE_EXTS = new string[]
		{
			".png",
			".jpeg",
			".jpg",
			".bmp",
			".gif",
			".emf",
			".wmf",
			".svg",
			".tif",
			".tiff",
		};

		/// <summary>
		/// エクセル内の画像ファイルを収集する。
		/// </summary>
		/// <param name="xlsxFile">エクセルファイル</param>
		/// <param name="routine">画像ファイル・リアクション(ここで画像ファイルを参照・取得すること)</param>
		public static void CollectPicture(string xlsxFile, Action<string> routine)
		{
			View(xlsxFile, dir =>
			{
				foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories).OrderBy(SCommon.CompIgnoreCase))
				{
					if (PICTURE_EXTS.Any(pictureExt => pictureExt.EqualsIgnoreCase(Path.GetExtension(file))))
					{
						routine(file);
					}
				}
			});
		}

		/// <summary>
		/// エクセル内の画像ファイルを置き換える。
		/// </summary>
		/// <param name="xlsxFile">エクセルファイル</param>
		/// <param name="routine">画像ファイル・リアクション(ここで画像ファイルを置き換えること)</param>
		public static void ReplacePicture(string xlsxFile, Action<string> routine)
		{
			ReplacePicture(xlsxFile, xlsxFile, routine);
		}

		/// <summary>
		/// エクセル内の画像ファイルを置き換える。
		/// </summary>
		/// <param name="inputXlsxFile">入力エクセルファイル</param>
		/// <param name="outputXlsxFile">出力エクセルファイル</param>
		/// <param name="routine">画像ファイル・リアクション(ここで画像ファイルを置き換えること)</param>
		public static void ReplacePicture(string inputXlsxFile, string outputXlsxFile, Action<string> routine)
		{
			Edit(inputXlsxFile, outputXlsxFile, dir =>
			{
				foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories).OrderBy(SCommon.CompIgnoreCase))
				{
					if (PICTURE_EXTS.Any(pictureExt => pictureExt.EqualsIgnoreCase(Path.GetExtension(file))))
					{
						routine(file);
					}
				}
			});
		}
	}
}
