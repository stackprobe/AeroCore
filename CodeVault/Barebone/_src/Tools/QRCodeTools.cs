using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの管理 -> 参照 -> QRCoder 1.7.0

	public static class QRCodeTools
	{
		public static Image Create(string text, Color 升Color, Color backgroundColor, int 升Size)
		{
			if (string.IsNullOrEmpty(text))
				throw new Exception("Bad text");

			// 升Color
			// backgroundColor

			if (升Size < 1 || SCommon.IMAX < 升Size)
				throw new Exception("Bad 升Size");

			// ----

			using (QRCoder.QRCodeGenerator gen = new QRCoder.QRCodeGenerator())
			using (QRCoder.QRCodeData data = gen.CreateQrCode(
				// プレーンデータ
				// -- ECCLevel.Q のとき 800 文字程度が実質上限
				text,
				// 全体のデータ量に占める誤り訂正データの割合：
				// L = 0.2
				// M = 0.35
				// Q = 0.5   <-- 業務で推奨
				// H = 0.65
				QRCoder.QRCodeGenerator.ECCLevel.Q,
				// UTF-8 として扱うか -- true で良し
				true,
				// UTF-8_BOM を付けるか -- false で良し
				false,
				// ECI(拡張チャンネル識別)で文字セット指定 -- UTF-8 で良し
				QRCoder.QRCodeGenerator.EciMode.Utf8,
				// バージョン(サイズ情報)
				// -1 は自動
				// 明示的にサイズを固定したい時のみ指定
				-1
				))
			using (QRCoder.QRCode code = new QRCoder.QRCode(data))
			using (Bitmap bmp = code.GetGraphic(
				// 1モジュール(1升)のサイズ(ピクセル四方)
				升Size,
				// 黒升の色(暗い方の色)
				升Color,
				// 白升の色(明るい方の色)
				backgroundColor,
				// 余白を付けるか
				// -- true のとき、上下左右に4升分の余白を追加
				true
				))
			using (MemoryStream mem = new MemoryStream())
			{
				bmp.Save(mem, ImageFormat.Png);

				return Image.FromStream(mem);
			}
		}
	}
}
