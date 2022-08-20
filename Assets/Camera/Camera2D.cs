using Hazel.VoxelEngine;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var worldPoint = this.Camera.ScreenToWorldPoint(Input.mousePosition);
            var coord = new Vector2Int((int)worldPoint.x, (int)worldPoint.y);

            // set to empty tile
            VoxelEngine2D.UpdateVoxel(coord, VoxelEngine2D.VoxelDefinitions[0]);
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
