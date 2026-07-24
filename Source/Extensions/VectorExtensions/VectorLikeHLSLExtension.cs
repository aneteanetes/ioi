using Godot;

namespace ioi
{
    internal static class VectorLikeHLSLExtension
    {
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
