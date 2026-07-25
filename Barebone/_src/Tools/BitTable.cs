using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	public class BitTable
	{
		private BitList Inner = new BitList();

		public readonly int W;
		public readonly int H;

		public BitTable(int w, int h)
		{
			if (
				w < 1 || SCommon.IMAX < w ||
				h < 1 || SCommon.IMAX < h
				)
				throw new Exception("Bad w_h");

			this.W = w;
			this.H = h;
		}

		public bool this[int x, int y]
		{
			get
			{
				this.CheckCoordination(x, y);

				return this.Inner[this.CoordinationToIndex(x, y)];
			}

			set
			{
				this.CheckCoordination(x, y);

				this.Inner[this.CoordinationToIndex(x, y)] = value;
			}
		}

		private void CheckCoordination(int x, int y)
		{
			if (
				x < 0 || this.W <= x ||
				y < 0 || this.H <= y
				)
				throw new Exception("Bad coordination");
		}

		private int CoordinationToIndex(int x, int y)
		{
			return x * this.H + y;
		}

		public void Clear()
		{
			this.Inner.Clear();
		}
	}
}
