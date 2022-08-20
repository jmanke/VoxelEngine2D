using System.Collections.Generic;
using UnityEngine;
using Hazel.VoxelEngine.Data;

namespace Hazel.VoxelEngine.Unity
{
    public class Chunk
    {
        private GameObject gameObject;
        private CustomCollider2D collider;
        private Rigidbody2D rigidbody;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private Vector2Int position;

        public Chunk(Material mat, Vector2Int position)
        {
            this.gameObject = new GameObject("Mesh");
            this.SetPosition(position);
            this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.meshRenderer.material = mat;
            this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.meshFilter.mesh = new Mesh();
            this.position = position;
        }

        public void Update()
        {
            var verticies = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var physicsShapeGroup = new PhysicsShapeGroup2D();
            int v = 0;

            for (int x = 0; x < VoxelEngine2D.ChunkSize; x++)
            {
                for (int y = 0; y < VoxelEngine2D.ChunkSize; y++)
                {
                    var voxel = VoxelEngine2D.VoxelDefinitions[TerrainBuilder.VoxelAt(this.position.x + x, this.position.y + y).Id];
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

                    var tilePos = new Vector2Int(this.position.x + x, this.position.y + y);

                    if (TerrainBuilder.VoxelAt(tilePos.x - 1, tilePos.y).Id == 0 ||
                        TerrainBuilder.VoxelAt(tilePos.x + 1, tilePos.y).Id == 0 || 
                        TerrainBuilder.VoxelAt(tilePos.x, tilePos.y - 1).Id == 0 ||
                        TerrainBuilder.VoxelAt(tilePos.x, tilePos.y + 1).Id == 0)
                    {
                        physicsShapeGroup.AddBox(new Vector2(x + 0.5f, y + 0.5f), new Vector2(1, 1));
                    }

                    v += 4;
                }
            }

            // construct mesh
            var mesh = this.meshFilter.mesh;
            mesh.Clear();
            mesh.vertices = verticies.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

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
                    Object.Destroy(this.rigidbody);
                    Object.Destroy(this.collider);
                    this.collider = null;
                    this.rigidbody = null;
                }
            }
        }

        public void Unload()
        {

        }

        public void SetPosition(Vector2 position)
        {
            this.gameObject.transform.position = position;
        }
    }
}
