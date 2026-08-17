using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLTStudio.Commons;

namespace HLTStudio.Drawings
{
	public struct D2Point
	{
		public double X;
		public double Y;

		public D2Point(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		public I2Point ToI2Point()
		{
			return new I2Point(
				SCommon.ToInt(this.X),
				SCommon.ToInt(this.Y)
				);
		}

		public static D2Point operator +(D2Point a, D2Point b)
		{
			return new D2Point(a.X + b.X, a.Y + b.Y);
		}

		public static D2Point operator -(D2Point a, D2Point b)
		{
			return new D2Point(a.X - b.X, a.Y - b.Y);
		}

		public static D2Point operator *(D2Point a, double b)
		{
			return new D2Point(a.X * b, a.Y * b);
		}

		public static D2Point operator /(D2Point a, double b)
		{
			return new D2Point(a.X / b, a.Y / b);
		}

		public double GetDistance(D2Point other)
		{
			return GetDistance(this, other);
		}

		public static double GetDistance(D2Point a, D2Point b)
		{
			return (a - b).GetDistanceToOrigin();
		}

		public double GetDistanceToOrigin()
		{
			return Math.Sqrt(this.X * this.X + this.Y * this.Y);
		}
	}
}
