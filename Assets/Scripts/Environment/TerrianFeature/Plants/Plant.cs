using System;
using UnityEngine;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    public abstract class Plant : ScriptableObject
    {
        public float noiseZoneScale;
        [Range(0.1f, 1f)]
        public float noiseZoneThreshold;
        public float noisePlacementScale;
        [Range(0.1f, 1f)]
        public float noisePlacementThreshold;
        public abstract BlockEdit[] Generate(Vector3Int placePosInWorld);
    }
}
