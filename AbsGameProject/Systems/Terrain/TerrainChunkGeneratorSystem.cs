﻿using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Jobs;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainChunkGeneratorSystem : AbsEngine.ECS.System
    {
        readonly List<TerrainChunkComponent> CHUNK_POOL = new();
        readonly List<TerrainChunkComponent> ACTIVE_CHUNKS = new();

        const int RADIUS = 32;
        const float LOD_SCALE = 2f;

        float lastX;
        float lastZ;
        bool hasBeenInitialised = false;

        TransformComponent? _mainCam;
        ChunkBuildJobState? activeJobState;

        public override bool UseJobSystem => false;

        public TerrainChunkGeneratorSystem(Scene scene) : base(scene)
        {
            var rh = RADIUS / 2;
            Shader.SetGlobalFloat("FogMaxDistance", (rh) * TerrainChunkComponent.WIDTH);
            Shader.SetGlobalFloat("FogMinDistance", (rh - 5) * TerrainChunkComponent.WIDTH);

            _mainCam = Scene.EntityManager.GetComponents<CameraComponent>().FirstOrDefault(x => x.IsMainCamera)?.Entity.Transform;
        }

        public override void OnTick(float deltaTime)
        {
            if (_mainCam == null)
                throw new InvalidOperationException("Main Camera object not found");

            if (activeJobState != null && activeJobState.IsComplete == false)
                return;

            var roundedX = (int)MathF.Floor(_mainCam.Entity.Transform.Position.X / TerrainChunkComponent.WIDTH) * TerrainChunkComponent.WIDTH;
            var roundedZ = (int)MathF.Floor(_mainCam.Entity.Transform.Position.Z / TerrainChunkComponent.WIDTH) * TerrainChunkComponent.WIDTH;

            if (hasBeenInitialised && roundedX == lastX && roundedZ == lastZ)
                return;

            lastX = roundedX;
            lastZ = roundedZ;

            ACTIVE_CHUNKS.Clear();
            hasBeenInitialised = true;

            activeJobState = new ChunkBuildJobState();

            Scene.Game.Scheduler.Schedule(
                new ChunkBuildJob(RADIUS, roundedX, roundedZ, ACTIVE_CHUNKS, CHUNK_POOL, Scene, activeJobState, LOD_SCALE));

            Scene.Game.Scheduler.Flush();
        }
    }
}
