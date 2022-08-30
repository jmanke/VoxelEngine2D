using Hazel.VoxelEngine2D;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Camera2D : MonoBehaviour
{
    public float cameraSpeed = 1f;

    public Camera Camera {get; private set; }

    private void Awake()
    {
        this.Camera = this.GetComponent<Camera>();    
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = this.Camera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            // set to empty tile
            VoxelEngine.Instance.UpdateVoxel(mousePosition, VoxelEngine.VoxelDefinitions[1]);
        }

        if (Input.GetMouseButtonDown(1))
        {
            var worldPoint = this.Camera.ScreenToWorldPoint(Input.mousePosition);

            // set to empty tile
            VoxelEngine.Instance.UpdateVoxel(worldPoint, VoxelEngine.VoxelDefinitions[0]);
        }

        if (Input.GetKey(KeyCode.W))
        {
            this.transform.position += Vector3.up * this.cameraSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.position += Vector3.down * this.cameraSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.position += Vector3.left * this.cameraSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.position += Vector3.right * this.cameraSpeed;
        }
    }
}
