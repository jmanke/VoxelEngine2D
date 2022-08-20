using System.Collections.Generic;
using Hazel.VoxelEngine.Unity;
using UnityEngine;

namespace Hazel.VoxelEngine
{
    public class TerrainBuilder
    {
        public static FastNoiseUnity FastNoise = new();

        private static Dictionary<Vector2Int, Voxel> cachedVoxels = new Dictionary<Vector2Int, Voxel>();

        public static Voxel VoxelAt(int x, int y)
        {
            if (cachedVoxels.TryGetValue(new Vector2Int(x, y), out var voxel))
            {
                return voxel;
            }

            if (y < 10)
            {
                return VoxelEngine2D.VoxelDefinitions[3];
            }

            if (y < 40)
            {
                return VoxelEngine2D.VoxelDefinitions[2];
            }

            return FastNoise.fastNoise.GetNoise(x, y) < 0.5f ? VoxelEngine2D.VoxelDefinitions[1] : VoxelEngine2D.VoxelDefinitions[0];
        }

        public static void SetVoxel(int x, int y, Voxel voxel)
        {
            cachedVoxels[new Vector2Int(x, y)] = voxel;
        }
    }
}
