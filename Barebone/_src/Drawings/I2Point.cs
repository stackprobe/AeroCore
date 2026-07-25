using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLTStudio.Drawings
{
	public struct I2Point
	{
		public int X;
		public int Y;

		public I2Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public D2Point ToD2Point()
		{
			return new D2Point(this.X, this.Y);
		}

		public static I2Point operator +(I2Point a, I2Point b)
		{
			return new I2Point(a.X + b.X, a.Y + b.Y);
		}

		public static I2Point operator -(I2Point a, I2Point b)
		{
			return new I2Point(a.X - b.X, a.Y - b.Y);
		}

		public static I2Point operator *(I2Point a, int b)
		{
			return new I2Point(a.X * b, a.Y * b);
		}

		public static I2Point operator /(I2Point a, int b)
		{
			return new I2Point(a.X / b, a.Y / b);
		}

		public bool IsSame(I2Point other)
		{
			return
				this.X == other.X &&
				this.Y == other.Y;
		}

		public double GetDistance(I2Point other)
		{
			return GetDistance(this, other);
		}

		public static double GetDistance(I2Point a, I2Point b)
		{
			return (a - b).GetDistanceToOrigin();
		}

		public double GetDistanceToOrigin()
		{
			return Math.Sqrt(this.X * this.X + this.Y * this.Y);
		}
	}
}
