using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using HLTStudio.Commons;

namespace HLTStudio.Drawings
{
	/// <summary>
	/// アルファ値の無い色を表す。
	/// 各色は 0 ～ 255 を想定する。
	/// </summary>
	public struct I3Color
	{
		private byte _bR;
		private byte _bG;
		private byte _bB;

		public int R => (int)_bR;
		public int G => (int)_bG;
		public int B => (int)_bB;

		public I3Color(int r, int g, int b)
		{
#if DEBUG
			if (
				r < 0 || 255 < r ||
				g < 0 || 255 < g ||
				b < 0 || 255 < b
				)
				throw new Exception("不正な引数");
#endif
			_bR = (byte)r;
			_bG = (byte)g;
			_bB = (byte)b;
		}

		public I3Color(Color color)
		{
			_bR = color.R;
			_bG = color.G;
			_bB = color.B;
		}

		public I3Color(string strColor)
		{
			if (!Regex.IsMatch(strColor, "^[0-9A-Fa-f]{6}$"))
				throw new Exception("Bad strColor");

			byte[] bytes = SCommon.Hex.I.GetBytes(strColor);

			_bR = bytes[0];
			_bG = bytes[1];
			_bB = bytes[2];
		}

		public override string ToString()
		{
			return string.Format("{0:x2}{1:x2}{2:x2}", this.R, this.G, this.B);
		}

		public I4Color WithAlpha(int a = 255)
		{
			return new I4Color(this.R, this.G, this.B, a);
		}

		public D3Color ToD3Color()
		{
			return new D3Color(
				this.R / 255.0,
				this.G / 255.0,
				this.B / 255.0
				);
		}

		public Color ToColor()
		{
			return Color.FromArgb(this.R, this.G, this.B);
		}

		public bool IsSame(I3Color other)
		{
			return
				this.R == other.R &&
				this.G == other.G &&
				this.B == other.B;
		}
	}
}
