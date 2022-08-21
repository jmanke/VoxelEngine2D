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
            int right = (int)center.x + radiusX + this.chunkSize;
            int left = (int)center.x - radiusX;

            int radiusY = (int)size.y / 2;
            int top = (int)center.y + radiusY + this.chunkSize;
            int bottom = (int)center.y - radiusY;

            var chunksToUnload = new List<Chunk>();

            // find all chunks that need to be unloaded
            foreach (var keyVal in this.chunks)
            {
                var chunkPos = keyVal.Value.WorldPosition;

                if (chunkPos.x < left || chunkPos.x > right || chunkPos.y > top || chunkPos.y < bottom)
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

        /// <summary>
        /// Updates chunk at world position
        /// </summary>
        /// <param name="position">world position</param>
        public void UpdateChunk(Vector2 position)
        {
            var coord = new Vector2Int((int)position.x / this.chunkSize, (int)position.y / this.chunkSize);
            if (this.chunks.TryGetValue(coord, out var chunk))
            {
                chunk.Update();
            }
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="coord">Coordinate of the voxel</param>
        /// <param name="voxel">Voxel to set at coordinate</param>
        public void UpdateVoxel(Vector2Int coord, Voxel voxel)
        {
            this.Set(coord.x, coord.y, voxel);
            this.UpdateChunk(new Vector2(coord.x, coord.y));

            // check if a neighbour chunk also needs to be updated
            int chunkX = coord.x % this.chunkSize;
            int chunkY = coord.y % this.chunkSize;

            // left
            if (chunkX == 0)
            {
                this.UpdateChunk(new Vector2(coord.x - 1, coord.y));
            }
            // right
            else if (chunkX == this.chunkSize - 1)
            {
                this.UpdateChunk(new Vector2(coord.x + 1, coord.y));
            }

            // bottom
            if (chunkY == 0)
            {
                this.UpdateChunk(new Vector2(coord.x, coord.y - 1));
            }
            // top
            else if (chunkY == this.chunkSize - 1)
            {
                this.UpdateChunk(new Vector2(coord.x, coord.y + 1));
            }
        }
    }
}
