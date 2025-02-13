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
        
        class baker:Baker<GridManagerAuthoring> {
            
            public override void Bake(GridManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new GridManagerData(){
                    Center = authoring.Center,
                    CellSize = authoring.CellSize,
                    GridSizeY = authoring.GridSizeY,
                    GridSizeX =  authoring.GridSizeX
                });
                var buffer = AddBuffer<GridNode>(entity).InitializeHashMap<GridNode, int2, Node>();
                AddBuffer<SectorNodes>(entity).InitializeHashMap<SectorNodes, float3, int>();
                var hashMap = buffer.AsHashMap<GridNode, int2, Node>();
                var startingPosition = authoring.Center - new float3(authoring.GridSizeX * authoring.CellSize / 2, 0, authoring.GridSizeY * authoring.CellSize / 2);

               
          
                    for (var x = 0; x < authoring.GridSizeX; x++)
                    {
                        for (var y = 0; y < authoring.GridSizeY; y++)
                        {
                            var position = startingPosition +
                                           new float3(x * authoring.CellSize, 0, y * authoring.CellSize);
                            hashMap.Add(new int2(x, y), new Node(FactionNames.Citizen, position, true));
                        }
                    }


                    return;

                bool IsPositionOnNavMesh(float3 position)=> NavMesh.SamplePosition(position, out _, authoring.CellSize/2, NavMesh.AllAreas);
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