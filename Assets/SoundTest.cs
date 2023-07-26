using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    public GameObject innerMaze;
    private Rigidbody rb;

    public bool isColliding = false;
    private float marbleRollSpeed;
    public float MarbleRollSpeed { get { return marbleRollSpeed; } set { marbleRollSpeed = value; }}

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == innerMaze)
        {
            isColliding = true;
            Debug.Log("Sphere collider is in contact with the mesh collider.");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == innerMaze)
        {
            isColliding = false;
            marbleRollSpeed = 0;
            Debug.Log("Sphere collider is no longer in contact with the mesh collider.");
        }
    }
}
