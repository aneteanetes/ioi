using System;
using System.Drawing;

namespace ioi
{
    internal static class PointScaleExtensions
    {
        public static Point Scale(this Point point, double scale)
        {
            var x = Math.Round(point.X * scale);
            var y = Math.Round(point.Y * scale);

            return new Point((int)x, (int)y);
        }
    }
}