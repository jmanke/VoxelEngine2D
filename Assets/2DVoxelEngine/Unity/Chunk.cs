using System.Collections.Generic;
using UnityEngine;

namespace Hazel.VoxelEngine2D.Unity
{
    public class Chunk
    {
        private readonly Vector2Int position;
        private readonly Material material;
        private GameObject gameObject;
        private CustomCollider2D collider;
        private Rigidbody2D rigidbody;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public Chunk(Material material, Vector2Int position)
        {
            this.material = material;
            this.position = position;
        }

        public void Update()
        {
            if (this.gameObject == null)
            {
                this.BuildGameObject();
            }

            var verticies = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var physicsShapeGroup = new PhysicsShapeGroup2D();
            int v = 0;

            int chunkSize = VoxelEngine.Instance.ChunkSize;
            float tileSize = VoxelEngine.Instance.TileSize;

            for (int x = 0; x < VoxelEngine.Instance.ChunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    var voxel = VoxelEngine.VoxelDefinitions[Voxels.At(this.position.x + x, this.position.y + y).Id];
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

                    var tilePos = new Vector2Int(this.position.x + x, this.position.y + y);

                    if (Voxels.At(tilePos.x - 1, tilePos.y).Id == 0 ||
                        Voxels.At(tilePos.x + 1, tilePos.y).Id == 0 || 
                        Voxels.At(tilePos.x, tilePos.y - 1).Id == 0 ||
                        Voxels.At(tilePos.x, tilePos.y + 1).Id == 0)
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
            Object.Destroy(this.gameObject);
            this.gameObject = null;
            collider = null;
            rigidbody = null;
            meshRenderer = null;
            meshFilter = null;
        }

        public void SetPosition(Vector2 position)
        {
            this.gameObject.transform.position = position;
        }

        private void BuildGameObject()
        {
            int chunkSize = VoxelEngine.Instance.ChunkSize;
            this.gameObject = new GameObject($"Chunk_{this.position.x / chunkSize}_{this.position.y / chunkSize}");
            this.SetPosition(position);
            this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.meshRenderer.material = this.material;
            this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.meshFilter.mesh = new Mesh();
        }
    }
}
