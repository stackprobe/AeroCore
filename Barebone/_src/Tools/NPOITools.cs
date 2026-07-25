using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの監理 -> 参照 -> NPOI 2.7.5

	public static class NPOITools
	{
		public class Sheet
		{
			public string Name;
			public string[][] Rows;
		}

		public static Sheet[] LoadSheets(string xlsxFile)
		{
			ProcMain.WriteLog("NPOITools.LoadSheets-ST");

			if (string.IsNullOrEmpty(xlsxFile))
				throw new Exception("Bad xlsxFile");

			if (!File.Exists(xlsxFile))
				throw new Exception("no excelFile");

			using (FileStream reader = new FileStream(xlsxFile, FileMode.Open, FileAccess.Read))
			using (IWorkbook workbook = new XSSFWorkbook(reader))
			{
				Sheet[] sheets = new Sheet[workbook.NumberOfSheets];

				for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
					sheets[sheetIndex] = LS_LoadSheet(workbook, sheetIndex);

				ProcMain.WriteLog("NPOITools.LoadSheets-ED");

				return sheets;
			}
		}

		private static Sheet LS_LoadSheet(IWorkbook workbook, int sheetIndex)
		{
			ISheet sheet = workbook.GetSheetAt(sheetIndex);

			if (sheet == null)
			{
				return new Sheet()
				{
					Name = "",
					Rows = new string[][] { new string[] { "" } },
				};
			}
			else
			{
				string[][] rows = LS_LoadRows(sheet);

				SCommon.Csv_m.ToRect(rows);

				return new Sheet()
				{
					Name = sheet.SheetName ?? "",
					Rows = rows,
				};
			}
		}

		private static string[][] LS_LoadRows(ISheet sheet)
		{
			if (
				//sheet == null ||
				sheet.FirstRowNum < 0 ||
				sheet.LastRowNum < 0
				)
				return new string[0][];

			int rowCount = sheet.LastRowNum + 1; // .LastRowNum は最終行を指す。
			string[][] destRows = new string[rowCount][];

			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
				destRows[rowIndex] = LS_LoadRow(sheet, rowIndex);

			return destRows;
		}

		private static string[] LS_LoadRow(ISheet sheet, int rowIndex)
		{
			if (rowIndex < sheet.FirstRowNum)
				return SCommon.EMPTY_STRINGS;

			IRow row = sheet.GetRow(rowIndex);

			if (
				row == null ||
				row.FirstCellNum < 0 ||
				row.LastCellNum < 0
				)
				return SCommon.EMPTY_STRINGS;

			int colCount = row.LastCellNum; // .LastCellNum は最終列の次列を指す。
			string[] destRow = new string[colCount];

			for (int colIndex = 0; colIndex < colCount; colIndex++)
				destRow[colIndex] = LS_LoadCell(row, colIndex);

			return destRow;
		}

		private static string LS_LoadCell(IRow row, int colIndex)
		{
			if (colIndex < row.FirstCellNum)
				return string.Empty;

			ICell cell = row.GetCell(colIndex);

			if (cell == null)
				return string.Empty;

			string value = cell.ToString();
			return value;
		}

#if false
		public static void ToPDF(string xlsxFile, string pdfFile)
		{
			throw null; // 実装しない
		}

		public static string[] GetPrinterNames()
		{
			throw null; // 実装しない
		}

		public static void Print(string xlsxFile, string optionalPrinterName = null)
		{
			throw null; // 実装しない
		}
#endif

		public class Placeholder
		{
			public string SourceText;
			public object DestinationValue;
			public Action<ICell> CellReaction;
			public Image DestinationPicture;
			public int DestinationPictureRowSpan;
			public int DestinationPictureColSpan;
			public int DestinationPictureDX1;
			public int DestinationPictureDY1;
			public int DestinationPictureDX2;
			public int DestinationPictureDY2;

			public Placeholder(
				string sourceText,
				object destinationValue
				)
				: this(sourceText, destinationValue, cell => { })
			{ }

			public Placeholder(
				string sourceText,
				object destinationValue,
				Action<ICell> cellReaction
				)
			{
				this.SourceText = sourceText;
				this.DestinationValue = destinationValue;
				this.CellReaction = cellReaction;
				this.DestinationPicture = null;
				this.DestinationPictureRowSpan = default;
				this.DestinationPictureColSpan = default;
				this.DestinationPictureDX1 = default;
				this.DestinationPictureDY1 = default;
				this.DestinationPictureDX2 = default;
				this.DestinationPictureDY2 = default;

				this.Initialize('T');
			}

			public Placeholder(
				string sourceText,
				Image destinationPicture,
				int destinationPictureRowSpan,
				int destinationPictureColSpan,
				int destinationPictureDY1,
				int destinationPictureDX1,
				int destinationPictureDY2,
				int destinationPictureDX2
				)
			{
				this.SourceText = sourceText;
				this.DestinationValue = null;
				this.CellReaction = null;
				this.DestinationPicture = destinationPicture;
				this.DestinationPictureRowSpan = destinationPictureRowSpan;
				this.DestinationPictureColSpan = destinationPictureColSpan;
				this.DestinationPictureDX1 = destinationPictureDX1;
				this.DestinationPictureDY1 = destinationPictureDY1;
				this.DestinationPictureDX2 = destinationPictureDX2;
				this.DestinationPictureDY2 = destinationPictureDY2;

				this.Initialize('P');
			}

			private void Initialize(char mode)
			{
				if (this.SourceText == null)
					throw new Exception("Bad SourceText(null)");

				this.SourceText = SCommon.ToJString(this.SourceText, true, false, false, false);

				if (this.SourceText == "")
					throw new Exception("Bad SourceText(空文字列)");

				if (mode == 'T')
				{
					switch (this.DestinationValue)
					{
						case string destinationStringValue:
							this.DestinationValue = SCommon.ToJString(destinationStringValue, true, true, false, true).Trim();
							break;

						case double destinationDoubleValue:
							break;

						case DateTime destinationDateTimeValue:
							break;

						case bool destinationBoolValue:
							break;

						case null:
							throw new Exception("Bad DestinationValue(null)");

						default:
							throw new Exception("Bad DestinationValue(Bad type)");
					}

					if (this.CellReaction == null)
						throw new Exception("no CellReaction");
				}
				else // 'P'
				{
					if (this.DestinationPicture == null)
						throw new Exception("Bad DestinationPicture");

					if (!SCommon.IsRange(this.DestinationPictureRowSpan, 1, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureRowSpan");

					if (!SCommon.IsRange(this.DestinationPictureColSpan, 1, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureColSpan");

					if (!SCommon.IsRange(this.DestinationPictureDX1, 0, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureDX1");

					if (!SCommon.IsRange(this.DestinationPictureDY1, 0, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureDY1");

					if (!SCommon.IsRange(this.DestinationPictureDX2, 0, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureDX2");

					if (!SCommon.IsRange(this.DestinationPictureDY2, 0, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureDY2");
				}
			}
		}

		public static void ReplacePlaceholder(string templateXlsxFile, string destinationXlsxFile, Placeholder[] placeholders)
		{
			ProcMain.WriteLog("NPOITools.ReplacePlaceholder-ST");

			if (string.IsNullOrEmpty(templateXlsxFile))
				throw new Exception("Bad templateXlsxFile");

			if (!File.Exists(templateXlsxFile))
				throw new Exception("no templateXlsxFile");

			if (new FileInfo(templateXlsxFile).Length == 0L)
				throw new Exception("templateXlsxFile is empty");

			if (string.IsNullOrEmpty(destinationXlsxFile))
				throw new Exception("Bad destinationXlsxFile");

			if (SCommon.IsExistsPath(destinationXlsxFile))
				throw new Exception("destinationXlsxFile already exists");

			if (
				placeholders == null ||
				placeholders.Any(placeholder => placeholder == null)
				)
				throw new Exception("Bad placeholders");

			// placeholders[].*

			string templateXlsxExt = Path.GetExtension(templateXlsxFile).ToLower();

			if (
				templateXlsxExt != ".xlsx" &&
				templateXlsxExt != ".xlsm"
				)
				throw new Exception("Bad templateXlsxExt");

			if (!templateXlsxExt.EqualsIgnoreCase(Path.GetExtension(destinationXlsxFile)))
				throw new Exception("Bad destinationXlsxFile's extension");

			using (FileStream reader = new FileStream(templateXlsxFile, FileMode.Open, FileAccess.Read))
			using (IWorkbook workbook = new XSSFWorkbook(reader))
			{
				for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
				{
					ISheet sheet = workbook.GetSheetAt(sheetIndex);

					if (
						sheet == null ||
						sheet.FirstRowNum < 0 ||
						sheet.LastRowNum < 0
						)
						continue;

					for (int rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum; rowIndex++) // sheet.LastRowNum は最終行を指す。
					{
						IRow row = sheet.GetRow(rowIndex);

						if (
							row == null ||
							row.FirstCellNum < 0 ||
							row.LastCellNum < 0
							)
							continue;

						for (int colIndex = row.FirstCellNum; colIndex < row.LastCellNum; colIndex++) // row.LastCellNum は最終列の次列を指す。
						{
							ICell cell = row.GetCell(colIndex);

							if (cell == null)
								continue;

							string value = cell.StringCellValue;

							if (string.IsNullOrEmpty(value))
								continue;

							Placeholder placeholder = placeholders.FirstOrDefault(p => p.SourceText == value);

							if (placeholder == null)
								continue;

							if (placeholder.DestinationValue != null) // ? 出力値有り -> テキスト
							{
								switch (placeholder.DestinationValue)
								{
									case string destinationStringValue:
										cell.SetCellValue(destinationStringValue);
										break;

									case double destinationDoubleValue:
										cell.SetCellValue(destinationDoubleValue);
										break;

									case DateTime destinationDateTimeValue:
										cell.SetCellValue(destinationDateTimeValue);
										break;

									case bool destinationBoolValue:
										cell.SetCellValue(destinationBoolValue);
										break;

									default:
										throw null; // never
								}

								if (placeholder.CellReaction != null)
									placeholder.CellReaction(cell);
							}
							else // ? 出力値無し -> 画像
							{
								byte[] pictureBytes;

								using (var mem = new MemoryStream())
								{
									placeholder.DestinationPicture.Save(mem, ImageFormat.Png);
									pictureBytes = mem.ToArray();
								}

								IDrawing<IShape> drawing = sheet.CreateDrawingPatriarch();

								int pictureIndex = workbook.AddPicture(pictureBytes, PictureType.PNG);

								IClientAnchor anchor = workbook.GetCreationHelper().CreateClientAnchor();
								anchor.Row1 = rowIndex;
								anchor.Col1 = colIndex;
								anchor.Row2 = rowIndex + placeholder.DestinationPictureRowSpan;
								anchor.Col2 = colIndex + placeholder.DestinationPictureColSpan;
								anchor.Dx1 = PxToEMU(placeholder.DestinationPictureDX1);
								anchor.Dy1 = PxToEMU(placeholder.DestinationPictureDY1);
								anchor.Dx2 = PxToEMU(placeholder.DestinationPictureDX2 * -1); // 左へ寄せるのは負数！
								anchor.Dy2 = PxToEMU(placeholder.DestinationPictureDY2 * -1); // 上へ寄せるのは負数！

								IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
							}
						}
					}
				}
				ProcMain.WriteLog("NPOITools.ReplacePlaceholder-ED");
			}
		}

		/// <summary>
		/// Pixel to EMU(English Metric Unit)
		/// </summary>
		/// <param name="value">Pixel</param>
		/// <returns>EMU</returns>
		private static int PxToEMU(int value)
		{
			return value * 9525;
		}

		// ====
		// ここから便利ツール
		// ====

		public static ICellStyle GetCopiedCellStyle(ICell cell)
		{
			return GetCopiedCellStyle(cell.CellStyle, cell.Sheet.Workbook);
		}

		public static ICellStyle GetCopiedCellStyle(ICellStyle oldStyle, IWorkbook workbook)
		{
			ICellStyle newStyle = workbook.CreateCellStyle();

			// 文字位置
			newStyle.Alignment = oldStyle.Alignment;
			newStyle.VerticalAlignment = oldStyle.VerticalAlignment;
			newStyle.WrapText = oldStyle.WrapText;
			newStyle.Indention = oldStyle.Indention;
			newStyle.Rotation = oldStyle.Rotation;

			// 罫線(スタイル)
			newStyle.BorderTop = oldStyle.BorderTop;
			newStyle.BorderBottom = oldStyle.BorderBottom;
			newStyle.BorderLeft = oldStyle.BorderLeft;
			newStyle.BorderRight = oldStyle.BorderRight;

			// 罫線(色)
			newStyle.TopBorderColor = oldStyle.TopBorderColor;
			newStyle.BottomBorderColor = oldStyle.BottomBorderColor;
			newStyle.LeftBorderColor = oldStyle.LeftBorderColor;
			newStyle.RightBorderColor = oldStyle.RightBorderColor;

			// 背景・塗りつぶし
			newStyle.FillForegroundColor = oldStyle.FillForegroundColor;
			newStyle.FillBackgroundColor = oldStyle.FillBackgroundColor;
			newStyle.FillPattern = oldStyle.FillPattern;

			// データ形式
			newStyle.DataFormat = oldStyle.DataFormat;

			// フォント
			{
				IFont oldFont = workbook.GetFontAt(oldStyle.FontIndex);
				IFont newFont = workbook.CreateFont();

				newFont.FontName = oldFont.FontName;
				newFont.FontHeightInPoints = oldFont.FontHeightInPoints;
				newFont.IsBold = oldFont.IsBold;
				newFont.IsItalic = oldFont.IsItalic;
				newFont.Underline = oldFont.Underline;
				newFont.Color = oldFont.Color;

				newStyle.SetFont(newFont);
			}

			return newStyle;
		}
	}
}
