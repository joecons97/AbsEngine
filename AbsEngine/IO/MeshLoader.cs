using Assimp;
using Silk.NET.Maths;

namespace AbsEngine.IO;

public static class MeshLoader
{
    public static Rendering.Mesh? LoadMesh(string fileLocation)
    {
        using AssimpContext assimpContext = new AssimpContext();

        var assimpScene = assimpContext.ImportFile(fileLocation,
            PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate |
            PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateNormals);

        if (assimpScene.MeshCount == 0)
            return null;

        if (assimpScene.MeshCount > 1)
            throw new NotImplementedException();

        var mesh = assimpScene.Meshes.First();

        var tris = new List<uint>();

        foreach (var face in mesh.Faces)
        {
            foreach (var index in face.Indices)
            {
                tris.Add((uint)index);
            }
        }

        var finalMesh = new Rendering.Mesh()
        {
            Positions = mesh.Vertices.Select(x => new Vector3D<float>(x.X, x.Y, x.Z)).ToArray(),
            Normals = mesh.Normals.Select(x => new Vector3D<float>(x.X, x.Y, x.Z)).ToArray(),
            Uvs = mesh.TextureCoordinateChannels[0].Select(x => new Vector2D<float>(x.X, x.Y)).ToArray(),
            Colours = Enumerable.Range(0, mesh.Vertices.Count).Select(x => new Vector4D<float>()).ToArray(),
            Triangles = tris.ToArray()
        };

        finalMesh.Build();

        return finalMesh;
    }
}
