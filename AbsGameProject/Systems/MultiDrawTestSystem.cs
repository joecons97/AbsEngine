using AbsEngine.ECS;
using AbsEngine.Rendering;
using AbsEngine.Rendering.RenderCommand;
using AbsGameProject.Models;
using AbsGameProject.Systems.Terrain;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace AbsGameProject.Systems;

internal class MultiDrawTestSystem : AbsEngine.ECS.System
{
    GraphicsBuffer _vertexBuffer;
    DrawBuffer _drawBuffer;
    Material _material;
    DrawArraysIndirectCommand[] _drawCommands;
    Matrix4X4<float>[] _trs;

    public MultiDrawTestSystem(Scene scene) : base(scene)
    {
        var model = VoxelModel.TryFromFile("Content/Models/Blocks/Dirt.json");
        if (model == null)
            throw new Exception();

        var mesh = CullableMesh.TryFromVoxelMesh(model);
        if (mesh == null)
            throw new Exception();

        List<TerrainVertex> vertices = new List<TerrainVertex>();

        foreach (var face in mesh.Faces)
        {
            for (int i = 0; i < face.Value.Positions.Count; i++)
            {
                var pos = face.Value.Positions[i];
                var uv = face.Value.UVs[i];

                vertices.Add(new TerrainVertex()
                {
                    position = (Vector3D<byte>)pos,
                    uv = (Vector2D<Half>)uv,
                    colour = new Vector4D<byte>(255, 255, 255, 255)
                });
            }
        }

        _vertexBuffer = new GraphicsBuffer(GraphicsBufferType.Vertices);
        _vertexBuffer.SetData(CollectionsMarshal.AsSpan(vertices));

        _drawBuffer = new DrawBuffer(_vertexBuffer);
        _drawBuffer.SetVertexAttributes(new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttributeFormat.SInt8, 3),
            new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
        });

        _drawCommands = new DrawArraysIndirectCommand[1]
        {
            new DrawArraysIndirectCommand()
            {
                count = (uint)vertices.Count,
                instanceCount = 1,
                baseInstance = 0,
                firstVertex = 0
            }
        };

        _trs = new Matrix4X4<float>[1]
        {
            Matrix4X4.CreateScale(Vector3D<float>.One) *
                            Matrix4X4.CreateTranslation(new Vector3D<float>(0,100,0))
        };

        _material = new Material("MultiDrawTest");
    }

    public override void Tick(float deltaTime)
    {
        Renderer.MultiDrawRender(_drawBuffer, _drawCommands, _material, _trs);
    }
}
