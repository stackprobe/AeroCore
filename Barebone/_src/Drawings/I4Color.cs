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
	/// アルファ値を含む色を表す。
	/// 各色は 0 ～ 255 を想定する。
	/// </summary>
	public struct I4Color
	{
		private byte _bR;
		private byte _bG;
		private byte _bB;
		private byte _bA;

		public int R => (int)_bR;
		public int G => (int)_bG;
		public int B => (int)_bB;
		public int A => (int)_bA;

		public I4Color(int r, int g, int b, int a)
		{
#if DEBUG
			if (
				r < 0 || 255 < r ||
				g < 0 || 255 < g ||
				b < 0 || 255 < b ||
				a < 0 || 255 < a
				)
				throw new Exception("不正な引数");
#endif
			_bR = (byte)r;
			_bG = (byte)g;
			_bB = (byte)b;
			_bA = (byte)a;
		}

		public I4Color(Color color)
		{
			_bR = color.R;
			_bG = color.G;
			_bB = color.B;
			_bA = color.A;
		}

		public I4Color(string strColor)
		{
			if (!Regex.IsMatch(strColor, "^[0-9A-Fa-f]{8}$"))
				throw new Exception("Bad strColor");

			byte[] bytes = SCommon.Hex.I.GetBytes(strColor);

			_bR = bytes[0];
			_bG = bytes[1];
			_bB = bytes[2];
			_bA = bytes[3];
		}

		public override string ToString()
		{
			return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}", this.R, this.G, this.B, this.A);
		}

		public I3Color WithoutAlpha()
		{
			return new I3Color(this.R, this.G, this.B);
		}

		public D4Color ToD4Color()
		{
			return new D4Color(
				this.R / 255.0,
				this.G / 255.0,
				this.B / 255.0,
				this.A / 255.0
				);
		}

		public Color ToColor()
		{
			// ARGB 注意
			return Color.FromArgb(this.A, this.R, this.G, this.B);
		}

		public bool IsSame(I4Color other)
		{
			return
				this.R == other.R &&
				this.G == other.G &&
				this.B == other.B &&
				this.A == other.A;
		}
	}
}
