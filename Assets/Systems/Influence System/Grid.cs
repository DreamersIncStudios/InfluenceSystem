using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace DreamerIncStudio.InfluenceMapSystem{
    
    public class Grid : MonoBehaviour
    {

        [SerializeField] private float3 Center=> transform.position;
        [SerializeField] private int GridSizeX, GridSizeY = 10;
        [SerializeField] float CellSize=1;
        
        class baker:Baker<Grid> {
            public override void Bake(Grid authoring)
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
    }

    public struct GridManagerData : IComponentData // This get baked in editor. Baking system converts to GridManager
    {
        public float3 Center;
        public int GridSizeX, GridSizeY;
        public float CellSize;
    }

}