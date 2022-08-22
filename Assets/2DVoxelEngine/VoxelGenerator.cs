using Hazel.VoxelEngine2D.Unity;

namespace Hazel.VoxelEngine2D
{
    public class VoxelGenerator
    {
        private readonly FastNoiseUnity fastNoise = new();

        public Voxel VoxelAt(int x, int y)
        {
            if (y < 10)
            {
                return VoxelEngine.VoxelDefinitions[3];
            }

            if (y < 40)
            {
                return VoxelEngine.VoxelDefinitions[2];
            }

            return fastNoise.fastNoise.GetNoise(x, y) < 0.5f ? VoxelEngine.VoxelDefinitions[1] : VoxelEngine.VoxelDefinitions[0];
        }
    }

}