using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using Hazel.VoxelEngine2D.Data;
using Hazel.VoxelEngine2D.Unity;

namespace Hazel.VoxelEngine2D
{
    /// <summary>
    /// Entry point for the 2D voxel engine
    /// </summary>
    public class VoxelEngine : MonoBehaviour
    {
        public static VoxelEngine Instance { get; private set; }

        [Tooltip("Path to voxel json definition in a Resources folder")]
        public string voxelAssetPath = "VoxelEngine2D/voxels";

        [Header("Tile sheet")]

        [Tooltip("Tile sheet for voxels")]
        public Sprite tileSheet;

        public Material material;

        // sparse array of definitions for voxels
        public static Voxel[] VoxelDefinitions { get; private set; }

        public float TileSize { get; private set; } = 0.0625f;

        public int ChunkSize { get; private set; }

        private FlatArray2D<Chunk> chunks;
        private WorldProfileData worldProfile;
        private Voxels voxels;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            this.worldProfile = new WorldProfileData();
            this.ChunkSize = this.worldProfile.ChunkSize;
            this.LoadVoxelDefinitions();
            this.voxels = new Voxels(this.ChunkSize, this.material);
        }

        public void Start()
        {
            this.chunks = new FlatArray2D<Chunk>(worldProfile.WorldWidth, worldProfile.WorldHeight);

            for (int i = 0; i < this.worldProfile.WorldWidth; i++)
            {
                for (int j = 0; j < this.worldProfile.WorldHeight; j++)
                {
                    var chunk = new Chunk(this.material, new Vector2Int(i, j));

                    chunk.Update();
                    this.chunks.Set(i, j, chunk);
                }
            }
        }

        /// <summary>
        /// Updates chunk at world position
        /// </summary>
        /// <param name="position">world position</param>
        public void UpdateChunk(Vector2 position)
        {
            var coord = new Vector2Int((int)position.x / this.ChunkSize, (int)position.y / this.ChunkSize);
            var chunk = this.chunks.Get(coord.x, coord.y);
            chunk.Update();
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="coord">Coordinate of the voxel</param>
        /// <param name="voxel">Voxel to set at coordinate</param>
        public void UpdateVoxel(Vector2Int coord, Voxel voxel)
        {
            this.voxels.Set(coord.x, coord.y, voxel);
            this.UpdateChunk(new Vector2(coord.x, coord.y));

            // check if a neighbour chunk also needs to be updated
            int chunkX = coord.x % this.worldProfile.ChunkSize;
            int chunkY = coord.y % this.worldProfile.ChunkSize;

            // left
            if (chunkX == 0)
            {
                this.UpdateChunk(new Vector2(coord.x - 1, coord.y));
            }
            // right
            else if (chunkX == worldProfile.ChunkSize - 1)
            {
                this.UpdateChunk(new Vector2(coord.x + 1, coord.y));
            }

            // bottom
            if (chunkY == 0)
            {
                this.UpdateChunk(new Vector2(coord.x, coord.y - 1));
            }
            // top
            else if (chunkY == worldProfile.ChunkSize - 1)
            {
                this.UpdateChunk(new Vector2(coord.x, coord.y + 1));
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

        /// <summary>
        /// Gets a voxel from a coordinate
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        public Voxel VoxelAt(int x, int y)
        {
            return this.voxels.At(x, y);
        }

        /// <summary>
        /// Sets voxel at coordinate
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="voxel">Voxel to set</param>
        public void SetVoxel(int x, int y, Voxel voxel)
        {
            this.voxels.Set(x, y, voxel);
        }
    }
}
