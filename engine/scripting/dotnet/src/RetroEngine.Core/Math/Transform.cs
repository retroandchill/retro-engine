using System.Runtime.InteropServices;
using RetroEngine.Binds;

namespace RetroEngine.Core.Math;

[StructLayout(LayoutKind.Sequential)]
[BlittableType("retro::Transform", CppModule = "retro.core")]
public readonly record struct Transform
{
    public Vector2F Position { get; init; }
    public float Rotation { get; init; }
    public Vector2F Scale { get; init; }
}
