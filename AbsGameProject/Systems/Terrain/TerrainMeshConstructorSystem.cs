﻿using AbsEngine;
using AbsEngine.ECS;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Jobs;
using AbsGameProject.Models.Meshing;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainMeshConstructorSystem : ComponentSystem<TerrainChunkComponent>
    {
        protected override int MaxIterationsPerFrame => 50;

        public override bool UseJobSystem => false;

        public TerrainMeshConstructorSystem(Scene scene) : base(scene)
        {
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.IsReadyForMeshGeneration == false 
                && ((component.State == TerrainChunkComponent.TerrainState.Done && component.IsAwaitingRebuild) == false))
                return;

            component.IsAwaitingRebuild = false;

            Scene.Game.Scheduler.Schedule(new ChunkMeshBuildJob(component));
            Scene.Game.Scheduler.Flush();
        }
    }
}
