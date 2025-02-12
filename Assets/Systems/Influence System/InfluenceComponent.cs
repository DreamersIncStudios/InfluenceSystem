using DreamersIncStudio.FactionSystem;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace DreamersInc.InfluenceMapSystem
{

    [System.Serializable]
    public struct InfluenceComponent : IComponentData
    {
         
        public FactionNames FactionID;
        public float DetectionRadius;
        public int Threat { get; private set; }
        public  int Protection { get; private set; }
        public InfluenceComponent(int factionID,int threat, int protection, float detectionRadius)
        {
            Threat = threat;
            Protection = protection;
            this.FactionID = (FactionNames)factionID;
            DetectionRadius = detectionRadius;
        }


 
     
   
    }
}