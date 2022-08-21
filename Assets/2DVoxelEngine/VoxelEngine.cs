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

        private WorldProfileData worldProfile;
        private Voxels voxels;

        /// <summary>
        /// Transform that the extent is based on
        /// </summary>
        public Transform extentTransform;
        private Vector2 lastExtentPosition;

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
            this.UpdateExtent();
        }

        public void UpdateExtent()
        {
            this.lastExtentPosition = this.extentTransform.position;
            this.voxels.UpdateExtent(this.lastExtentPosition, new Vector2(350f, 350f));
        }

        /// <summary>
        /// Updates chunk at world position
        /// </summary>
        /// <param name="position">world position</param>
        public void UpdateChunk(Vector2 position)
        {
            this.voxels.UpdateChunk(position);
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="coord">Coordinate of the voxel</param>
        /// <param name="voxel">Voxel to set at coordinate</param>
        public void UpdateVoxel(Vector2Int coord, Voxel voxel)
        {
            this.voxels.UpdateVoxel(coord, voxel);
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

        private void FixedUpdate()
        {
            if (Vector2.Distance(this.extentTransform.position, this.lastExtentPosition) > this.ChunkSize)
            {
                this.UpdateExtent();
            }
        }
    }
}
