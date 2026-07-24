
using Godot;

namespace ioi.Source.Scenes
{
    internal interface IViewportScene
    {
        void ProcessClick(Vector2 posWorld, Vector2 posLocal);
    }
}
