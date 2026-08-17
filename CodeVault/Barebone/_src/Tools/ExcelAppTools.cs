using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	public static class ExcelAppTools
	{
		#region EXCEL_TO_CSV_SCRIPT

		private static string EXCEL_TO_CSV_SCRIPT = @"

$inputExcelFile = ""<INPUT-EXCEL-FILE>""
$outputFolder   = ""<OUTPUT-FOLDER>""
$errorLogPath   = ""<ERROR-LOG-PATH>""
$successfulFile = ""<SUCCESSFUL-FILE>""

try {
	$excel = New-Object -ComObject Excel.Application
}
catch {
	""エクセルがインストールされていないか、使用できません。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
	$workbook = $excel.Workbooks.Open($inputExcelFile, 0, $true)
}
catch {
	try {
		$excel.Quit() | Out-Null
	}
	catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	""指定されたエクセルファイルは破損しているか、対応していない形式です。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

try {
	$sheetNames = @()

	for ($i = 1; $i -le $workbook.Sheets.Count; $i++) {
		$sheet = $workbook.Sheets.Item($i)
		try {
			$sheetName = $sheet.Name
			$sheetNames += $sheetName

			$csvFileName = ""{0:D4}.csv"" -f $i
			$csvPath = Join-Path $outputFolder $csvFileName

			$xlCSV = 62
			$sheet.SaveAs($csvPath, $xlCSV)
		}
		finally {
			[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($sheet)
			$sheet = $null
		}
	}

	$sheetListPath = Join-Path $outputFolder ""sheet-names.txt""
	$sheetNames | Out-File -Encoding UTF8 $sheetListPath

	New-Item -ItemType File -Path $successfulFile -Force | Out-Null
}
catch {
	""エクセルファイルの読み込み中に不明なエラーが発生しました。"" | Out-File -Encoding UTF8 -Append $errorLogPath
}
finally {
	try {
		$workbook.Close($false) | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
	$workbook = $null

	try {
		$excel.Quit() | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()
}

";

		#endregion

		#region EXCEL_TO_PDF_SCRIPT

		private static string EXCEL_TO_PDF_SCRIPT = @"

$inputExcelFile = ""<INPUT-EXCEL-FILE>""
$outputPDFFile  = ""<OUTPUT-PDF-FILE>""
$errorLogPath   = ""<ERROR-LOG-PATH>""
$successfulFile = ""<SUCCESSFUL-FILE>""

try {
	$excel = New-Object -ComObject Excel.Application
}
catch {
	""エクセルがインストールされていないか、使用できません。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
	$workbook = $excel.Workbooks.Open($inputExcelFile, 0, $true)
}
catch {
	try {
		$excel.Quit() | Out-Null
	}
	catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	""指定されたエクセルファイルは破損しているか、対応していない形式です。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

try {
	$xlTypePDF = 0
	$xlQualityStandard = 0

	$workbook.ExportAsFixedFormat($xlTypePDF, $outputPDFFile, $xlQualityStandard, $true, $false)

	New-Item -ItemType File -Path $successfulFile -Force | Out-Null
}
catch {
	""エクセルファイルの処理中に不明なエラーが発生しました。"" | Out-File -Encoding UTF8 -Append $errorLogPath
}
finally {
	try {
		$workbook.Close($false) | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
	$workbook = $null

	try {
		$excel.Quit() | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()
}

";

		#endregion

		#region EXCEL_PRINT_SCRIPT

		private static string EXCEL_PRINT_SCRIPT = @"

$inputExcelFile            = ""<INPUT-EXCEL-FILE>""
$optionalPtinerNameEnabled =   <OPTIONAL-PRINTER-NAME-ENABLED>
$optionalPtinerName        = ""<OPTIONAL-PRINTER-NAME>""
$errorLogPath              = ""<ERROR-LOG-PATH>""
$successfulFile            = ""<SUCCESSFUL-FILE>""

try {
	$excel = New-Object -ComObject Excel.Application
}
catch {
	""エクセルがインストールされていないか、使用できません。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
	$workbook = $excel.Workbooks.Open($inputExcelFile, 0, $true)
}
catch {
	try {
		$excel.Quit() | Out-Null
	}
	catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	""指定されたエクセルファイルは破損しているか、対応していない形式です。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

try {
	if ($optionalPtinerNameEnabled) {
		$activePrinter = $optionalPrinterName
	} else {
		$activePrinter = [Type]::Missing
	}

	$workbook.PrintOut(1, 9999, 1, $false, $activePrinter)
	#                     |
	#                     終了ページ、全ページを印刷させるため十分大きな値にしておく

	# 印刷キュー投入後すぐにエクセルを閉じると失敗する場合があるので待機
	Start-Sleep -Seconds 5

	New-Item -ItemType File -Path $successfulFile -Force | Out-Null
}
catch {
	""印刷中にエラーが発生しました。"" | Out-File -Encoding UTF8 -Append $errorLogPath
}
finally {
	try {
		$workbook.Close($false) | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
	$workbook = $null

	try {
		$excel.Quit() | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()
}

";

		#endregion

		#region REPLACE_PLACEHOLDER_SCRIPT

		private static string REPLACE_PLACEHOLDER_SCRIPT = @"

$inputExcelFile  = ""<INPUT-EXCEL-FILE>""
$outputExcelFile = ""<OUTPUT-EXCEL-FILE>""
$errorLogPath    = ""<ERROR-LOG-PATH>""
$successfulFile  = ""<SUCCESSFUL-FILE>""

try {
	$excel = New-Object -ComObject Excel.Application
}
catch {
	""エクセルがインストールされていないか、使用できません。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
	$workbook = $excel.Workbooks.Open($inputExcelFile, 0, $true)
}
catch {
	try {
		$excel.Quit() | Out-Null
	}
	catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	""指定されたエクセルファイルは破損しているか、対応していない形式です。"" | Out-File -Encoding UTF8 -Append $errorLogPath
	exit 1
}

$registeredComs = New-Object System.Collections.ArrayList

function ReleaseRegisteredComs {
	for ($i = $script:registeredComs.Count - 1; 0 -le $i; $i--) {
		$com = $script:registeredComs[$i]
		try {
			if ($com -and [System.Runtime.InteropServices.Marshal]::IsComObject($com)) {
				[void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($com)
			}
		}
		catch {
		}
		$com = $null
	}
	$script:registeredComs.Clear()

	# 1
	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	# 2
	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()
}

function RPText {
	# script scope -> local scope
	$excel = $script:excel
	$worksheet = $script:worksheet
	$sheet = $script:sheet
	$sourceText = $script:sourceText
	$destinationText = $script:destinationText
	$optionalFontNameEnabled      = $script:optionalFontNameEnabled
	$optionalFontName             = $script:optionalFontName
	$optionalFontSizeEnabled      = $script:optionalFontSizeEnabled
	$optionalFontSize             = $script:optionalFontSize
	$optionalFontBoldEnabled      = $script:optionalFontBoldEnabled
	$optionalFontBold             = $script:optionalFontBold
	$optionalFontItalicEnabled    = $script:optionalFontItalicEnabled
	$optionalFontItalic           = $script:optionalFontItalic
	$optionalFontUnderlineEnabled = $script:optionalFontUnderlineEnabled
	$optionalFontUnderline        = $script:optionalFontUnderline
	$optionalFontColorEnabled     = $script:optionalFontColorEnabled
	$optionalFontColor            = $script:optionalFontColor
	$optionalBackColorEnabled     = $script:optionalBackColorEnabled
	$optionalBackColor            = $script:optionalBackColor
	$registeredComs = $script:registeredComs

	try {
		$sourceText = $sourceText -replace '~', '~~' -replace '\*', '~*' -replace '\?', '~?'
#		$registeredComs.Add($sourceText) | Out-Null # 文字列
		$scope = $sheet.UsedRange
		$registeredComs.Add($scope) | Out-Null
		$curr = $scope.Find($sourceText)
		$registeredComs.Add($curr) | Out-Null

		if ($curr) {
			$addr0 = $curr.Address()
			$registeredComs.Add($addr0) | Out-Null

			do {
				$curr.Value2 = $destinationText
#				$registeredComs.Add($destinationText) | Out-Null # 文字列

				if ($optionalFontNameEnabled) {
					$curr.Font.Name = $optionalFontName
				}
				if ($optionalFontSizeEnabled) {
					$curr.Font.Size = $optionalFontSize
				}
				if ($optionalFontBoldEnabled) {
					$curr.Font.Bold = $optionalFontBold
				}
				if ($optionalFontItalicEnabled) {
					$curr.Font.Italic = $optionalFontItalic
				}
				if ($optionalFontUnderlineEnabled) {
					$curr.Font.Underline = $optionalFontUnderline
				}
				if ($optionalFontColorEnabled) {
					$curr.Font.Color = $optionalFontColor
				}
				if ($optionalBackColorEnabled) {
					$curr.Interior.Color = $optionalBackColor
				}
				$curr = $scope.FindNext($curr)
				$registeredComs.Add($curr) | Out-Null
			}
			while ($curr -and $curr.Address() -ne $addr0)
		}
	}
	finally {
		ReleaseRegisteredComs
	}
}

function RPPicture {
	# script scope -> local scope
	$excel = $script:excel
	$worksheet = $script:worksheet
	$sheet = $script:sheet
	$sourceText = $script:sourceText
	$destinationPicture = $script:destinationPicture
	$destinationPictureLeft = $script:destinationPictureLeft
	$destinationPictureTop  = $script:destinationPictureTop
	$registeredComs = $script:registeredComs

	try {
		$sourceText = $sourceText -replace '~', '~~' -replace '\*', '~*' -replace '\?', '~?'
#		$registeredComs.Add($sourceText) | Out-Null # 文字列
		$scope = $sheet.UsedRange
		$registeredComs.Add($scope) | Out-Null
		$curr = $scope.Find($sourceText)
		$registeredComs.Add($curr) | Out-Null

		if ($curr) {
			$addr0 = $curr.Address()
			$registeredComs.Add($addr0) | Out-Null

			do {
				$curr.Value2 = ''

				$linkToFile = $false        # ファイルにリンクするか(True：する　False：しない)
				$saveWithDocument = $true   # ワークブックに埋め込むか(True：埋め込む　False：埋め込まない)
				$width = -1                 # 画像の幅(-1：元画像のサイズを保持)
				$height = -1                # 画像の高さ(-1：元画像のサイズを保持)

				$picture = $sheet.Shapes.AddPicture(
					$destinationPicture,
					$linkToFile,
					$saveWithDocument,
					$curr.Left + $destinationPictureLeft,
					$curr.Top  + $destinationPictureTop,
					$width,
					$height
					)
				$registeredComs.Add($picture) | Out-Null
				$curr = $scope.FindNext($curr)
				$registeredComs.Add($curr) | Out-Null
			}
			while ($curr -and $curr.Address() -ne $addr0)
		}
	}
	finally {
		ReleaseRegisteredComs
	}
}

try {
	foreach ($sheet in @($workbook.Worksheets)) {

<RP-SCRIPT>

	}
	$workbook.SaveAs($outputExcelFile)

	New-Item -ItemType File -Path $successfulFile -Force | Out-Null
}
catch {
	""エクセルファイルの処理中に不明なエラーが発生しました。"" | Out-File -Encoding UTF8 -Append $errorLogPath
}
finally {
	try {
		$workbook.Close($false) | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
	$workbook = $null

	try {
		$excel.Quit() | Out-Null
	} catch {
	}
	[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
	$excel = $null

	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()
}

";

		#endregion

		public class Sheet
		{
			public string Name;
			public string[][] Rows;
		}

		public static Sheet[] LoadSheets(string excelFile)
		{
			ProcMain.WriteLog("ExcelAppTools.LoadSheets-ST");

			if (string.IsNullOrEmpty(excelFile))
				throw new Exception("Bad excelFile");

			if (!File.Exists(excelFile))
				throw new Exception("no excelFile");

			// memo: .csv .txt などで空のファイルは有り得るので、空のファイルをここでエラーにしないこと。

			using (WorkingDir wd = new WorkingDir())
			{
				string inputExcelFile = wd.MakePath() + Path.GetExtension(excelFile);
				string outputDir = wd.MakePath();
				string errorLogFile = wd.MakePath();
				string successfulFile = wd.MakePath();
				string scriptFile = wd.MakePath() + ".ps1";

				File.Copy(excelFile, inputExcelFile);
				SCommon.CreateDir(outputDir);
				File.WriteAllBytes(errorLogFile, SCommon.EMPTY_BYTES);

				File.WriteAllText(
					scriptFile,
					EXCEL_TO_CSV_SCRIPT.ReplaceAll(
						"<INPUT-EXCEL-FILE>", inputExcelFile,
						"<OUTPUT-FOLDER>", outputDir,
						"<ERROR-LOG-PATH>", errorLogFile,
						"<SUCCESSFUL-FILE>", successfulFile
						),
					Encoding.UTF8
					);

				SCommon.Batch(new string[]
				{
					$"PowerShell.exe -STA -ExecutionPolicy Bypass -File \"{scriptFile}\"",
				});

				string errorLog = File.ReadAllText(errorLogFile, Encoding.UTF8).Trim();

				if (errorLog != "")
					throw new Exception(errorLog);

				if (!File.Exists(successfulFile))
					throw new Exception("パワーシェルがクラッシュしたか、起動できませんでした。");

				string[] sheetNames = File.ReadAllLines(Path.Combine(outputDir, "sheet-names.txt"), Encoding.UTF8)
					.Select(line => line.Trim())
					.Where(line => line != "")
					.ToArray();

				List<string[][]> rowsList = new List<string[][]>();

				for (int i = 1; ; i++)
				{
					string csvFile = Path.Combine(outputDir, string.Format("{0:D4}.csv", i));

					if (!File.Exists(csvFile))
						break;

					string[][] rows = CsvFileReader.ReadToEnd(csvFile);
					rows = SCommon.Csv_m.ToRect(rows);
					rowsList.Add(rows);
				}

				if (sheetNames.Length < 1)
					throw new Exception("Bad sheetNames.Length: " + sheetNames.Length);

				if (sheetNames.Length != rowsList.Count)
					throw new Exception("Bad sheetNames.Length: " + sheetNames.Length + ", " + rowsList.Count);

				Sheet[] sheets = Enumerable.Range(0, sheetNames.Length)
					.Select(i => new Sheet()
					{
						Name = sheetNames[i],
						Rows = rowsList[i],
					})
					.ToArray();

				ProcMain.WriteLog("ExcelAppTools.LoadSheets-ED");

				return sheets;
			}
		}

		/// <summary>
		/// PDFファイル出力
		/// 注意：進捗ダイアログが表示されることがある。
		/// </summary>
		/// <param name="excelFile">入力エクセルファイル</param>
		/// <param name="pdfFile">出力PDFファイル</param>
		public static void ToPDF(string excelFile, string pdfFile)
		{
			ProcMain.WriteLog("ExcelAppTools.ToPDF-ST");

			if (string.IsNullOrEmpty(excelFile))
				throw new Exception("Bad excelFile");

			if (!File.Exists(excelFile))
				throw new Exception("no excelFile");

			if (string.IsNullOrEmpty(pdfFile))
				throw new Exception("Bad pdfFile");

			if (SCommon.IsExistsPath(pdfFile))
				throw new Exception("pdfFile already exists");

			// memo: .csv .txt などで空のファイルは有り得るので、空のファイルをここでエラーにしないこと。

			using (WorkingDir wd = new WorkingDir())
			{
				string inputExcelFile = wd.MakePath() + Path.GetExtension(excelFile);
				string outputPDFFile = wd.MakePath() + ".pdf";
				string outputPDFFile2 = wd.MakePath() + ".pdf";
				string errorLogFile = wd.MakePath();
				string successfulFile = wd.MakePath();
				string scriptFile = wd.MakePath() + ".ps1";

				File.Copy(excelFile, inputExcelFile);
				File.WriteAllBytes(errorLogFile, SCommon.EMPTY_BYTES);

				File.WriteAllText(
					scriptFile,
					EXCEL_TO_PDF_SCRIPT.ReplaceAll(
						"<INPUT-EXCEL-FILE>", inputExcelFile,
						"<OUTPUT-PDF-FILE>", outputPDFFile,
						"<ERROR-LOG-PATH>", errorLogFile,
						"<SUCCESSFUL-FILE>", successfulFile
						),
					Encoding.UTF8
					);

				SCommon.Batch(new string[]
				{
					$"PowerShell.exe -STA -ExecutionPolicy Bypass -File \"{scriptFile}\"",
				});

				string errorLog = File.ReadAllText(errorLogFile, Encoding.UTF8).Trim();

				if (errorLog != "")
					throw new Exception(errorLog);

				if (!File.Exists(successfulFile))
					throw new Exception("パワーシェルがクラッシュしたか、起動できませんでした。");

				if (!File.Exists(outputPDFFile))
					throw new Exception("no outputPDFFile");

				SCommon.EnsureMoveFile(outputPDFFile, outputPDFFile2);
				File.Move(outputPDFFile2, pdfFile);

				ProcMain.WriteLog("ExcelAppTools.ToPDF-ED");
			}
		}

		public static string[] GetPrinterNames()
		{
			ProcMain.WriteLog("ExcelAppTools.GetPrinterNames-ST");

			using (WorkingDir wd = new WorkingDir())
			{
				string outputFile = wd.MakePath();
				string scriptFile = wd.MakePath() + ".ps1";

				File.WriteAllText(
					scriptFile,
					$"Get-Printer | Select-Object -ExpandProperty Name | Out-File -FilePath \"{outputFile}\" -Encoding utf8",
					Encoding.UTF8
					);

				SCommon.Batch(new string[]
				{
					$"PowerShell.exe -STA -ExecutionPolicy Bypass -File \"{scriptFile}\"",
				});

				if (!File.Exists(outputFile))
					throw new Exception("パワーシェルがクラッシュしたか、起動できませんでした。(no outputFile)");

				string[] printerNames = File.ReadAllLines(outputFile, Encoding.UTF8)
					.Select(line => line.Trim())
					.Where(line => line != "")
					.ToArray();

				ProcMain.WriteLog("ExcelAppTools.GetPrinterNames-ED");

				return printerNames;
			}
		}

		public static void Print(string excelFile, string optionalPrinterName = null)
		{
			ProcMain.WriteLog("ExcelAppTools.Print-ST");

			if (string.IsNullOrEmpty(excelFile))
				throw new Exception("Bad excelFile");

			// optionalPrinterName

			// memo: .csv .txt などで空のファイルは有り得るので、空のファイルをここでエラーにしないこと。

			using (WorkingDir wd = new WorkingDir())
			{
				string inputExcelFile = wd.MakePath() + Path.GetExtension(excelFile);
				string errorLogFile = wd.MakePath();
				string successfulFile = wd.MakePath();
				string scriptFile = wd.MakePath() + ".ps1";

				File.Copy(excelFile, inputExcelFile);
				File.WriteAllBytes(errorLogFile, SCommon.EMPTY_BYTES);

				File.WriteAllText(
					scriptFile,
					EXCEL_PRINT_SCRIPT.ReplaceAll(
						"<INPUT-EXCEL-FILE>", inputExcelFile,
						"<OPTIONAL-PRINTER-NAME-ENABLED>", string.IsNullOrEmpty(optionalPrinterName) ? "$false" : "$true",
						"<OPTIONAL-PRINTER-NAME>", optionalPrinterName ?? "",
						"<ERROR-LOG-PATH>", errorLogFile,
						"<SUCCESSFUL-FILE>", successfulFile
						),
					Encoding.UTF8
					);

				SCommon.Batch(new string[]
				{
					$"PowerShell.exe -STA -ExecutionPolicy Bypass -File \"{scriptFile}\"",
				});

				string errorLog = File.ReadAllText(errorLogFile, Encoding.UTF8).Trim();

				if (errorLog != "")
					throw new Exception(errorLog);

				if (!File.Exists(successfulFile))
					throw new Exception("パワーシェルがクラッシュしたか、起動できませんでした。");

				ProcMain.WriteLog("ExcelAppTools.Print-ED");
			}
		}

		public class Placeholder
		{
			public string SourceText;
			public string DestinationText;
			public string OptionalFontName;
			public int? OptionalFontSize;
			public bool? OptionalFontBold;
			public bool? OptionalFontItalic;
			public bool? OptionalFontUnderline;
			public Color? OptionalFontColor;
			public Color? OptionalBackColor;
			public Image DestinationPicture;
			public int DestinationPictureLeft;
			public int DestinationPictureTop;

			public Placeholder(
				string sourceText,
				string destinationText,
				string optionalFontName = null,
				int? optionalFontSize = null,
				bool? optionalFontBold = null,
				bool? optionalFontItalic = null,
				bool? optionalFontUnderline = null,
				Color? optionalFontColor = null,
				Color? optionalBackColor = null
				)
			{
				this.SourceText = sourceText;
				this.DestinationText = destinationText;
				this.OptionalFontName = optionalFontName;
				this.OptionalFontSize = optionalFontSize;
				this.OptionalFontBold = optionalFontBold;
				this.OptionalFontItalic = optionalFontItalic;
				this.OptionalFontUnderline = optionalFontUnderline;
				this.OptionalFontColor = optionalFontColor;
				this.OptionalBackColor = optionalBackColor;
				this.DestinationPicture = null;
				this.DestinationPictureLeft = default;
				this.DestinationPictureTop = default;

				this.Initialize('T');
			}

			public Placeholder(
				string sourceText,
				Image destinationPicture,
				int destinationPictureLeft,
				int destinationPictureTop
				)
			{
				this.SourceText = sourceText;
				this.DestinationText = null;
				this.OptionalFontName = null;
				this.OptionalFontSize = null;
				this.OptionalFontBold = null;
				this.OptionalFontItalic = null;
				this.OptionalFontUnderline = null;
				this.OptionalFontColor = null;
				this.OptionalBackColor = null;
				this.DestinationPicture = destinationPicture;
				this.DestinationPictureLeft = destinationPictureLeft;
				this.DestinationPictureTop = destinationPictureTop;

				this.Initialize('P');
			}

			public string Marker;

			private void Initialize(char mode)
			{
				if (this.SourceText == null)
					throw new Exception("Bad SourceText(null)");

				this.SourceText = SCommon.ToJString(this.SourceText, true, false, false, false);

				if (this.SourceText == "")
					throw new Exception("Bad SourceText(空文字列)");

				// 接頭辞・接尾辞チェック
				{
					const string OMAJINAI_SOURCE_TEXT_PREFIX = "**";
					const string OMAJINAI_SOURCE_TEXT_SUFFIX = "**";

					if (this.SourceText.Length < OMAJINAI_SOURCE_TEXT_PREFIX.Length + OMAJINAI_SOURCE_TEXT_SUFFIX.Length)
						throw new Exception("Bad SourceText(接頭辞・接尾辞エラー)");

					if (!this.SourceText.StartsWith(OMAJINAI_SOURCE_TEXT_PREFIX))
						throw new Exception("Bad SourceText(接頭辞エラー)");

					if (!this.SourceText.EndsWith(OMAJINAI_SOURCE_TEXT_SUFFIX))
						throw new Exception("Bad SourceText(接尾辞エラー)");
				}

				if (mode == 'T')
				{
					if (this.DestinationText == null)
						throw new Exception("Bad DestinationText");

					this.DestinationText = SCommon.ToJString(this.DestinationText, true, true, false, true).Trim();

					if (this.OptionalFontName != null)
					{
						this.OptionalFontName = SCommon.ToJString(this.OptionalFontName, true, false, false, true).Trim();

						if (this.OptionalFontName == "")
							throw new Exception("Bad OptionalFontName");

						ProcMain.WriteLog($"OptionalFontName: [{this.OptionalFontName}]");
					}
					if (this.OptionalFontSize != null)
					{
						if (!SCommon.IsRange(this.OptionalFontSize.Value, 1, SCommon.IMAX))
							throw new Exception("Bad OptionalFontSize");
					}
					// this.OptionalFontBold
					// this.OptionalFontItalic
					// this.OptionalFontUnderline
					// this.OptionalFontColor
					// this.OptionalBackColor
				}
				else // 'P'
				{
					if (this.DestinationPicture == null)
						throw new Exception("Bad DestinationPicture");

					if (!SCommon.IsRange(this.DestinationPictureLeft, -SCommon.IMAX, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureLeft");

					if (!SCommon.IsRange(this.DestinationPictureTop, -SCommon.IMAX, SCommon.IMAX))
						throw new Exception("Bad DestinationPictureTop");
				}
				this.Marker = SCommon.GetCUID();
			}
		}

		public static void ReplacePlaceholder(string templateExcelFile, string destinationExcelFile, Placeholder[] placeholders)
		{
			ProcMain.WriteLog("ExcelAppTools.ReplacePlaceholder-ST");

			if (string.IsNullOrEmpty(templateExcelFile))
				throw new Exception("Bad templateExcelFile");

			if (!File.Exists(templateExcelFile))
				throw new Exception("no templateExcelFile");

			if (new FileInfo(templateExcelFile).Length == 0L)
				throw new Exception("templateExcelFile is empty");

			if (string.IsNullOrEmpty(destinationExcelFile))
				throw new Exception("Bad destinationExcelFile");

			if (SCommon.IsExistsPath(destinationExcelFile))
				throw new Exception("destinationExcelFile already exists");

			if (
				placeholders == null ||
				placeholders.Any(placeholder => placeholder == null)
				)
				throw new Exception("Bad placeholders");

			// placeholders[].*

			string templateExcelExt = Path.GetExtension(templateExcelFile).ToLower();

			if (
				templateExcelExt != ".xlsx" &&
				templateExcelExt != ".xlsm"
				)
				throw new Exception("Bad templateExcelExt");

			if (!templateExcelExt.EqualsIgnoreCase(Path.GetExtension(destinationExcelFile)))
				throw new Exception("Bad destinationExcelFile's extension");

			using (WorkingDir wd = new WorkingDir())
			{
				string inputExcelFile = wd.MakePath() + templateExcelExt;
				string outputExcelFile = wd.MakePath() + templateExcelExt;
				string outputExcelFile2 = wd.MakePath() + templateExcelExt;
				string errorLogFile = wd.MakePath();
				string successfulFile = wd.MakePath();
				string scriptFile = wd.MakePath() + ".ps1";

				string rpScript = RP_GetRPScript(placeholders, wd);

				File.Copy(templateExcelFile, inputExcelFile);
				File.WriteAllBytes(errorLogFile, SCommon.EMPTY_BYTES);

				File.WriteAllText(
					scriptFile,
					REPLACE_PLACEHOLDER_SCRIPT.ReplaceAll(
						"<INPUT-EXCEL-FILE>", inputExcelFile,
						"<OUTPUT-EXCEL-FILE>", outputExcelFile,
						"<ERROR-LOG-PATH>", errorLogFile,
						"<SUCCESSFUL-FILE>", successfulFile,
						"<RP-SCRIPT>", rpScript
						),
					Encoding.UTF8
					);

				SCommon.Batch(new string[]
				{
					$"PowerShell.exe -STA -ExecutionPolicy Bypass -File \"{scriptFile}\"",
				});

				string errorLog = File.ReadAllText(errorLogFile, Encoding.UTF8).Trim();

				if (errorLog != "")
					throw new Exception(errorLog);

				if (!File.Exists(successfulFile))
					throw new Exception("パワーシェルがクラッシュしたか、起動できませんでした。");

				if (!File.Exists(outputExcelFile))
					throw new Exception("no outputExcelFile");

				SCommon.EnsureMoveFile(outputExcelFile, outputExcelFile2);
				File.Move(outputExcelFile2, destinationExcelFile);

				ProcMain.WriteLog("ExcelAppTools.ReplacePlaceholder-ED");
			}
		}

		private static string RP_GetRPScript(Placeholder[] placeholders, WorkingDir wd)
		{
			List<string> dest = new List<string>();

			foreach (Placeholder placeholder in placeholders)
			{
				dest.Add($"$sourceText = '{placeholder.SourceText.Replace("'", "''")}'");
				dest.Add($"$destinationText = '{placeholder.Marker}'");

				dest.Add("$optionalFontNameEnabled = $false");
				dest.Add("$optionalFontSizeEnabled = $false");
				dest.Add("$optionalFontBoldEnabled = $false");
				dest.Add("$optionalFontItalicEnabled = $false");
				dest.Add("$optionalFontUnderlineEnabled = $false");
				dest.Add("$optionalFontColorEnabled = $false");
				dest.Add("$optionalBackColorEnabled = $false");

				dest.Add("RPText");
			}
			foreach (Placeholder placeholder in placeholders)
			{
				dest.Add($"$sourceText = '{placeholder.Marker}'");

				if (placeholder.DestinationText != null) // テキスト
				{
					dest.Add($"$destinationText = '{placeholder.DestinationText.Replace("'", "''")}'");

					if (placeholder.OptionalFontName != null)
					{
						dest.Add("$optionalFontNameEnabled = $true");
						dest.Add($"$optionalFontName = '{placeholder.OptionalFontName.Replace("'", "''")}'");
					}
					else
					{
						dest.Add("$optionalFontNameEnabled = $false");
					}

					if (placeholder.OptionalFontSize != null)
					{
						dest.Add("$optionalFontSizeEnabled = $true");
						dest.Add($"$optionalFontSize = {placeholder.OptionalFontSize.Value}");
					}
					else
					{
						dest.Add("$optionalFontSizeEnabled = $false");
					}

					if (placeholder.OptionalFontBold != null)
					{
						dest.Add("$optionalFontBoldEnabled = $true");
						dest.Add($"$optionalFontBold = {(placeholder.OptionalFontBold.Value ? "$true" : "$false")}");
					}
					else
					{
						dest.Add("$optionalFontBoldEnabled = $false");
					}

					if (placeholder.OptionalFontItalic != null)
					{
						dest.Add("$optionalFontItalicEnabled = $true");
						dest.Add($"$optionalFontItalic = {(placeholder.OptionalFontItalic.Value ? "$true" : "$false")}");
					}
					else
					{
						dest.Add("$optionalFontItalicEnabled = $false");
					}

					if (placeholder.OptionalFontUnderline != null)
					{
						const int UNDERLINE_OFF = -4142;
						const int UNDERLINE_ON = 2;

						dest.Add("$optionalFontUnderlineEnabled = $true");
						dest.Add($"$optionalFontUnderline = {(placeholder.OptionalFontUnderline.Value ? UNDERLINE_ON : UNDERLINE_OFF)}");
					}
					else
					{
						dest.Add("$optionalFontUnderlineEnabled = $false");
					}

					if (placeholder.OptionalFontColor != null)
					{
						dest.Add("$optionalFontColorEnabled = $true");
						dest.Add($"$optionalFontColor = {ToPS1Code(placeholder.OptionalFontColor.Value)}");
					}
					else
					{
						dest.Add("$optionalFontColorEnabled = $false");
					}

					if (placeholder.OptionalBackColor != null)
					{
						dest.Add("$optionalBackColorEnabled = $true");
						dest.Add($"$optionalBackColor = {ToPS1Code(placeholder.OptionalBackColor.Value)}");
					}
					else
					{
						dest.Add("$optionalBackColorEnabled = $false");
					}

					dest.Add("RPText");
				}
				else // 画像
				{
					string imageFile = wd.MakePath() + ".png";

					placeholder.DestinationPicture.Save(imageFile, ImageFormat.Png);

					dest.Add($"$destinationPicture = '{imageFile}'");
					dest.Add($"$destinationPictureLeft = '{placeholder.DestinationPictureLeft}'");
					dest.Add($"$destinationPictureTop = '{placeholder.DestinationPictureTop}'");
					dest.Add("RPPicture");
				}
			}
			return SCommon.LinesToText(dest);
		}

		private static string ToPS1Code(Color color)
		{
			return $"0x{color.B:x2}{color.G:x2}{color.R:x2}";
		}
	}
}
