using Hazel.VoxelEngine2D.Data;
using UnityEngine;

namespace Hazel.VoxelEngine2D.Unity
{
    [System.Serializable]
    public class Voxel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Empty { get; set; }

        public int HitPoints { get; set; }

        public int[] SpriteCoord { get; set; }

        public int Hardness { get; set; }

        public int CurrentHitPoints { get; set; }

        public VoxelData ToVoxelData()
        {
            return new VoxelData
            {
                Id = this.Id,
                CurrentHitPoints = this.CurrentHitPoints,
            };
        }

        /// <summary>
        /// Returns a voxel coordinate based on world position
        /// </summary>
        /// <param name="pos">World position</param>
        /// <returns>A voxel coordinate</returns>
        public static Vector2Int WorldToCoord(Vector2 pos)
        {
            return new Vector2Int(pos.x < 0f ? (int)pos.x - 1 : (int)pos.x, pos.y < 0f ? (int)pos.y - 1 : (int)pos.y);
        }
    }
}
