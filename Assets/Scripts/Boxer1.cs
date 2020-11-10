using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class Boxer1 : MonoBehaviour
{
    [SerializeField] // shows it in inspector
    private Rigidbody rb;

    public Transform opponent; 

    public float footspeed = 3;
    public float rotationspeed = 5;

    [SerializeField] private float inputX;
    [SerializeField] private float inputZ;
    private Quaternion targetRotation;

    [SerializeField] private bool isCollided = false;
    [SerializeField] private Vector3 opponentNormal;

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
    
    // helper for debugging
    public static void DumpToConsole(object obj)
    {
        Type myType = obj.GetType();
        Debug.Log(myType);
        IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

        foreach (PropertyInfo prop in props)
        {
            object propValue = prop.GetValue(obj, null);

            Debug.Log(propValue);
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        GameObject otherObj = collision.gameObject;
        Debug.Log($"Collided with: {otherObj} (opponent: {opponent.name})");

        if (otherObj.name == opponent.name) {
            this.isCollided = true;
            this.opponentNormal = collision.contacts[0].normal;
            Debug.Log("this.isCollided" + this.isCollided);
        }
    }

    void OnCollisionExit(Collision collision) 
    {
        GameObject otherObj = collision.gameObject;
        Debug.Log($"UNCollided with: {otherObj} (opponent: {opponent.name})");

        if (otherObj.name == opponent.name) {
            this.isCollided = false;
        }
    }
 
    void OnTriggerEnter(Collider collider) {
        GameObject otherObj = collider.gameObject;
        Debug.Log("Triggered with: " + otherObj);
    }

    static float SpeedUpGetAxis(float ogValue) {
        return (Math.Abs(ogValue) < 0.5) ? Math.Min(ogValue * (float)2.5, (float)0.5) : ogValue;
    }

    // Fixed time step of 50 FPS (rate of physics calculations)
    void FixedUpdate()
    {
        inputX = SpeedUpGetAxis(Input.GetAxis("Horizontal"));
        inputZ = SpeedUpGetAxis(Input.GetAxis("Vertical"));
        var camera = Camera.main;
 
        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 intendedVec = (right * inputX + forward * inputZ) * footspeed;
        if (this.isCollided) {
            Vector3 v1 = intendedVec;
            Vector3 v2 = this.opponentNormal;
            v1.Normalize();
            v2.Normalize();

            // When collided, can only move away straight. 
            // No strafing while in contact, for now (causes rotation issues)
            // otherwise, simply: rb.velocity = v1 + v2;
            Vector3 vecTowardOpponent = Vector3.ProjectOnPlane(v1, Vector3.Cross(v2, Vector3.up));
            if ((vecTowardOpponent + v2).magnitude < vecTowardOpponent.magnitude) { // toward opponent
                rb.velocity = Vector3.zero;
            } else {
                rb.velocity = vecTowardOpponent;
            }

            Debug.Log($"is collided.\t nromalized IntendedVec {v1}.\t  opponentNormal {v2}");
            Debug.Log($"is collided.\t IntendedVec {intendedVec}.\t  opponentNormal {opponentNormal}");

        } else {
            rb.velocity = intendedVec;
        }
        
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
