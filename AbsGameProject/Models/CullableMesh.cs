using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject.Models;

public class CullableMesh
{
    public class CullableFace
    {
        public List<Vector3D<float>> Positions = new();
        public List<Vector3D<float>> Normals = new();
        public List<Vector2D<float>> UVs = new();
        public List<int?> TintIndicies = new(); 
    }

    public Dictionary<CullFaceDirection, CullableFace> Faces { get; private set; } = new()
    {
        {CullFaceDirection.Down, new CullableFace() },
        {CullFaceDirection.Up, new CullableFace() },
        {CullFaceDirection.North, new CullableFace() },
        {CullFaceDirection.South, new CullableFace() },
        {CullFaceDirection.East, new CullableFace() },
        {CullFaceDirection.West, new CullableFace() },
        {CullFaceDirection.All, new CullableFace() }
    };

    const int VERT_SCALE = 16;

    public static CullableMesh? TryFromVoxelMesh(VoxelModel voxel)
    {
        var mesh = new CullableMesh();
        foreach (var elem in voxel.Elements)
        {
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
                var tex = facePair.Value.Texture;
                var coords = TextureAtlas.BlockLocations[tex];
                var uvs = new Rectangle<float>(
                    (coords.Origin.X / (float)TextureAtlas.Size), 
                    (coords.Origin.Y / (float)TextureAtlas.Size), 
                    (coords.Size.X / (float)TextureAtlas.Size),
                    (coords.Size.Y / (float)TextureAtlas.Size));

                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X + uvs.Size.X, uvs.Origin.Y + uvs.Size.Y));
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X + uvs.Size.X, uvs.Origin.Y));
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X, uvs.Origin.Y + uvs.Size.Y));

                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X, uvs.Origin.Y + uvs.Size.Y));
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X + uvs.Size.X, uvs.Origin.Y));
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].UVs
                    .Add(new Vector2D<float>(uvs.Origin.X, uvs.Origin.Y));

                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);
                mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].TintIndicies.Add(facePair.Value.TintIndex);

                switch (facePair.Key)
                {
                    case CullFaceDirection.Up:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[2]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[3]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[6]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[6]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[3]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[7]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitY);
                        break;
                    case CullFaceDirection.Down:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[4]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[5]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[0]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[0]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[5]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[1]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitY);
                        break;
                    case CullFaceDirection.West:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[6]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[7]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[4]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[4]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[7]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[5]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitX);
                        break;
                    case CullFaceDirection.East:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[3]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[2]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[1]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[1]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[2]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[0]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitX);
                        break;
                    case CullFaceDirection.North:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[7]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[3]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[5]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[5]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[3]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[1]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(Vector3D<float>.UnitZ);
                        break;
                    case CullFaceDirection.South:
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[2]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[6]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[0]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[0]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[6]);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Positions.Add(elemVerts[4]);

                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        mesh.Faces[facePair.Value.CullFace ?? CullFaceDirection.All].Normals.Add(-Vector3D<float>.UnitZ);
                        break;
                }
            }
        }

        return mesh;
    }
}
