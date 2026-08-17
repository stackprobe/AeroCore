using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの監理 -> 参照 -> SixLabors.ImageSharp 2.1.13

	public static class ImageSharpTools
	{
		public static Image LoadFromFile(string file)
		{
			using (var sLISImage = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(file))
			{
				int w = sLISImage.Width;
				int h = sLISImage.Height;

				Bitmap bmp = new Bitmap(w, h);

				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						var dot = sLISImage[x, y];

						bmp.SetPixel(x, y, Color.FromArgb(
							dot.A,
							dot.R,
							dot.G,
							dot.B
							));
					}
				}
				return bmp;
			}
		}
	}
}
