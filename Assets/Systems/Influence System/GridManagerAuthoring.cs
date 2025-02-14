using System;
using System.Collections.Generic;
using BovineLabs.Core.Iterators;
using DreamersIncStudio.FactionSystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;


namespace DreamersIncStudio.InfluenceMapSystem{
    
    public class GridManagerAuthoring : MonoBehaviour
    {

        [SerializeField] private float3 Center=> transform.position;
        [SerializeField] private int GridSizeX, GridSizeY = 10;
        [SerializeField] float CellSize=1;

        class baker : Baker<GridManagerAuthoring>
        {

            public override void Bake(GridManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new GridManagerData()
                {
                    Center = authoring.Center,
                    CellSize = authoring.CellSize,
                    GridSizeY = authoring.GridSizeY,
                    GridSizeX = authoring.GridSizeX
                });

            }
        }
    }
    [Serializable]
    public struct Node
    {
        public FactionNames GridFor;
        public float3 Position;
        public bool IsWalkable;

        public Node(FactionNames faction, float3 position, bool isWalkable = false)
        {
            GridFor = faction;
            Position = position;
            IsWalkable = isWalkable;
        }
    }

    [InternalBufferCapacity(0)]
    public struct GridNode : IDynamicHashMap<int2, Node>
    {

        byte IDynamicHashMap<int2, Node>.Value { get; }
   

    }
    [InternalBufferCapacity(0)]
    public struct SectorNodes : IDynamicHashMap<float3, int>
    {

        byte IDynamicHashMap<float3, int>.Value { get; }
   

    }

    public struct GridManagerData : IComponentData // This get baked in editor. Baking system converts to GridManager
    {
        public float3 Center;
        public int GridSizeX, GridSizeY;
        public float CellSize;
    }

}