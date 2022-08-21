namespace Hazel.VoxelEngine2D.Data
{
    [System.Serializable]
    public class ChunkData
    {
        public VoxelData[] Voxels { get; private set; }

        public ChunkData(int size) : this(new VoxelData[size * size]) { }

        public ChunkData(VoxelData[] voxels)
        {
            this.Voxels = voxels;
        }
    }
}
