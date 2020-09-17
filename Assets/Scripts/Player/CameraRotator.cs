using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS CONTROLS SEPERATELY TO MULTIPLAYER ARCHITECTURE

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float targetDist = 10.0f;
    [SerializeField] private float targetHeight = 10.0f;
    [SerializeField] public Transform cameraTarget = null;
    [SerializeField] private float cameraSensitivity = 1.0f;

    private float pitch, yaw;
    [SerializeField] private Vector2 pitchMinMax = new Vector2(-90, 90);

    // Start is called before the first frame update
    void Start()
    {
        pitch = transform.rotation.x;
        yaw = transform.rotation.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(cameraTarget)
        {
            yaw += Input.GetAxis("Mouse X") * cameraSensitivity;
            pitch += Input.GetAxis("Mouse Y") * cameraSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
            
            Vector3 targetRot = new Vector3(-pitch, yaw, 0);
            transform.eulerAngles = targetRot;
            
            transform.position = cameraTarget.position - (transform.forward * targetDist) + (Vector3.up * targetHeight);
        }
    }
}
