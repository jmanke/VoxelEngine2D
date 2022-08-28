namespace Hazel.VoxelEngine2D.Data
{
    [System.Serializable]
    public class ChunkData
    {
        public int Size { get; private set; }

        public VoxelData[,] Voxels { get; private set; }

        public ChunkData(int size) : this(size, new VoxelData[size, size]) { }

        public ChunkData(int size, VoxelData[,] voxels)
        {
            this.Size = size;
            this.Voxels = voxels;
        }
    }
}
