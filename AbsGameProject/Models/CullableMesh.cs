using Silk.NET.Maths;

namespace AbsGameProject.Models;

internal class CullableMesh
{
    const int VERT_SCALE = 16;

    public readonly Dictionary<CullFaceDirection, List<Vector3D<float>>> verts = new()
    {
        {CullFaceDirection.Down, new List<Vector3D<float>>() },
        {CullFaceDirection.Up, new List<Vector3D<float>>() },
        {CullFaceDirection.North, new List<Vector3D<float>>() },
        {CullFaceDirection.South, new List<Vector3D<float>>() },
        {CullFaceDirection.East, new List<Vector3D<float>>() },
        {CullFaceDirection.West, new List<Vector3D<float>>() },
    };

    public static bool TryFromVoxelMesh(VoxelModel voxel, out CullableMesh? mesh)
    {
        mesh = new CullableMesh();
        foreach (var elem in voxel.Elements)
        {
            uint vertOffset = (uint)mesh.verts.Count;

            float lx = MathF.Min(elem.From[0], elem.To[0]) / VERT_SCALE;
            float mx = MathF.Max(elem.From[0], elem.To[0]) / VERT_SCALE;
            float ly = MathF.Min(elem.From[1], elem.To[1]) / VERT_SCALE;
            float my = MathF.Max(elem.From[1], elem.To[1]) / VERT_SCALE;
            float lz = MathF.Min(elem.From[2], elem.To[2]) / VERT_SCALE;
            float mz = MathF.Max(elem.From[2], elem.To[2]) / VERT_SCALE;

            Vector3D<float>[] elemVerts = new Vector3D<float>[]{
                new  Vector3D<float>(lx, ly, lz), new Vector3D < float >(lx, ly, mz),
                new Vector3D < float >(lx, my, lz), new Vector3D < float >(lx, my, mz),
                new Vector3D < float >(mx, ly, lz), new Vector3D < float >(mx, ly, mz),
                new Vector3D < float >(mx, my, lz), new Vector3D < float >(mx, my, mz)
            };

            foreach (var facePair in elem.Faces)
            {
                switch (facePair.Key)
                {
                    case CullFaceDirection.Up:
                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[2]);
                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[3]);
                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[6]);

                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[6]);
                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[3]);
                        mesh.verts[CullFaceDirection.Up].Add(elemVerts[7]);
                        break;
                    case CullFaceDirection.Down:
                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[4]);
                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[5]);
                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[0]);

                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[0]);
                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[5]);
                        mesh.verts[CullFaceDirection.Down].Add(elemVerts[1]);
                        break;
                    case CullFaceDirection.East:
                        mesh.verts[CullFaceDirection.East].Add(elemVerts[6]);
                        mesh.verts[CullFaceDirection.East].Add(elemVerts[7]);
                        mesh.verts[CullFaceDirection.East].Add(elemVerts[4]);

                        mesh.verts[CullFaceDirection.East].Add(elemVerts[4]);
                        mesh.verts[CullFaceDirection.East].Add(elemVerts[7]);
                        mesh.verts[CullFaceDirection.East].Add(elemVerts[5]);
                        break;                                                          
                    case CullFaceDirection.West:
                        mesh.verts[CullFaceDirection.West].Add(elemVerts[3]);
                        mesh.verts[CullFaceDirection.West].Add(elemVerts[2]);
                        mesh.verts[CullFaceDirection.West].Add(elemVerts[1]);

                        mesh.verts[CullFaceDirection.West].Add(elemVerts[1]);
                        mesh.verts[CullFaceDirection.West].Add(elemVerts[2]);
                        mesh.verts[CullFaceDirection.West].Add(elemVerts[0]);
                        break;
                    case CullFaceDirection.South:
                        mesh.verts[CullFaceDirection.South].Add(elemVerts[7]);
                        mesh.verts[CullFaceDirection.South].Add(elemVerts[3]);
                        mesh.verts[CullFaceDirection.South].Add(elemVerts[5]);

                        mesh.verts[CullFaceDirection.South].Add(elemVerts[5]);
                        mesh.verts[CullFaceDirection.South].Add(elemVerts[3]);
                        mesh.verts[CullFaceDirection.South].Add(elemVerts[1]);
                        break;
                    case CullFaceDirection.North:
                        mesh.verts[CullFaceDirection.North].Add(elemVerts[2]);
                        mesh.verts[CullFaceDirection.North].Add(elemVerts[6]);
                        mesh.verts[CullFaceDirection.North].Add(elemVerts[0]);

                        mesh.verts[CullFaceDirection.North].Add(elemVerts[0]);
                        mesh.verts[CullFaceDirection.North].Add(elemVerts[6]);
                        mesh.verts[CullFaceDirection.North].Add(elemVerts[4]);
                        break;
                }
            }
        }

        return true;
    }
}
