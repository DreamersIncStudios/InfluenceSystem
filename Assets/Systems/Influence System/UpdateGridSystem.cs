using DreamersInc.InfluenceMapSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.InfluenceMapSystem
{

    public partial class UpdateGridSystem : SystemBase
    {
        
        protected override void OnUpdate()
        {
            new MyStruct().Schedule();
        }
  
        partial struct MyStruct: IJobEntity
        {
            public NativeArray<InfluenceComponent> Influence;
            public NativeArray<LocalToWorld> Transforms;
            void Execute(Entity entity, ref GridManager manager)
            {
                foreach (var node in manager.Nodes)
                {
                    int sectorMask = 0;
                    if (!node.Value.IsWalkable) continue;
                    for (int i = 0; i < Transforms.Length; i++)
                    {
                        var dist = Vector3.Distance(node.Value.Position, Transforms[i].Position);  
                        var direction = ((Vector3)node.Value.Position - (Vector3)Transforms[i].Position).normalized;

                        if(dist > Influence[i].DetectionRadius) continue;
                        if (IsObjectBlocked(Transforms[i].Position, node.Value.Position)) continue;
                        int sector = GetSectorForDirection(direction);
                        int rangeValue = GetRangeValue(dist, Influence[i].DetectionRadius);
                        int sectorShift = sector * 4;
                        int currentSectorValue = (sectorMask >> sectorShift) & 0b1111;
                        int newSectorValue = Mathf.Min(15, currentSectorValue + rangeValue);
                        sectorMask &= ~(0b1111 << sectorShift);
                        sectorMask |= (newSectorValue << sectorShift);
                    }
                    manager.NodeSectorData[node.Value.Position] = sectorMask; 

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