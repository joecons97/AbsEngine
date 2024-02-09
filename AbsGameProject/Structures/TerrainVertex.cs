using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace AbsGameProject.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TerrainVertex
{
    public Vector3D<byte> position;
    public Vector3D<byte> colour;
    public Vector2D<Half> uv;
    public byte light;
}
