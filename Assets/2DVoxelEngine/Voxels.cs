using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hazel.VoxelEngine2D.Data;
using Hazel.VoxelEngine2D.Unity;
using UnityEngine;

namespace Hazel.VoxelEngine2D
{
    public class Voxels
    {
        private readonly Dictionary<Vector2Int, Chunk> chunks = new();
        private readonly Dictionary<Vector2Int, Voxel> cachedVoxels = new();

        private readonly Queue<Chunk> chunksToUpdate = new();
        private readonly Queue<Chunk> chunksToUnload = new();

        private readonly int chunkSize;
        private readonly Material material;

        private readonly Persistence persistence;
        private readonly VoxelGenerator voxelGenerator;

        public Voxels(int chunkSize, Material chunkMaterial)
        {
            this.chunkSize = chunkSize;
            this.material = chunkMaterial;
            this.persistence = new Persistence("TestWorld");
            this.voxelGenerator = new VoxelGenerator();

            // TODO: clear all chunk files for testing, remove later
            var dir = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "TestWorld", "chunks"));
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
        }

        public Voxel VoxelAt(int x, int y)
        {
            if (this.cachedVoxels.TryGetValue(new Vector2Int(x, y), out var voxel))
            {
                return voxel;
            }

            return this.voxelGenerator.VoxelAt(x, y);
        }

        public void SetVoxel(int x, int y, Voxel voxel)
        {
            cachedVoxels[new Vector2Int(x, y)] = voxel;
        }

        /// <summary>
        /// Updates the extent of voxels
        /// </summary>
        /// <param name="center">Center of extent as world position</param>
        /// <param name="size">Size of extent</param>
        public async void UpdateExtent(Vector2 center, Vector2 size)
        {
            int radiusX = (int)size.x / 2;
            int right = (int)center.x + radiusX + this.chunkSize;
            int left = (int)center.x - radiusX;
            int radiusY = (int)size.y / 2;
            int top = (int)center.y + radiusY + this.chunkSize;
            int bottom = (int)center.y - radiusY;

            int leftCoord = left / this.chunkSize;
            int rightCoord = right / this.chunkSize + 1;
            int bottomCoord = bottom / this.chunkSize;
            int topCoord = top / this.chunkSize + 1;

            // find all chunks that need to be unloaded
            var chunks = new List<Chunk>(this.chunks.Values);
            foreach (var chunk in chunks)
            {
                var coord = chunk.Coord;

                if (coord.x < leftCoord || coord.x > rightCoord || coord.y > topCoord || coord.y < bottomCoord)
                {
                    this.chunksToUnload.Enqueue(chunk);
                    this.chunks.Remove(chunk.Coord);
                }
            }

            var chunkTasks = new List<Task<Chunk>>();

            for (int x = leftCoord; x <= rightCoord; x++)
            {
                for (int y = bottomCoord; y <= topCoord; y++)
                {
                    var coord = new Vector2Int(x, y);
                    if (!this.chunks.ContainsKey(coord))
                    {
                        var task = Task.Run(() =>
                        {
                            // load chunk
                            string chunkFilename = this.ChunkDataFilename(coord);
                            if (!this.persistence.TryLoad<ChunkData>(chunkFilename, out var chunkData))
                            {
                                // if not found, generate chunk data and save it
                                chunkData = this.GenerateChunkData(coord);
                                this.persistence.Save(chunkFilename, chunkData);
                            }

                            return new Chunk(this.material, coord, chunkData);
                        });

                        chunkTasks.Add(task);
                    }
                }
            }

            // wait for all chunks to load
            await Task.WhenAll(chunkTasks.ToArray());
            chunkTasks.ForEach(task => 
                {
                    var chunk = task.Result;
                    this.chunks.Add(chunk.Coord, chunk);
                    this.chunksToUpdate.Enqueue(chunk);
                });
        }

        private ChunkData GenerateChunkData(Vector2Int coord)
        {
            var chunkData = new ChunkData(this.chunkSize);
            var worldPosition = new Vector2Int(coord.x * this.chunkSize, coord.y * this.chunkSize);

            for (int x = 0; x < this.chunkSize; x++)
            {
                for (int y = 0; y < this.chunkSize; y++)
                {
                    var voxel = this.voxelGenerator.VoxelAt(worldPosition.x + x, worldPosition.y + y);
                    var voxelData = new VoxelData
                    {
                        Id = voxel.Id,
                        CurrentHitPoints = 0
                    };

                    chunkData.SetVoxel(x, y, voxelData);
                }
            }

            return chunkData;
        }

        /// <summary>
        /// Retrieves chunk at coord
        /// </summary>
        /// <param name="coord">Coordinate of chunk (world pos / chunk size)</param>
        /// <returns>Chunk or null if chunk is not found</returns>
        public Chunk ChunkAt(Vector2Int coord)
        {
            this.chunks.TryGetValue(coord, out var chunk);

            return chunk;
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
                this.chunksToUpdate.Enqueue(chunk);
            }
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="coord">Coordinate of the voxel</param>
        /// <param name="voxel">Voxel to set at coordinate</param>
        public void UpdateVoxel(Vector2Int coord, Voxel voxel)
        {
            this.SetVoxel(coord.x, coord.y, voxel);
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

        public void Update()
        {
            //unload chunk
            if (this.chunksToUnload.TryDequeue(out var chunkToUnload))
            {
                chunkToUnload.Unload();
            }

            // load chunk
            if (this.chunksToUpdate.TryDequeue(out var chunkToLoad))
            {
                chunkToLoad.Update();
            }
        }

        private string ChunkDataFilename(Vector2Int chunkCoord)
        {
            return Path.Combine("chunks", $"chunk_{chunkCoord.x}_{chunkCoord.y}.dat");
        }

        private void LoadChunkData(Vector2Int chunkCoord)
        {
            
        }
    }
}
