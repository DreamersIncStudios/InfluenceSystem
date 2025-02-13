using BovineLabs.Core.Iterators;
using DreamersInc.InfluenceMapSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.InfluenceMapSystem
{
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(32, true);

        }
    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct UpdateGridSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<InfluenceComponent, LocalToWorld>().Build();
            new MyStruct()
            {
                Influence = query.ToComponentDataArray<InfluenceComponent>(allocator: Allocator.TempJob),
                Transforms = query.ToComponentDataArray<LocalToWorld>(allocator: Allocator.TempJob)
            }.Schedule();
        }
  [BurstCompile]
        partial struct MyStruct: IJobEntity
        {
            public NativeArray<InfluenceComponent> Influence;
            public NativeArray<LocalToWorld> Transforms;
            void Execute(Entity entity,ref GridManagerData data, DynamicBuffer<GridNode> gridNodes, DynamicBuffer<SectorNodes> sectorNode)
            {
                var nodes = gridNodes.AsHashMap<GridNode, int2, Node>();
                var NodeSectorData = sectorNode.AsHashMap<SectorNodes, float3, int>();

                for (int x = 0; x < data.GridSizeX; x++)
                {
                    for (int y = 0; y < data.GridSizeY; y++)
                    {
                        var index = new int2(x, y);
                        int sectorMask = 0;
                        if (!nodes[index].IsWalkable) continue;
                        for (var i = 0; i < Transforms.Length; i++)
                        {
                            var dist = Vector3.Distance(nodes[index].Position, Transforms[i].Position);
                            var direction = ((Vector3)nodes[index].Position - (Vector3)Transforms[i].Position)
                                .normalized;

                            if (dist > Influence[i].DetectionRadius) continue;
                            if (IsObjectBlocked(Transforms[i].Position, nodes[index].Position)) continue;
                            int sector = GetSectorForDirection(direction);
                            int rangeValue = GetRangeValue(dist, Influence[i].DetectionRadius);
                            int sectorShift = sector * 4;
                            int currentSectorValue = (sectorMask >> sectorShift) & 0b1111;
                            int newSectorValue = Mathf.Min(15, currentSectorValue + rangeValue);
                            sectorMask &= ~(0b1111 << sectorShift);
                            sectorMask |= (newSectorValue << sectorShift);
                        }

                        NodeSectorData[nodes[index].Position] = sectorMask;

                    }
                }
            }

            int GetRangeValue(float dist, float DetectionRadius)
            {
                if (dist < DetectionRadius * .5f) return 3;
                if (dist < DetectionRadius * .75f) return 2;
                return dist <= DetectionRadius ? 1 : 0;
            }

            private int GetSectorForDirection(Vector3 direction)
            {
                return Mathf.FloorToInt((Mathf.Atan2(direction.z, direction.x)* Mathf.Rad2Deg+360)%360/45);
            }

            private bool IsObjectBlocked(float3 enemyPosition, float3 nodePosition)
            {
                return false; // TODO: Add dots physics 
            }
        }

    }
}