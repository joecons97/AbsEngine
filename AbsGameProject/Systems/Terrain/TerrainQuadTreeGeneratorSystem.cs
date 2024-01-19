using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.IO;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using Silk.NET.Maths;
using System.Drawing;

namespace AbsGameProject.Systems.Terrain
{
    public class Node
    {
        public const int MIN_CHUNK_SIZE = 16;

        private const float THRESHOLD_MULTIPLIER = 5;

        public Entity? AssociatedEntity { get; set; }
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

            //TODO Remove Size == 16
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
        public List<Node> _activeNodes = new List<Node>();
        public List<Node> _lastActiveNodes = new List<Node>();
        public Queue<Entity> _pool = new Queue<Entity>();

        Mesh _planeMesh;
        CameraComponent _mainCam;
        TransformComponent _mainCameraTransform;
        Vector2D<int>? _lastPos;

        IReadOnlyCollection<Component> _sceneChunkListReference;

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
            _mainCam = Scene.EntityManager.GetComponents<SceneCameraComponent>().First();
            _mainCameraTransform = _mainCam.Entity.Transform;

            _planeMesh = MeshLoader.LoadMesh("Engine/Meshes/Plane.fbx");

            _sceneChunkListReference = Scene.EntityManager.GetComponentListReference<TerrainChunkComponent>();
        }

        public override void Tick(float deltaTime)
        {
            var roundedX = (int)MathF.Floor(_mainCameraTransform.LocalPosition.X / Node.MIN_CHUNK_SIZE) * Node.MIN_CHUNK_SIZE;
            var roundedZ = (int)MathF.Floor(_mainCameraTransform.LocalPosition.Z / Node.MIN_CHUNK_SIZE) * Node.MIN_CHUNK_SIZE;

            if (_lastPos != null && roundedX == _lastPos.Value.X && roundedZ == _lastPos.Value.Y)
                return;

            Node rootNode = new Node(null, new Vector2D<int>(roundedX, roundedZ), 4096);

            rootNode.Recurse(new Vector3D<int>(roundedX, 0, roundedZ), this);

            _toLoad = _toLoad.OrderBy(x => x.Size).ToList();

            _lastActiveNodes = _activeNodes.ToList();
            _activeNodes.Clear();

            while(_toLoad.Count() > 0)
            {
                var node = _toLoad.First();
                _toLoad.Remove(node);
                if (_lastActiveNodes.Contains(node))
                {
                    node.AssociatedEntity = _lastActiveNodes.First(x => x == node).AssociatedEntity;
                    _lastActiveNodes.Remove(node);
                    _activeNodes.Add(node);
                    continue;
                }

                TerrainChunkComponent? chunkComp = null;
                Entity? ent = null;
                if (_pool.TryDequeue(out ent) == false)
                {
                    ent = Scene.EntityManager.CreateEntity(node.ToString());

                    chunkComp = ent.AddComponent<TerrainChunkComponent>();

                    //var mesh = ent.AddComponent<MeshRendererComponent>();
                    //mesh.Mesh = _planeMesh;
                    //mesh.Material = new Material("QuadTreeChunk");

                    //var col = _colors[node.Size];
                    //mesh.Material.SetColor("Colour", Color.FromArgb((int)(col.X * 255), (int)(col.Y * 255), (int)(col.Z * 255)));
                }
                else
                {
                    ent.IsActive = true;
                    //var mesh = ent.GetComponent<MeshRendererComponent>();
                    //var col = _colors[node.Size];
                    //mesh.Material?.SetColor("Colour", Color.FromArgb((int)(col.X * 255), (int)(col.Y * 255), (int)(col.Z * 255)));

                    chunkComp = ent.GetComponent<TerrainChunkComponent>()!;
                }

                chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                chunkComp.State = TerrainChunkComponent.TerrainState.None;
                chunkComp.IsPooled = false;

                ent.Transform.Position = new Vector3D<float>(node.Position.X, 0, node.Position.Y);
                ent.Transform.LocalEulerAngles = new Vector3D<float>(0, 0, 0);
                ent.Transform.LocalScale = new Vector3D<float>(node.Size / 16, 1, node.Size / 16);

                node.AssociatedEntity = ent;

                _activeNodes.Add(node);

                //HandleNeighbours(chunkComp, (int)node.Position.X, (int)node.Position.Y);
            }

            foreach (var node in _lastActiveNodes)
            {
                if (node.AssociatedEntity != null && _pool.Contains(node.AssociatedEntity) == false)
                {
                    node.AssociatedEntity.IsActive = false;

                    var chunk = node.AssociatedEntity.GetComponent<TerrainChunkComponent>()!;
                    if (chunk.NorthNeighbour != null)
                    {
                        chunk.NorthNeighbour.SouthNeighbour = null;
                        chunk.NorthNeighbour = null;
                    }
                    if (chunk.RightNeighbour != null)
                    {
                        chunk.RightNeighbour.LeftNeighbour = null;
                        chunk.RightNeighbour = null;
                    }
                    if (chunk.LeftNeighbour != null)
                    {
                        chunk.LeftNeighbour.RightNeighbour = null;
                        chunk.LeftNeighbour = null;
                    }
                    if (chunk.SouthNeighbour != null)
                    {
                        chunk.SouthNeighbour.NorthNeighbour = null;
                        chunk.SouthNeighbour = null;
                    }

                    chunk.State = TerrainChunkComponent.TerrainState.None;
                    chunk.IsPooled = true;
                    chunk.VoxelData = null;
                    chunk.Heightmap = null;

                    TerrainChunkBatcherRenderer.QueueChunkForBatching(chunk);
                    _pool.Enqueue(node.AssociatedEntity);
                }
            }
            _lastActiveNodes.Clear();

            _lastPos = new Vector2D<int>(roundedX, roundedZ);

        }

        void HandleNeighbours(TerrainChunkComponent chunkComp, int xF, int zF)
        {
            using (Profiler.BeginEvent($"HandleNeighbours for {chunkComp}"))
            {
                var neighbours = _sceneChunkListReference.Where(x =>
                (int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH ||

                (int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH ||

                (int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF ||

                (int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF);

                chunkComp.NorthNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH) as TerrainChunkComponent ?? chunkComp.NorthNeighbour;

                chunkComp.SouthNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH) as TerrainChunkComponent ?? chunkComp.SouthNeighbour;

                chunkComp.LeftNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF) as TerrainChunkComponent ?? chunkComp.LeftNeighbour;

                chunkComp.RightNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF) as TerrainChunkComponent ?? chunkComp.RightNeighbour;

                if (chunkComp.NorthNeighbour != null)
                {
                    chunkComp.NorthNeighbour.SouthNeighbour = chunkComp;
                }
                if (chunkComp.RightNeighbour != null)
                {
                    chunkComp.RightNeighbour.LeftNeighbour = chunkComp;
                }
                if (chunkComp.LeftNeighbour != null)
                {
                    chunkComp.LeftNeighbour.RightNeighbour = chunkComp;
                }
                if (chunkComp.SouthNeighbour != null)
                {
                    chunkComp.SouthNeighbour.NorthNeighbour = chunkComp;
                }
            }
        }
    }
}
