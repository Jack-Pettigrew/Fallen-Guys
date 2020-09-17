using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenGuysNetworkManager : NetworkManager
{
    public Transform spawnPointOne, spawnPointTwo;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // add player at correct spawn position
        Transform start = numPlayers == 0 ? spawnPointOne : spawnPointTwo;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
