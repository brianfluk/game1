using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxer2 : MonoBehaviour
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
        // inputX = Input.GetAxis("Horizontal");
        // inputZ = Input.GetAxis("Vertical");

       
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
        
        if (opponent != null) {
            Vector3 DeltaVector = opponent.transform.position - transform.position;
            DeltaVector.y = 0.0f; // only rotate on the y-axis
            targetRotation = Quaternion.LookRotation(DeltaVector);
        } else {
            Debug.Log($"no opponent to target to face {this.GetType().Name}");
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationspeed * Time.deltaTime);

    }
}
