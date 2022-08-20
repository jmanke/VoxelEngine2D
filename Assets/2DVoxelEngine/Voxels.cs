using System.Collections.Generic;
using Hazel.VoxelEngine2D.Unity;
using UnityEngine;

namespace Hazel.VoxelEngine2D
{
    public class Voxels
    {
        public static FastNoiseUnity FastNoise = new();

        private static Dictionary<Vector2Int, Voxel> cachedVoxels = new Dictionary<Vector2Int, Voxel>();

        public static Voxel At(int x, int y)
        {
            if (cachedVoxels.TryGetValue(new Vector2Int(x, y), out var voxel))
            {
                return voxel;
            }

            if (y < 10)
            {
                return VoxelEngine.VoxelDefinitions[3];
            }

            if (y < 40)
            {
                return VoxelEngine.VoxelDefinitions[2];
            }

            return FastNoise.fastNoise.GetNoise(x, y) < 0.5f ? VoxelEngine.VoxelDefinitions[1] : VoxelEngine.VoxelDefinitions[0];
        }

        public static void Set(int x, int y, Voxel voxel)
        {
            cachedVoxels[new Vector2Int(x, y)] = voxel;
        }
    }
}
