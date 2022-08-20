namespace Hazel.VoxelEngine2D.Data
{
    public class ChunkData
    {
        public int SizeX { get; private set; }

        public int SizeY { get; private set; }

        public FlatArray2D<VoxelData> Voxels { get; private set; }

        public ChunkData(int sizeX, int sizeY) : this(new FlatArray2D<VoxelData>(sizeX, sizeY)) { }

        public ChunkData(FlatArray2D<VoxelData> voxels)
        {
            this.SizeX = voxels.Width;
            this.SizeY = voxels.Height;
            this.Voxels = voxels;
        }
    }
}
