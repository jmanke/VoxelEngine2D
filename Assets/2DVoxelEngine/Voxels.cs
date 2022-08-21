using System.Collections.Generic;
using Hazel.VoxelEngine2D.Unity;
using UnityEngine;

namespace Hazel.VoxelEngine2D
{
    public class Voxels
    {
        public FastNoiseUnity FastNoise = new();

        private readonly Dictionary<Vector2Int, Chunk> chunks = new();
        private readonly Dictionary<Vector2Int, Voxel> cachedVoxels = new();

        private readonly int chunkSize;
        private readonly Material material;

        public Voxels(int chunkSize, Material chunkMaterial)
        {
            this.chunkSize = chunkSize;
            this.material = chunkMaterial;
        }

        public Voxel At(int x, int y)
        {
            if (cachedVoxels.TryGetValue(new Vector2Int(x, y), out var voxel))
            {
                return voxel;
            }

            if (y < 10)
            {
                return VoxelEngine.VoxelDefinitions[3];
            }

            if (y < 40)
            {
                return VoxelEngine.VoxelDefinitions[2];
            }

            return FastNoise.fastNoise.GetNoise(x, y) < 0.5f ? VoxelEngine.VoxelDefinitions[1] : VoxelEngine.VoxelDefinitions[0];
        }

        public void Set(int x, int y, Voxel voxel)
        {
            cachedVoxels[new Vector2Int(x, y)] = voxel;
        }

        /// <summary>
        /// Updates the extent of voxels
        /// </summary>
        /// <param name="center">Center of extent as world position</param>
        /// <param name="size">Size of extent</param>
        public void UpdateExtent(Vector2 center, Vector2 size)
        {
            int radiusX = (int)size.x / 2;
            int right = (int)center.x + radiusX;
            int left = (int)center.x - radiusX;

            int radiusY = (int)size.y / 2;
            int top = (int)center.y - radiusY;
            int bottom = (int)center.y - radiusY;

            var chunksToUnload = new List<Chunk>();

            // find all chunks that need to be unloaded
            foreach (var keyVal in this.chunks)
            {
                var chunkPos = keyVal.Value.WorldPosition;

                if (chunkPos.x < left || chunkPos.x > right + this.chunkSize || chunkPos.y > top + this.chunkSize || chunkPos.y < bottom)
                {
                    chunksToUnload.Add(keyVal.Value);
                }
            }

            foreach (var chunk in chunksToUnload)
            {
                chunk.Unload();
                this.chunks.Remove(chunk.Coord);
            }

            // load in chunks
            int leftCoord = left / this.chunkSize;
            int rightCoord = right / this.chunkSize + 1;
            int bottomCoord = bottom / this.chunkSize;
            int topCoord = top / this.chunkSize + 1;

            var chunksToLoad = new List<Vector2Int>();
            for (int x = leftCoord; x <= rightCoord; x++)
            {
                for (int y = bottomCoord; y <= topCoord; y++)
                {
                    var coord = new Vector2Int(x, y);
                    if (!this.chunks.ContainsKey(coord))
                    {
                        chunksToLoad.Add(coord);
                    }
                }
            }

            foreach (var coord in chunksToLoad)
            {
                var chunk = new Chunk(this.material, coord);
                chunk.Update();
                this.chunks.Add(coord, chunk);
            }
        }
    }
}