using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Parameters;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Резолвер для определения материала на Unity Terrain.
    /// Анализирует AlphaMaps (SplatMaps) и выбирает наиболее "весомый" слой в точке попадания.
    /// </summary>
    public class SurfaceTerrainResolver<T> : BaseSurfaceResolver<T>
    {
        private readonly WwiseSwitchMaping<T> _wwiseSwitchMaping;

        public SurfaceTerrainResolver(BaseSurfaceResolverConfig<T> config) : base(config)
        {
            _wwiseSwitchMaping = new(_config.WwiseToMaterial);
        }

        public override bool TryResolve(RaycastHit[] raycastHits, ref T resolve)
        {
            if (raycastHits.Length == 0) 
                return false;

            GameObject hitObject = GetClosestGameObject(raycastHits);

            if (hitObject == null) 
                return false;

            if(!hitObject.TryGetComponent<Terrain>(out Terrain terrain) || terrain.terrainData == null)
                return false;
            

            RaycastHit hit = GetClosestRaycastHit(raycastHits);
            int layerIndex = GetMainTerrainTexture(hit.point, terrain);

            if (layerIndex >= terrain.terrainData.terrainLayers.Length) 
                return false;

            TerrainLayer layer = terrain.terrainData.terrainLayers[layerIndex];

            foreach (var pair in _wwiseSwitchMaping.Map)
            {
                Material mat = pair.Key;

                if (mat != null && mat.mainTexture == layer.diffuseTexture)
                {
                    LastDebugReason = $"Found Material by Terrain Texture: {mat.name}";
                    resolve = LastDetectedSurface = pair.Value;

                    return true;
                }
            }

            LastDebugReason = "No matching Material found for Terrain Layer texture";
            return false;
        }

        private int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
        {
            TerrainData terrainData = terrain.terrainData;
            float mapX = (worldPos.x - terrain.transform.position.x) / terrainData.size.x;
            float mapZ = (worldPos.z - terrain.transform.position.z) / terrainData.size.z;

            mapX = Mathf.Clamp01(mapX);
            mapZ = Mathf.Clamp01(mapZ);

            int x = Mathf.FloorToInt(mapX * (terrainData.alphamapWidth - 1));
            int z = Mathf.FloorToInt(mapZ * (terrainData.alphamapHeight - 1));

            float[,,] splatmapData = terrainData.GetAlphamaps(x, z, 1, 1);

            float maxMix = 0;
            int maxIndex = 0;

            for (int n = 0; n < splatmapData.GetLength(2); n++)
            {
                if (splatmapData[0, 0, n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = splatmapData[0, 0, n];
                }
            }
            return maxIndex;
        }
    }
}