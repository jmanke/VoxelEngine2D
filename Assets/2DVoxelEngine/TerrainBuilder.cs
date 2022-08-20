using Hazel.VoxelEngine.Unity;

namespace Hazel.VoxelEngine
{
    public class TerrainBuilder
    {
        public static FastNoiseUnity FastNoise = new();

        public static Voxel VoxelAt(int x, int y)
        {
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
    }
}
