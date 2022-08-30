using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public float speed = 1f;
    public float jumpForce = 20f;

    public new Rigidbody2D rigidbody { get; private set; }

    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int movementX = 0;
        int movementY = 0;

        if (Input.GetKey(KeyCode.W))
        {
            movementY += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementY -= 1;

        }
        if (Input.GetKey(KeyCode.A))
        {
            movementX -= 1;

        }
        if (Input.GetKey(KeyCode.D))
        {
            movementX += 1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.rigidbody.AddForce(Vector2.up * this.jumpForce, ForceMode2D.Impulse);
        }

        this.rigidbody.velocity = new Vector2(movementX * this.speed, (this.rigidbody.velocity.y + (Physics.gravity.y * Time.deltaTime)));
    }
}
