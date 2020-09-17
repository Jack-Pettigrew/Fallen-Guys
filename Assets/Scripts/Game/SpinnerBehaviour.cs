using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 100.0f;

    public bool x, y, z;

    Vector3 origin;

    private void Start()
    {
        origin = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddTorque(new Vector3(
            x ? 1 : 0,
            y ? 1 : 0,
            z ? 1 : 0) * speed, ForceMode.VelocityChange);

        rb.position = origin - rb.centerOfMass;
    }
}
