using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEditor;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class TestPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject spherePrefab = null;
    [SerializeField] public float speed = 1.0f;
    [SerializeField] private Rigidbody rb = null;

    private void Update()
    {
        if(isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                speed += 0.5f;
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                speed -= 0.5f;
            }

            if(Input.GetKeyDown(KeyCode.F))
            {
                // Ask server to spawn sphere
                CmdSpawnSphere();
            }
            
        }
    }

    // This Client asks Server to do this:
    [Command]
    private void CmdSpawnSphere()
    {
        GameObject sphere = Instantiate(spherePrefab, transform.position + Vector3.up * 5.0f, Quaternion.identity);
        NetworkServer.Spawn(sphere);
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            rb.AddForce(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed, ForceMode.Impulse);

            if(Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector3.up * 10.0f, ForceMode.Impulse);
            }
        }
    }
}
