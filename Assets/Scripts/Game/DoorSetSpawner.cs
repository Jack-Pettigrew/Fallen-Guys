using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSetSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject fullDoorPrefeb, brokenDoorPrefab;
    [SerializeField] private GameObject[] doorFrames;
    [SerializeField] private int maxBrokenDoors = 1;

    [ServerCallback]
    void Start()
    {
        // HOST
        List<GameObject> doorSet = new List<GameObject>();
        GameObject spawnedDoor;

        // Randomly Spawn broken Doors
        int brokenDoorsSpawned = 0;
        int doorsLeft = doorFrames.Length;
        foreach (GameObject frame in doorFrames)
        {
            if(doorsLeft > maxBrokenDoors)
            {
                // Random
                int rand = Random.Range(0, 2);
                if (rand == 0)
                    spawnedDoor = Instantiate(fullDoorPrefeb, frame.transform.position, Quaternion.identity);
                else
                {
                    spawnedDoor = Instantiate(brokenDoorPrefab, frame.transform.position, Quaternion.identity);
                    brokenDoorsSpawned++;
                }
            }
            else if(brokenDoorsSpawned < maxBrokenDoors)
            {
                // Spawn the rest broken doors
                spawnedDoor = Instantiate(brokenDoorPrefab, frame.transform.position, Quaternion.identity);
                brokenDoorsSpawned++;
            }
            else
            {
                // Spawn the rest full doors
                spawnedDoor = Instantiate(fullDoorPrefeb, frame.transform.position, Quaternion.identity);
            }


            doorsLeft--;
            doorSet.Add(spawnedDoor);
        }

        // Spawn all on Client
        foreach (var door in doorSet)
        {
            NetworkServer.Spawn(door);
        }
    }

    private void OnValidate()
    {
        if(maxBrokenDoors >= doorFrames.Length)
        {
            maxBrokenDoors = doorFrames.Length - 1;
        }
    }
}
