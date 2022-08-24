using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazel.VoxelEngine2D.Data;
using UnityEngine;

namespace Hazel.VoxelEngine2D.Unity
{
    public class Chunk
    {
        public readonly Vector2Int Coord;
        public readonly Vector2 WorldPosition;

        public ChunkData ChunkData { get; private set; }

        private readonly Material material;
        private GameObject gameObject;
        private CustomCollider2D collider;
        private Rigidbody2D rigidbody;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public Chunk(Material material, Vector2Int coord, ChunkData chunkData)
        {
            this.ChunkData = chunkData;
            this.material = material;
            this.Coord = coord;
            this.WorldPosition = new Vector2(coord.x * chunkData.Size, coord.y * chunkData.Size);
        }

        /// <summary>
        /// Converts a world position to chunk coordinate
        /// </summary>
        /// <param name="pos">Worl position</param>
        /// <returns>Chunk coordinate</returns>
        public static Vector2Int WorldPosToCoord(Vector2 pos)
        {
            return WorldPosToCoord((int)pos.x, (int)pos.y);
        }

        /// <summary>
        ///  Converts a world position to chunk coordinate
        /// </summary>
        /// <param name="x">World x position</param>
        /// <param name="y">World y position</param>
        /// <returns>Chunk coordinate</returns>
        public static Vector2Int WorldPosToCoord(int x, int y)
        {
            int chunkSize = VoxelEngine.Instance.ChunkSize;
            if (x < 0) x -= (chunkSize - 1);
            if (y < 0) x -= (chunkSize - 1);
            return new Vector2Int(x / chunkSize, y / chunkSize);
        }

        /// <summary>
        ///  Converts a world position to the voxel's position in the chunk
        /// </summary>
        /// <param name="x">World x position</param>
        /// <param name="y">World y position</param>
        /// <returns>Voxel coordinate in the chunk</returns>
        public static Vector2Int WorldPosToVoxelCoord(int x, int y)
        {
            int chunkSize = VoxelEngine.Instance.ChunkSize;
            x %= chunkSize;
            y %= chunkSize;
            if (x < 0) x += chunkSize;
            if (y < 0) x += chunkSize;
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Retrieves the voxel at coordinate (relative to chunk)
        /// </summary>
        /// <param name="coord">Coordinate relative to chunk</param>
        /// <returns>Voxel</returns>
        public Voxel VoxelAt(Vector2Int coord)
        {
            return VoxelEngine.VoxelDefinitions[this.ChunkData.VoxelAt(coord.x, coord.y).Id];
        }

        /// <summary>
        /// Updates the chunk with current voxel information
        /// </summary>
        public async void Update()
        {
            if (this.gameObject == null)
            {
                this.BuildGameObject();
            }

            var (verticies, triangles, uv, physicsShapeGroup) = await this.CalculateObjectProperties();

            var mesh = this.meshFilter.mesh;
            mesh.Clear();
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.uv = uv;

            // add or remove collider
            if (physicsShapeGroup.shapeCount > 0)
            {
                if (this.collider == null)
                {
                    this.rigidbody = this.gameObject.AddComponent<Rigidbody2D>();
                    this.rigidbody.bodyType = RigidbodyType2D.Static;
                    this.collider = this.gameObject.AddComponent<CustomCollider2D>();
                }

                this.collider.SetCustomShapes(physicsShapeGroup);
            }
            else
            {
                if (this.collider != null)
                {
                    UnityEngine.Object.Destroy(this.rigidbody);
                    UnityEngine.Object.Destroy(this.collider);
                    this.collider = null;
                    this.rigidbody = null;
                }
            }
        }

        /// <summary>
        /// Unloads gameobject
        /// </summary>
        public void Unload()
        {
            if (this.gameObject == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(this.gameObject);
            this.gameObject = null;
            collider = null;
            rigidbody = null;
            meshRenderer = null;
            meshFilter = null;
        }

        private async Task<Tuple<Vector3[], int[], Vector2[], PhysicsShapeGroup2D>> CalculateObjectProperties()
        {
            return await Task.Run(() =>
            {
                var verticies = new List<Vector3>();
                var uvs = new List<Vector2>();
                var triangles = new List<int>();
                var physicsShapeGroup = new PhysicsShapeGroup2D();
                int v = 0;

                int chunkSize = VoxelEngine.Instance.ChunkSize;
                float tileSize = VoxelEngine.Instance.TileSize;

                // 0 = left, 1 = right, 2 = bottom, 3 = top
                var neighbourChunks = new Chunk[4];

                for (int x = 0; x < VoxelEngine.Instance.ChunkSize; x++)
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        var voxelData = this.ChunkData.VoxelAt(x, y);
                        var voxel = VoxelEngine.VoxelDefinitions[voxelData.Id];

                        if (voxel.Empty)
                        {
                            continue;
                        }

                        // build tile

                        // add verticies
                        verticies.Add(new Vector3(x, y));
                        verticies.Add(new Vector3(x + 1, y));
                        verticies.Add(new Vector3(x, y + 1));
                        verticies.Add(new Vector3(x + 1, y + 1));

                        // add uv's
                        var corner = new Vector2(tileSize * voxel.SpriteCoord[0], tileSize * voxel.SpriteCoord[1]);
                        uvs.Add(new Vector2(corner.x, corner.y));
                        uvs.Add(new Vector2(corner.x + tileSize, corner.y));
                        uvs.Add(new Vector2(corner.x, corner.y + tileSize));
                        uvs.Add(new Vector2(corner.x + tileSize, corner.y + tileSize));

                        // add triangles
                        triangles.Add(v);
                        triangles.Add(v + 2);
                        triangles.Add(v + 1);

                        triangles.Add(v + 1);
                        triangles.Add(v + 2);
                        triangles.Add(v + 3);

                        if (this.VoxelEmpty(x - 1, y, neighbourChunks) ||
                            this.VoxelEmpty(x + 1, y, neighbourChunks) ||
                            this.VoxelEmpty(x, y - 1, neighbourChunks) ||
                            this.VoxelEmpty(x, y + 1, neighbourChunks))
                        {
                            physicsShapeGroup.AddBox(new Vector2(x + 0.5f, y + 0.5f), new Vector2(1, 1));
                        }

                        v += 4;
                    }
                }

                return new Tuple<Vector3[], int[], Vector2[], PhysicsShapeGroup2D>(verticies.ToArray(), triangles.ToArray(), uvs.ToArray(), physicsShapeGroup);
            });
        }

        /// <summary>
        /// Checks if voxel is empty
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <param name="neighbourChunks">cached neighbour chunks</param>
        /// <returns>True if voxel is empty or doesn't exist</returns>
        private bool VoxelEmpty(int x, int y, Chunk[] neighbourChunks)
        {
            if (x >= 0 && x < this.ChunkData.Size && y >= 0 && y < this.ChunkData.Size)
            {
                return VoxelEngine.VoxelDefinitions[this.ChunkData.VoxelAt(x, y).Id].Empty;
            }
            else
            {
                int neighbourIndex;
                int neighbourX = x;
                int neighbourY = y;

                if (x == -1)
                {
                    neighbourIndex = 0;
                    neighbourX = this.ChunkData.Size - 1;
                }
                else if (x == this.ChunkData.Size)
                {
                    neighbourIndex = 1;
                    neighbourX = 0;
                } 
                else if (y == -1)
                {
                    neighbourIndex = 2;
                    neighbourY = this.ChunkData.Size - 1;
                }
                else
                {
                    neighbourIndex = 3;
                    neighbourY = 0;
                }

                var neighbourChunk = neighbourChunks[neighbourIndex];
                if (neighbourChunk == null)
                {
                    var neighbourChunkCoord = neighbourIndex switch
                    {
                        0 => new Vector2Int(this.Coord.x - 1, this.Coord.y),
                        1 => new Vector2Int(this.Coord.x + 1, this.Coord.y),
                        2 => new Vector2Int(this.Coord.x, this.Coord.y - 1),
                        _ => new Vector2Int(this.Coord.x, this.Coord.y + 1),
                    };
                    neighbourChunk = VoxelEngine.Instance.ChunkAtCoord(neighbourChunkCoord);
                    neighbourChunks[neighbourIndex] = neighbourChunk;
                }

                if (neighbourChunk != null)
                {
                    return VoxelEngine.VoxelDefinitions[neighbourChunk.ChunkData.VoxelAt(neighbourX, neighbourY).Id].Empty;
                }
            }

            return false;
        }

        private void BuildGameObject()
        {
            int chunkSize = VoxelEngine.Instance.ChunkSize;
            this.gameObject = new GameObject($"Chunk_{this.Coord.x / chunkSize}_{this.Coord.y / chunkSize}");
            this.gameObject.transform.position = this.WorldPosition;
            this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.meshRenderer.material = this.material;
            this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.meshFilter.mesh = new Mesh();
        }
    }
}
