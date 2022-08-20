using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using Hazel.VoxelEngine.Data;
using Hazel.VoxelEngine.Unity;

namespace Hazel.VoxelEngine
{
    /// <summary>
    /// Entry point for the 2D voxel engine
    /// </summary>
    public class VoxelEngine2D : MonoBehaviour
    {
        [Tooltip("Path to voxel json definition in a Resources folder")]
        public string voxelAssetPath = "VoxelEngine2D/voxels";

        [Header("Tile sheet")]

        [Tooltip("Tile sheet for voxels")]
        public Sprite tileSheet;

        public Material material;

        // sparse array of definitions for voxels
        public static Voxel[] VoxelDefinitions { get; private set; }

        public static float TileSize { get; } = 0.0625f;

        public static int ChunkSize;

        private static FlatArray2D<Chunk> chunks;
        private static TerrainProfileData terrainProfile;

        public void Awake()
        {
            terrainProfile = new TerrainProfileData();
            ChunkSize = terrainProfile.ChunkSize;
            this.LoadVoxelDefinitions();
        }

        public void Start()
        {
            chunks = new FlatArray2D<Chunk>(terrainProfile.WorldWidth, terrainProfile.WorldHeight);

            for (int i = 0; i < terrainProfile.WorldWidth; i++)
            {
                for (int j = 0; j < terrainProfile.WorldHeight; j++)
                {
                    var pos = new Vector2Int(i * terrainProfile.ChunkSize, j * terrainProfile.ChunkSize);
                    var chunk = new Chunk(this.material, pos);

                    chunk.Update();

                    chunks.Set(i, j, chunk);
                }
            }
        }

        public static void UpdateChunk(Vector2 position)
        {
            var coord = new Vector2Int((int)position.x / terrainProfile.ChunkSize, (int)position.y / terrainProfile.ChunkSize);
            var chunk = chunks.Get(coord.x, coord.y);
            chunk.Update();
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="coord">Coordinate of the voxel</param>
        /// <param name="voxel">Voxel to set at coordinate</param>
        public static void UpdateVoxel(Vector2Int coord, Voxel voxel)
        {
            TerrainBuilder.SetVoxel(coord.x, coord.y, voxel);
            UpdateChunk(new Vector2(coord.x, coord.y));

            // check if a neighbour chunk also needs to be updated
            int chunkX = coord.x % terrainProfile.ChunkSize;
            int chunkY = coord.y % terrainProfile.ChunkSize;

            // left
            if (chunkX == 0)
            {
                UpdateChunk(new Vector2(coord.x - 1, coord.y));
            }
            // right
            else if (chunkX == terrainProfile.ChunkSize - 1)
            {
                UpdateChunk(new Vector2(coord.x + 1, coord.y));
            }

            // bottom
            if (chunkY == 0)
            {
                UpdateChunk(new Vector2(coord.x, coord.y - 1));
            }
            // top
            else if (chunkY == terrainProfile.ChunkSize - 1)
            {
                UpdateChunk(new Vector2(coord.x, coord.y + 1));
            }
        }

        /// <summary>
        /// Loads voxels
        /// </summary>
        private void LoadVoxelDefinitions()
        {
            var jsonText = Resources.Load<TextAsset>(this.voxelAssetPath);
            if (jsonText == null)
            {
                throw new System.Exception($"Could not load voxels at {this.voxelAssetPath}");
            }

            var voxels = JsonConvert.DeserializeObject<Voxel[]>(jsonText.ToString());

            int maxId = voxels.Max(voxel => voxel.Id);

            // to keep voxel definition lookup as fast as possible, we use a sparse array to store them.
            // Array index corresponds to the voxel's id, so we're sacrificing space for lookup time.
            VoxelDefinitions = new Voxel[maxId + 1];

            foreach (var voxel in voxels)
            {
                VoxelDefinitions[voxel.Id] = voxel;
            }
        }
    }
}
