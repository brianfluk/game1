using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    [SerializeField] // shows it in inspector
    private Rigidbody rb;

    public Transform opponent; 

    public float footspeed = 3;
    public float rotationspeed = 5;

    private float inputX;
    private float inputZ;
    private Quaternion targetRotation;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        if (opponent != null) {
            targetRotation = Quaternion.LookRotation(opponent.transform.position - transform.position);
        } else {
            print($"no opponent to target for rotation for {this.GetType().Name}");
        }
    }

    // Fixed time step of 50 FPS (rate of physics calculations)
    void FixedUpdate()
    {
        var camera = Camera.main;
 
        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        rb.velocity = (right * inputX + forward * inputZ) * footspeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationspeed * Time.deltaTime);


    }
}
