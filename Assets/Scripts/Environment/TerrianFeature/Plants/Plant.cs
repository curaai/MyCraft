using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    public abstract class Plant : ScriptableObject
    {
        public float NoiseZoneScale;
        [Range(0.1f, 1f)]
        public float NoiseZoneThreshold;
        public float NoisePlacementScale;
        [Range(0.1f, 1f)]
        public float NoisePlacementThreshold;
        public abstract List<BlockEdit> Generate(Vector3Int placePos);
    }
}
