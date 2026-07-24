

using System.Numerics;

namespace ioi
{
    internal static class VectorExtensions
    {
        public static Vector3 ToVector3(this Vector2 vector2, float z=0)
        {
            return new Vector3(vector2,z);
        }
        
        public static Vector2 MultipleOpposite(this Vector2 vector2, Vector2 multiplier)
        {
            var diff = vector2 - (vector2 * multiplier);
            return vector2 + diff / 2;
        }

        public static string StringHash(this Vector2 vector2) => $"{vector2.X}{vector2.Y}";
    }
}
