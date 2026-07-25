using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの監理 -> 参照 -> Microsoft.Office.Interop.Excel 16.0.18925.20022

	public static class ExcelInteropTools
	{
		private static void ExecuteWorkbook(string excelFile, Action<Microsoft.Office.Interop.Excel.Application, Microsoft.Office.Interop.Excel.Workbook> routine)
		{
			Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
			try
			{
				app.Visible = false;

				Microsoft.Office.Interop.Excel.Workbooks workbooks = app.Workbooks;
				try
				{
					Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Open(excelFile);
					try
					{
						routine(app, workbook);
					}
					finally
					{
						workbook.Close(false);
						Marshal.ReleaseComObject(workbook);
						workbook = null;
					}
				}
				finally
				{
					//workbooks.Close(); // 不要
					Marshal.ReleaseComObject(workbooks);
					workbooks = null;
				}
			}
			finally
			{
				app.Quit();
				Marshal.ReleaseComObject(app);
				app = null;

				// 1
				GC.Collect();
				GC.WaitForPendingFinalizers();

				// 2
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public static void ToPDF(string excelFile, string pdfFile)
		{
			ExecuteWorkbook(excelFile, (app, workbook) =>
			{
				workbook.ExportAsFixedFormat(
					Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF,
					pdfFile,
					Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard,
					IncludeDocProperties: true,
					IgnorePrintAreas: false,
					OpenAfterPublish: false
					);
			});
		}

		public static string[] GetPrinterNames()
		{
			string[] printerNames = new string[PrinterSettings.InstalledPrinters.Count];

			PrinterSettings.InstalledPrinters.CopyTo(printerNames, 0);

			return printerNames;
		}

		public static void Print(string excelFile, string optionalPrinterName = null)
		{
			ExecuteWorkbook(excelFile, (app, workbook) =>
			{
				workbook.PrintOut(
					1,
					9999, // 終了ページ、全ページを印刷させるため十分大きな値にしておく
					1,
					false,
					optionalPrinterName ?? Type.Missing
					);

				Thread.Sleep(5000); // 印刷キュー投入後すぐにエクセルを閉じると失敗する場合があるので待機
			});
		}
	}
}
