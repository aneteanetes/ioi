using System;
using System.Numerics;

namespace ioi
{
    internal static class VectorRoundExtentsion
    {
        public static Vector2 RoundNew(this Vector2 vector)
        {
            var x = MathF.Round(vector.X);
            var y = MathF.Round(vector.Y);

            return new Vector2(x, y);
        }

        public static Vector2 AsVector2(this string str)
        {
            if (!str.Contains(","))
                throw new ArgumentException("string does not contains ','!");

            var split = str.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var left = split[0].Replace(".",",");
            var right = split[1].Replace(".", ",");

            if(!float.TryParse(left, out var x))
            {
                throw new ArgumentException("x is not recognized: "+left);
            }

            if (!float.TryParse(right, out var y))
            {
                throw new ArgumentException("y is not recognized: " + right);
            }

            return new Vector2(x, y);
        }
    }
}
