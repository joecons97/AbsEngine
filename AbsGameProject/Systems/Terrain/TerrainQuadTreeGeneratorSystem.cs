using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.IO;
using AbsEngine.Rendering;
using Silk.NET.Maths;
using System.Drawing;

namespace AbsGameProject.Systems.Terrain
{
    public class Node
    {
        private const int MIN_CHUNK_SIZE = 16;

        private const float THRESHOLD_MULTIPLIER = 5;

        public Vector2D<int> Position { get; set; }
        public int Size { get; set; }
        public List<Node> Children { get; set; }
        public Node? Parent { get; set; }

        public Node(Node? parent, Vector2D<int> pos, int size)
        {
            Children = new List<Node>();
            Parent = parent;
            Position = pos;
            Size = size;
        }
        public bool IsInside(Vector3D<int> pos)
        {
            return Vector2D.Distance(Position, new Vector2D<int>(pos.X, pos.Z)) < Size * THRESHOLD_MULTIPLIER;
        }

        public void Recurse(Vector3D<int> pos, TerrainQuadTreeGeneratorSystem sys)
        {
            if (IsInside(pos) && Size > MIN_CHUNK_SIZE)
            {
                var offset = (int)(Size / 4f);

                //Bottom right
                Children.Add(new Node(this, Position + new Vector2D<int>(offset, -offset), (int)(Size / 2f)));
                //Bottom left
                Children.Add(new Node(this, Position + new Vector2D<int>(-offset, -offset), (int)(Size / 2f)));
                //Top right
                Children.Add(new Node(this, Position + new Vector2D<int>(offset, offset), (int)(Size / 2f)));
                //Top left
                Children.Add(new Node(this, Position + new Vector2D<int>(-offset, offset), (int)(Size / 2f)));

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Recurse(pos, sys);
                }
            }

            if (Children.Count == 0)
            {
                sys._toLoad.Add(this);
            }
        }

        public override string ToString()
        {
            return Position.ToString() + $"({Size},{Size})";
        }

        public override int GetHashCode()
        {
            return Position.X.GetHashCode() ^ (Position.Y.GetHashCode() << 2) ^ (Size.GetHashCode() >> 2);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            var node = ((Node)obj);
            return Position == node.Position && Size == node.Size;
        }

        public static bool operator ==(Node lhs, Node rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                // ...and right hand side is null...
                if (ReferenceEquals(rhs, null))
                {
                    //...both are null and are Equal.
                    return true;
                }

                // ...right hand side is not null, therefore not Equal.
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Node lhs, Node rhs)
        {
            return !(lhs == rhs);
        }
    }

    public class TerrainQuadTreeGeneratorSystem : AbsEngine.ECS.System
    {
        public List<Node> _toLoad = new List<Node>();

        Dictionary<int, Vector4D<float>> _colors = new Dictionary<int, Vector4D<float>>()
        {
            { 16, new Vector4D<float>(1,0,0,0) },
            { 32, new Vector4D<float>(0,1,0,0) },
            { 64, new Vector4D<float>(0,0,1,0) },
            { 128, new Vector4D<float>(1,0,1,0) },
            { 256, new Vector4D<float>(1,1,1,0) },
            { 512, new Vector4D<float>(1,1,0,0) },
            { 1024, new Vector4D<float>(0,0,0,0) },
            { 2048, new Vector4D<float>(1,1,1,0) },
            { 4096, new Vector4D<float>(0.5f,0.5f,0.5f,0) },
        };

        public TerrainQuadTreeGeneratorSystem(Scene scene) : base(scene)
        {
            var planeMesh = MeshLoader.LoadMesh("Engine/Meshes/Plane.fbx");
            Node rootNode = new Node(null, Vector2D<int>.Zero, 4096);

            rootNode.Recurse(new Vector3D<int>(0, 0, 0), this);

            foreach (var node in _toLoad)
            {
                var ent = Scene.EntityManager.CreateEntity(node.ToString());
                ent.Transform.Position = new Vector3D<float>(node.Position.X, 0, node.Position.Y);
                ent.Transform.LocalEulerAngles = new Vector3D<float>(-90, 0, 0);
                ent.Transform.LocalScale = (node.Size / 2) * Vector3D<float>.One;

                var mesh = ent.AddComponent<MeshRendererComponent>();
                mesh.Mesh = planeMesh;
                mesh.Material = new Material("QuadTreeChunk");

                var col = _colors[node.Size];
                mesh.Material.SetColor("Colour", Color.FromArgb((int)(col.X * 255), (int)(col.Y * 255), (int)(col.Z * 255)));
            }
        }
    }
}
