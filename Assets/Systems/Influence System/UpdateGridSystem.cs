using DreamersInc.InfluenceMapSystem;
using Unity.Collections;
using Unity.Entities;
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
                    if (!node.Value.IsWalkable) continue;
                    for (int i = 0; i < Transforms.Length; i++)
                    {
                        var dist = Vector3.Distance(node.Value.Position, Transforms[i].Position);  
                        if(dist > Influence[i].DetectionRadius) continue;
                        var direction = ((Vector3)node.Value.Position - (Vector3)Transforms[i].Position).normalized;
                    }


                }
            }
            
            int GetSectorForDirection(Vector3 direction)
            {
                return Mathf.FloorToInt((Mathf.Atan2(direction.z, direction.x)* Mathf.Rad2Deg+360)%360/45);
            }
        }

    }
}