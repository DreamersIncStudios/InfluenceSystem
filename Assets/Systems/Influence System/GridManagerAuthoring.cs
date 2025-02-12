using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;


namespace DreamerIncStudio.InfluenceMapSystem{
    
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
            }
        }
    }
    [Serializable]
    public struct Node
    {
        public float3 Position;
        public bool IsWalkable;

        public Node(float3 position, bool isWalkable = false)
        {
            Position = position;
            IsWalkable = isWalkable;
        }
    }

    public struct GridManager : IComponentData
    {
        public float3 Center;
        public int GridSizeX, GridSizeY;
        public float CellSize;
        public NativeParallelMultiHashMap<int2, Node> Nodes;  
        public NativeHashMap<float3, int> NodeSectorData; // < Position, Index>

        public GridManager(GridManagerData data)
        {
            Center = data.Center;
            GridSizeX = data.GridSizeX;
            GridSizeY = data.GridSizeY;
            CellSize = data.CellSize;
            Nodes = new NativeParallelMultiHashMap<int2, Node>(Mathf.CeilToInt(GridSizeX*GridSizeY/CellSize), Allocator.Persistent);
            NodeSectorData = new NativeHashMap<float3, int>(8, Allocator.Persistent);
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            var startingPosition = Center - new float3(GridSizeX * CellSize / 2, 0, GridSizeY * CellSize / 2);
            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = 0; y < GridSizeY; y++)
                {
                    var position = startingPosition + new float3(x * CellSize, 0, y * CellSize);
                    Nodes.Add(new int2(x, y), new Node(position, isPositionOnNavMesh(position)));
                }
            }
        }

        private bool isPositionOnNavMesh(float3 position)=> NavMesh.SamplePosition(position, out _, CellSize/2, NavMesh.AllAreas);
    }

    public struct GridManagerData : IComponentData // This get baked in editor. Baking system converts to GridManager
    {
        public float3 Center;
        public int GridSizeX, GridSizeY;
        public float CellSize;
    }

     partial struct AddGridBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<GridManagerData>()
                .Build();
            state.EntityManager.AddComponent<GridManager>(query);
            foreach (var (manager, data) in SystemAPI.Query<RefRW<GridManager>, GridManagerData>())
            {
                manager.ValueRW = new GridManager(data);
            }
            state.EntityManager.RemoveComponent<GridManagerData>(query);


        }
    }
}