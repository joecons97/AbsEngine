using AbsEngine.Rendering;
using Silk.NET.Maths;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbsGameProject.Models.Meshing;

public class RawMesh
{
    public uint[] Triangles { get; set; } = Array.Empty<uint>();
    public Vector4D<float>[] Colours { get; set; } = Array.Empty<Vector4D<float>>();
    public Vector3D<float>[] Positions { get; set; } = Array.Empty<Vector3D<float>>();
    public Vector3D<float>[] Normals { get; set; } = Array.Empty<Vector3D<float>>();
    public Vector3D<float>[] Tangents { get; set; } = Array.Empty<Vector3D<float>>();
    public Vector2D<float>[] Uvs { get; set; } = Array.Empty<Vector2D<float>>();

    public Mesh ToMesh()
    {
        var mesh = new Mesh();
        mesh.Triangles = Triangles;
        mesh.Colours = Colours;
        mesh.Positions = Positions;
        mesh.Normals = Normals;
        mesh.Tangents = Tangents;
        mesh.Uvs = Uvs;
        return mesh;
    }
}

public class VoxelModel
{
    const int VERT_SCALE = 16;

    [JsonPropertyName("textures")]
    public Dictionary<string, string> Textures { get; init; } = new Dictionary<string, string>();

    [JsonPropertyName("elements")]
    public List<VoxelElementModel> Elements { get; init; } = new List<VoxelElementModel>();

    public RawMesh ToRawMesh(FaceDirection cullFaces)
    {
        RawMesh mesh = new RawMesh();
        var verts = new List<Vector3D<float>>();
        var indices = new List<uint>();

        foreach (var elem in Elements)
        {
            uint vertOffset = (uint)verts.Count;

            float lx = MathF.Min(elem.From[0], elem.To[0]) / VERT_SCALE;
            float mx = MathF.Max(elem.From[0], elem.To[0]) / VERT_SCALE;
            float ly = MathF.Min(elem.From[1], elem.To[1]) / VERT_SCALE;
            float my = MathF.Max(elem.From[1], elem.To[1]) / VERT_SCALE;
            float lz = MathF.Min(elem.From[2], elem.To[2]) / VERT_SCALE;
            float mz = MathF.Max(elem.From[2], elem.To[2]) / VERT_SCALE;

            verts.Add(new Vector3D<float>(lx, ly, lz) * -1);
            verts.Add(new Vector3D<float>(lx, ly, mz) * -1);
            verts.Add(new Vector3D<float>(lx, my, lz) * -1);
            verts.Add(new Vector3D<float>(lx, my, mz) * -1);
            verts.Add(new Vector3D<float>(mx, ly, lz) * -1);
            verts.Add(new Vector3D<float>(mx, ly, mz) * -1);
            verts.Add(new Vector3D<float>(mx, my, lz) * -1);
            verts.Add(new Vector3D<float>(mx, my, mz) * -1);

            foreach (var facePair in elem.Faces)
            {
                var face = facePair.Value;

                if (cullFaces.HasFlag(face.CullFace ?? FaceDirection.All))
                    continue;

                switch (facePair.Key)
                {
                    case FaceDirection.Up:
                        indices.Add(vertOffset + 6);
                        indices.Add(vertOffset + 3);
                        indices.Add(vertOffset + 2);

                        indices.Add(vertOffset + 6);
                        indices.Add(vertOffset + 7);
                        indices.Add(vertOffset + 3);
                        break;
                    case FaceDirection.Down:
                        indices.Add(vertOffset + 0);
                        indices.Add(vertOffset + 5);
                        indices.Add(vertOffset + 4);

                        indices.Add(vertOffset + 0);
                        indices.Add(vertOffset + 1);
                        indices.Add(vertOffset + 5);
                        break;
                    case FaceDirection.East:
                        indices.Add(vertOffset + 4);
                        indices.Add(vertOffset + 7);
                        indices.Add(vertOffset + 6);

                        indices.Add(vertOffset + 4);
                        indices.Add(vertOffset + 5);
                        indices.Add(vertOffset + 7);
                        break;
                    case FaceDirection.West:
                        indices.Add(vertOffset + 1);
                        indices.Add(vertOffset + 2);
                        indices.Add(vertOffset + 3);

                        indices.Add(vertOffset + 1);
                        indices.Add(vertOffset + 0);
                        indices.Add(vertOffset + 2);
                        break;
                    case FaceDirection.South:
                        indices.Add(vertOffset + 5);
                        indices.Add(vertOffset + 3);
                        indices.Add(vertOffset + 7);

                        indices.Add(vertOffset + 5);
                        indices.Add(vertOffset + 1);
                        indices.Add(vertOffset + 3);
                        break;
                    case FaceDirection.North:
                        indices.Add(vertOffset + 0);
                        indices.Add(vertOffset + 6);
                        indices.Add(vertOffset + 2);

                        indices.Add(vertOffset + 0);
                        indices.Add(vertOffset + 4);
                        indices.Add(vertOffset + 6);
                        break;
                }
            }
        }

        var vertsToRemove = new List<int>();
        for (int i = 0; i < verts.Count; i++)
        {
            if (!indices.Contains((uint)i))
                vertsToRemove.Add(i);
        }

        foreach (var vert in vertsToRemove.OrderByDescending(x => x))
        {
            verts.RemoveAt(vert);
        }

        mesh.Positions = verts.ToArray();
        mesh.Triangles = indices.ToArray();
        mesh.Normals = new Vector3D<float>[0];
        mesh.Tangents = new Vector3D<float>[0];
        mesh.Colours = new Vector4D<float>[0];
        mesh.Uvs = new Vector2D<float>[0];

        return mesh;
    }

    public static VoxelModel? TryFromFile(string path)
    {
        var file = File.ReadAllText(path);
        try
        {
            return JsonSerializer.Deserialize<VoxelModel>(file);
        }
        catch
        {
            return null;
        }
    }
}
