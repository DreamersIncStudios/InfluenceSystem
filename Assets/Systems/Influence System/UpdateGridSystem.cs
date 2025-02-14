using System.Collections.Generic;
using BovineLabs.Core.Iterators;
using DreamersInc.InfluenceMapSystem;
using DreamersIncStudio.FactionSystem;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace DreamersIncStudio.InfluenceMapSystem
{
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(64, true);

        }
    }

   [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial class UpdateGridSystem : SystemBase
    {
        public int gridSizeX = 700;
        public int gridSizeZ = 700;
        public float cellSize = 1.0f;

        Node[,] grid;

        // A sector represents an 8-way directional segment (45-degree slices) 
        // around a grid node, used to track enemy influence and danger levels in different directions.
        readonly Dictionary<Vector3, int> nodeSectorData = new(); // Each position will map to an 8-sector bitmask

        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / cellSize);
            int z = Mathf.RoundToInt(worldPosition.z / cellSize);
            x = Mathf.Clamp(x, 0, gridSizeX - 1); // Clamp to grid bounds.
            z = Mathf.Clamp(z, 0, gridSizeZ - 1); // Clamp to grid bounds.
            return grid[x, z];
        }

        public List<Node> GetNodesInRange(float3 position, float range)
        {
            var index = range / 2;
            var startPosition = position - new float3(index, 0, index);
            var endPosition = position + new float3(index, 0, index);
            var nodesInRange = new List<Node>();
            for (int x = Mathf.RoundToInt(startPosition.x / cellSize); x <= Mathf.RoundToInt(endPosition.x / cellSize); x++)
            for (int z = Mathf.RoundToInt(startPosition.z / cellSize); z <= Mathf.RoundToInt(endPosition.z / cellSize); z++)
            {
                var node = grid[x, z];
                if (node.IsWalkable) nodesInRange.Add(node);
            }
            return nodesInRange;
            
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            InitializeGrid();
        }

        protected override void OnUpdate()
        {
            PrecomputeSectors();
            if (nodeSectorData.TryGetValue(new float3(40, 0, 40), out var sectorMask))
            {
                Debug.Log(sectorMask);
            }
            else
            {
                Debug.Log("No sector data found");
            }

        }
        void PrecomputeSectors()
        {
         nodeSectorData.Clear();

            var enemiesQuery = SystemAPI.QueryBuilder().
                WithAll<LocalToWorld,InfluenceComponent>()
                .Build(); 
            var enemyLocations = enemiesQuery.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            foreach (var enemy in enemyLocations)
            {
                var nodes = GetNodesInRange(enemy.Position, 30);
                foreach (var node in nodes)
                {
                    int sectorMask = 0;
                    float detectionRadius = 25;
                    Vector3 direction = ((Vector3)node.Position - (Vector3)enemy.Position).normalized;
                    float distance = Vector3.Distance(node.Position, enemy.Position);
                
                    if (distance > detectionRadius) continue;
                    if (IsObstructed(enemy.Position, node.Position + new float3(0.5f, 0, 0.5f))) continue;
                
                    int sector = GetSectorForDirection(direction); 
                    int rangeValue = GetRangeValue(distance, detectionRadius);
                    int sectorShift = sector * 4;
                    int currentSectorValue = (sectorMask >> sectorShift) & 0b1111;
                    int newSectorValue = Mathf.Min(15, currentSectorValue + rangeValue);
                    sectorMask &= ~(0b1111 << sectorShift);
                    sectorMask |= newSectorValue << sectorShift;
                
                 nodeSectorData[node.Position] = sectorMask;
                }
            }
            enemyLocations.Dispose();
        }
        void InitializeGrid()
        {
            grid = new Node[gridSizeX, gridSizeZ];
            for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeZ; z++)
            {
                var pos = new Vector3(x, 0, z) * cellSize;
                grid[x, z] = new Node(FactionNames.Citizen, pos, true);
            }
        }
        int GetRangeValue(float distance, float detectionRadius) {
            if (distance < detectionRadius * 0.5f) return 3;
            if (distance < detectionRadius * 0.75f) return 2;
            if (distance <= detectionRadius) return 1;
            return 0;
        }

        int GetSectorForDirection(Vector3 direction) {
            return Mathf.FloorToInt((Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg + 360) % 360 / 45f);
        }
        bool IsObstructed(Vector3 from, Vector3 to)
        {
            return false;
        }

        bool IsPositionOnNavMesh(Vector3 position) =>
            NavMesh.SamplePosition(position, out _, cellSize / 2, NavMesh.AllAreas);

    }
}