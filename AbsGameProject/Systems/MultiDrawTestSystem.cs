using AbsEngine.ECS;
using AbsEngine.Rendering;
using AbsEngine.Rendering.RenderCommand;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Systems.Terrain;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace AbsGameProject.Systems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MultiDrawTestBuffer
{
    public Matrix4X4<float> transform;
    public Vector4D<float> color;
}

internal class MultiDrawTestSystem : AbsEngine.ECS.System
{
    GraphicsBuffer _vertexBuffer;
    DrawBuffer _drawBuffer;
    Material _material;
    DrawArraysIndirectCommand[] _drawCommands;
    MultiDrawTestBuffer[] _buffers;

    MultiDrawRenderCommand<MultiDrawTestBuffer> _command;

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

        _buffers = new[]
        {
            new MultiDrawTestBuffer()
            {
                transform = Matrix4X4.CreateScale(Vector3D<float>.One) *
                                Matrix4X4.CreateTranslation(new Vector3D<float>(0,100,0)),
                color = new Vector4D<float>(0,1,0,1)
            }
        };
        _material = new Material("MultiDrawTest");

        _command = new MultiDrawRenderCommand<MultiDrawTestBuffer>(_drawBuffer, _drawCommands, _material, _buffers);
    }

    public override void Tick(float deltaTime)
    {
        Renderer.Render(_command);
    }
}
