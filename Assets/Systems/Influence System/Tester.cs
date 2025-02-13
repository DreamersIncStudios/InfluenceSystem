using DreamersInc.InfluenceMapSystem;
using Unity.Entities;
using UnityEngine;

public class Tester : MonoBehaviour
{
    private class TesterBaker : Baker<Tester>
   {
      public override void Bake(Tester authoring)
      {
          var entity = GetEntity(TransformUsageFlags.WorldSpace);
          AddComponent(entity, new InfluenceComponent(1, 10,10, 25));
      }
   }
}
