using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

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

        [Header("Chunks")]

        public int chuckSize = 32;

        public Material material;

        // sparse array of definitions for voxels
        public static Voxel[] VoxelDefinitions { get; private set; }

        public static float TileSize { get; } = 0.0625f;

        public void Awake()
        {
            this.LoadVoxelDefinitions();
        }

        public void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var chunkData = new ChunkData(8, 8);
                    for (int k = 0; k < chunkData.Voxels.Width; k++)
                    {
                        for (int l = 0; l < chunkData.Voxels.Width; l++)
                        {
                            chunkData.Voxels.Set(k, l, new VoxelData
                            {
                                Id = Random.Range(0, 3),
                                CurrentHitPoints = 0
                            });
                        }
                    }

                    var chunk = new Chunk(chunkData);

                    chunk.Load(material);
                    chunk.SetPosition(new Vector2(i * 8, j * 8));
                }
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
