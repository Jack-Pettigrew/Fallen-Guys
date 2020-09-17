using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationViewer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float dist;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position;
        transform.eulerAngles += Vector3.up * speed;
        transform.position = (target.position + offset) + (-transform.forward * dist);
    }
}
