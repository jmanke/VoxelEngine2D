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
    public class VoxelEngine : Singleton<VoxelEngine>
    {
        [Tooltip("Path to voxel json definition in a Resources folder")]
        public string voxelAssetPath = "VoxelEngine2D/voxels";

        [Header("Tile sheet")]

        [Tooltip("Tile sheet for voxels")]
        public Sprite tileSheet;

        public Material material;

        // sparse array of definitions for voxels
        public static Voxel[] VoxelDefinitions { get; private set; }

        public static float TileSize { get; private set; } = 0.0625f;

        public static int ChunkSize { get; private set; }

        private WorldProfileData worldProfile;
        private Voxels voxels;

        /// <summary>
        /// Transform that the extent is based on
        /// </summary>
        public Transform extentTransform;
        private Vector2 lastExtentPosition;

        protected override void Awake()
        {
            base.Awake();
            this.worldProfile = new WorldProfileData();
            ChunkSize = this.worldProfile.ChunkSize;
            this.LoadVoxelDefinitions();
            this.voxels = new Voxels(this.material);
        }

        public void Start()
        {
            this.UpdateExtent();
        }

        public void OnApplicationQuit()
        {
            this.voxels.SaveAll();
        }

        public void UpdateExtent()
        {
            this.lastExtentPosition = this.extentTransform.position;
            this.voxels.UpdateExtent(this.lastExtentPosition, new Vector2(150f, 150f));
        }

        public Voxel VoxelAt(Vector2 pos)
        {
            return this.voxels.VoxelAt(Voxel.WorldToCoord(pos));
        }

        /// <summary>
        /// Updates a voxel at the given position. This will also update necessary chunks
        /// </summary>
        /// <param name="pos">World position of the voxel</param>
        /// <param name="voxel">Voxel to set at position</param>
        public void UpdateVoxel(Vector2 pos, Voxel voxel)
        {
            this.voxels.UpdateVoxel(Voxel.WorldToCoord(pos), voxel);
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

        private void FixedUpdate()
        {
            if (Vector2.Distance(this.extentTransform.position, this.lastExtentPosition) > ChunkSize)
            {
                this.UpdateExtent();
            }
        }

        private void LateUpdate()
        {
            this.voxels.Update();
        }
    }
}
