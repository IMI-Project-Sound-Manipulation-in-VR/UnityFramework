using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    public GameObject innerMaze;
    public GameObject outerMaze;
    private Rigidbody rb;

    private float volume = 1.0f;
    public float Volume { get { return volume; } set { volume = value; } }

    private float pitch = 0.5f;
    public float Pitch { get { return pitch; } set { pitch = value; } }

    public bool isColliding = false;
    
    private float marbleRollSpeed;
    public float MarbleRollSpeed { get { return marbleRollSpeed; } set { marbleRollSpeed = value; }}

    private float marble;
    public float Marble { get { return marble; } set { marble = value; }}

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isColliding) {
            Vector3 velocity = rb.velocity;
            marbleRollSpeed = velocity.magnitude * 5;
        }

        // prevents master control of sound manager
        // to override this value
        marble = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == innerMaze)
        {
            isColliding = true;
            Debug.Log("Sphere collider is in contact with the mesh collider.");
        }

        if(collision.gameObject == outerMaze && isColliding == false)
        {
            marble = 1;
            isColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == innerMaze || collision.gameObject == outerMaze)
        {
            isColliding = false;
            marbleRollSpeed = 0;
            Debug.Log("Sphere collider is no longer in contact with the mesh collider.");
        }
    }
}
