using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CinecamController : MonoBehaviour
{
    private PostProcessVolume ppVolume;
    private DepthOfField dof;
    public float dofAdjustSpeed = 1.0f;

    public float speed = 1.0f;
    private float yaw, pitch;

    private void Start()
    {
        ppVolume = GetComponent<PostProcessVolume>();
        ppVolume.profile.TryGetSettings(out dof);

        yaw = transform.eulerAngles.x;
        pitch = transform.eulerAngles.y;

        CursorManager.ToggleCursor(false);
    }

    // Update is called once per frame
    void Update()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch += Input.GetAxis("Mouse Y");
        transform.localEulerAngles = new Vector3(-pitch, yaw, 0);

        if (Input.GetKeyDown(KeyCode.PageUp))
            speed++;
        else if (Input.GetKeyDown(KeyCode.PageDown))
            speed--;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = 0;
        if (Input.GetKey(KeyCode.E))
            y = 1;
        else if (Input.GetKey(KeyCode.Q))
            y = -1;

        transform.position += (transform.rotation * new Vector3(x, y, z)) * speed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 10.0f);

        float distance;
        if (hit.distance < 10.0f)
        {
            distance = Vector3.Distance(transform.position, hit.point);
        }
        else
            distance = 10.0f;

        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, distance, Time.deltaTime * dofAdjustSpeed);
    }
}
