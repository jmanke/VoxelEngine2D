using System.Collections.Generic;
using UnityEngine;

namespace Hazel.VoxelEngine
{
    public class Chunk
    {
        public ChunkData ChunkData { get; private set; }

        private GameObject renderObject;

        public Chunk(ChunkData chunkData)
        {
            this.ChunkData = chunkData;
        }

        public void Load(Material mat)
        {
            var verticies = new List<Vector3>();
            var uvs = new List<Vector2>();

            var triangles = new List<int>();
            int v = 0;

            for (int x = 0; x < this.ChunkData.SizeX; x++)
            {
                for (int y = 0; y < this.ChunkData.SizeY; y++)
                {
                    var voxel = VoxelEngine2D.VoxelDefinitions[this.ChunkData.Voxels.Get(x, y).Id];
                    if (voxel.Empty)
                    {
                        continue;
                    }

                    // add verticies
                    verticies.Add(new Vector3(x, y));
                    verticies.Add(new Vector3(x + 1, y));
                    verticies.Add(new Vector3(x, y + 1));
                    verticies.Add(new Vector3(x + 1, y + 1));

                    // add uv's
                    var corner = new Vector2(VoxelEngine2D.TileSize * voxel.SpriteCoord[0], VoxelEngine2D.TileSize * voxel.SpriteCoord[1]);
                    uvs.Add(new Vector2(corner.x, corner.y));
                    uvs.Add(new Vector2(corner.x + VoxelEngine2D.TileSize, corner.y));
                    uvs.Add(new Vector2(corner.x, corner.y + VoxelEngine2D.TileSize));
                    uvs.Add(new Vector2(corner.x + VoxelEngine2D.TileSize, corner.y + VoxelEngine2D.TileSize));

                    // add triangles
                    triangles.Add(v);
                    triangles.Add(v + 2);
                    triangles.Add(v + 1);

                    triangles.Add(v + 1);
                    triangles.Add(v + 2);
                    triangles.Add(v + 3);

                    v += 4;
                }
            }

            var mesh = new Mesh
            {
                vertices = verticies.ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray(),
            };

            var obj = new GameObject("Mesh");
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            this.renderObject = obj;
        }

        public void Unload()
        {

        }

        public void SetPosition(Vector2 position)
        {
            this.renderObject.transform.position = position;
        }
    }
}
