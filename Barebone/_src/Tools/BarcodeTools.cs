using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの監理 -> 参照 -> BarcodeLib 3.1.5

	public static class BarcodeTools
	{
		public enum BarcodeType_e
		{
			Code128 = 1,
			Code39,
			Ean13,
			Ean8,
		}

		private static Image P_Create(BarcodeType_e barcodeType, string text, Color barColor, Color backgroundColor, int w, int h)
		{
			// barcodeType

			if (string.IsNullOrEmpty(text))
				throw new Exception("Bad text");

			// barColor
			// backgroundColor

			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			// ----

			BarcodeStandard.Type barcodeType2;
			switch (barcodeType)
			{
				case BarcodeType_e.Code128:
					barcodeType2 = BarcodeStandard.Type.Code128;
					break;

				case BarcodeType_e.Code39:
					barcodeType2 = BarcodeStandard.Type.Code39;
					break;

				case BarcodeType_e.Ean13:
					barcodeType2 = BarcodeStandard.Type.Ean13;
					break;

				case BarcodeType_e.Ean8:
					barcodeType2 = BarcodeStandard.Type.Ean8;
					break;

				default:
					throw new Exception("Bad barcodeType");
			}

			SkiaSharp.SKColorF barColor2 = new SkiaSharp.SKColorF(
				barColor.R / 255F,
				barColor.G / 255F,
				barColor.B / 255F
				);

			SkiaSharp.SKColorF backgroundColor2 = new SkiaSharp.SKColorF(
				backgroundColor.R / 255F,
				backgroundColor.G / 255F,
				backgroundColor.B / 255F
				);

			using (BarcodeStandard.Barcode barcode = new BarcodeStandard.Barcode())
			{
				SkiaSharp.SKImage barcodeImage = barcode.Encode(
					barcodeType2,
					text,
					barColor2,
					backgroundColor2,
					w,
					h
					);

				using (SkiaSharp.SKData imageData = barcodeImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
				using (MemoryStream mem = new MemoryStream())
				{
					imageData.SaveTo(mem);

					return Image.FromStream(mem);
				}
			}
		}

		public static Image CreateCode128(string text, Color barColor, Color backgroundColor, int w, int h)
		{
			return P_Create(
				BarcodeType_e.Code128,
				text,
				barColor,
				backgroundColor,
				w,
				h
				);
		}

		public static Image CreateCode39(string text, Color barColor, Color backgroundColor, int w, int h)
		{
			return P_Create(
				BarcodeType_e.Code39,
				text,
				barColor,
				backgroundColor,
				w,
				h
				);
		}

		public static Image CreateEan13(string text, Color barColor, Color backgroundColor, int w, int h)
		{
			return P_Create(
				BarcodeType_e.Ean13,
				text,
				barColor,
				backgroundColor,
				w,
				h
				);
		}

		public static Image CreateEan8(string text, Color barColor, Color backgroundColor, int w, int h)
		{
			return P_Create(
				BarcodeType_e.Ean8,
				text,
				barColor,
				backgroundColor,
				w,
				h
				);
		}
	}
}
