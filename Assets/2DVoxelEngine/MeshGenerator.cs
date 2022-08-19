using UnityEngine;

namespace Hazel.VoxelEngine
{
    public class MeshGenerator
    {
        public Mesh CreateMeshPlane(int sizeX, int sizeY)
        {
            // generate a mesh for the chunk
            int dimX = sizeX + 1;
            int dimY = sizeY + 1;

            var verticies = new Vector3[dimX * dimY];
            for (int i = 0; i < verticies.Length; i++)
            {
                verticies[i] = new Vector3(i % dimX, i / dimX);
            }

            int[] triangles = new int[sizeX * sizeY * 6];
            for (int y = 0, j = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++, j += 6)
                {
                    int i = y * (sizeX + 1) + x;
                    triangles[j] = i;
                    triangles[j + 1] = i + 1;
                    triangles[j + 2] = i + sizeX + 1;

                    triangles[j + 3] = i + 1;
                    triangles[j + 4] = i + sizeX + 2;
                    triangles[j + 5] = i + sizeX + 1;
                }
            }

            var mesh = new Mesh
            {
                vertices = verticies,
                triangles = triangles
            };

            return mesh;
        }
    }
}
