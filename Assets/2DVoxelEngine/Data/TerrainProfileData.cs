namespace Hazel.VoxelEngine.Data
{
    [System.Serializable]
    public class TerrainProfileData
    {
        /// <summary>
        /// Measured in chunks
        /// </summary>
        public int WorldHeight { get; set; } = 16;

        /// <summary>
        /// Measured in chunks
        /// </summary>
        public int WorldWidth { get; set; } = 16 * 4;


        /// <summary>
        /// Measured in tiles
        /// </summary>
        public int ChunkSize { get; set; } = 16;
    }
}
