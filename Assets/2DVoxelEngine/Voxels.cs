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

        private readonly Queue<Chunk> chunksToUpdate = new();
        private readonly Queue<Chunk> chunksToUnload = new();

        private readonly Material material;

        private readonly Persistence persistence;
        private readonly VoxelGenerator voxelGenerator;

        public Voxels(Material chunkMaterial)
        {
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

        /// <summary>
        /// Retrieves voxel at voxel coordinate
        /// </summary>
        /// <param name="coord">Voxel coordinate</param>
        /// <returns>Voxel</returns>
        public Voxel VoxelAt(Vector2Int coord)
        {
            var chunk = this.ChunkAt(Chunk.WorldPosToCoord(coord.x, coord.y));
            if (chunk == null)
            {
                return this.voxelGenerator.VoxelAt(coord.x, coord.y);
            }

            var voxelChunkCoord = Chunk.WorldPosToVoxelCoord(coord.x, coord.y);
            return chunk.VoxelAt(voxelChunkCoord);
        }

        /// <summary>
        /// Updates the extent of voxels
        /// </summary>
        /// <param name="center">Center of extent as world position</param>
        /// <param name="size">Size of extent</param>
        public async void UpdateExtent(Vector2 center, Vector2 size)
        {
            int radiusX = (int)size.x / 2;
            int right = (int)center.x + radiusX + VoxelEngine.ChunkSize;
            int left = (int)center.x - radiusX;
            int radiusY = (int)size.y / 2;
            int top = (int)center.y + radiusY + VoxelEngine.ChunkSize;
            int bottom = (int)center.y - radiusY;

            int leftCoord = left / VoxelEngine.ChunkSize;
            int rightCoord = right / VoxelEngine.ChunkSize + 1;
            int bottomCoord = bottom / VoxelEngine.ChunkSize;
            int topCoord = top / VoxelEngine.ChunkSize + 1;

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

                            return new Chunk(this, this.material, coord, chunkData);
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

        /// <summary>
        /// Generates chunk data at specified chunk coordinates
        /// </summary>
        /// <param name="coord">Chunk coordinate</param>
        /// <returns>Chunk data</returns>
        private ChunkData GenerateChunkData(Vector2Int coord)
        {
            var chunkData = new ChunkData(VoxelEngine.ChunkSize);
            var worldPosition = new Vector2Int(coord.x * VoxelEngine.ChunkSize, coord.y * VoxelEngine.ChunkSize);

            for (int x = 0; x < VoxelEngine.ChunkSize; x++)
            {
                for (int y = 0; y < VoxelEngine.ChunkSize; y++)
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
        /// Retrieves chunk at coordinate
        /// </summary>
        /// <param name="coord">Coordinate of chunk</param>
        /// <returns>Chunk or null if chunk is not found</returns>
        public Chunk ChunkAt(Vector2Int coord)
        {
            this.chunks.TryGetValue(coord, out var chunk);

            return chunk;
        }

        /// <summary>
        /// Queues chunk for update
        /// </summary>
        /// <param name="chunk">Chunk to update</param>
        public void UpdateChunk(Chunk chunk)
        {
            if (chunk != null)
            {
                this.chunksToUpdate.Enqueue(chunk);
            }
        }

        /// <summary>
        /// Updates a voxel at the given coordinate. This will also update necessary chunks
        /// </summary>
        /// <param name="pos">World position of voxel</param>
        /// <param name="voxel">Voxel to set at position</param>
        public void UpdateVoxel(Vector2Int pos, Voxel voxel)
        {
            var chunkCoord = Chunk.WorldPosToCoord(pos.x, pos.y);
            var chunk = this.ChunkAt(chunkCoord);

            if (chunk == null)
            {
                return;
            }

            var chunkVoxelCoord = Chunk.WorldPosToVoxelCoord(pos.x, pos.y);

            chunk.ChunkData.SetVoxel(chunkVoxelCoord.x, chunkVoxelCoord.y, voxel.ToVoxelData());
            this.UpdateChunk(chunk);

            // check if a neighbour chunk also needs to be updated
            // left
            if (chunkVoxelCoord.x == 0)
            {
                var c = this.ChunkAt(new Vector2Int(chunkCoord.x - 1, chunkCoord.y));
                this.UpdateChunk(c);
            }
            // right
            else if (chunkVoxelCoord.x == VoxelEngine.ChunkSize - 1)
            {
                var c = this.ChunkAt(new Vector2Int(chunkCoord.x + 1, chunkCoord.y));
                this.UpdateChunk(c);
            }

            // bottom
            if (chunkVoxelCoord.y == 0)
            {
                var c = this.ChunkAt(new Vector2Int(chunkCoord.x, chunkCoord.y - 1));
                this.UpdateChunk(c);
            }
            // top
            else if (chunkVoxelCoord.y == VoxelEngine.ChunkSize - 1)
            {
                var c = this.ChunkAt(new Vector2Int(chunkCoord.x, chunkCoord.y + 1));
                this.UpdateChunk(c);
            }
        }

        public void Update()
        {
            // only perform one unload and load per frame to prevent frame drops

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
    }
}
