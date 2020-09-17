using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownBehaviour : NetworkBehaviour
{
    private Vector3 origin = Vector3.zero;
    public float amplitude = 2.0f;
    public float frequency = 2.0f;

    [ServerCallback]
    private void Start()
    {
        origin = transform.position;
    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        transform.position = origin + Vector3.up * amplitude * Mathf.Sin(frequency * Time.time);
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            CmdEndGame();
        }
    }

    [Command(ignoreAuthority = true)]
    private void CmdEndGame()
    {
        GameManager.singleton.RpcEndGame();
    }
}
