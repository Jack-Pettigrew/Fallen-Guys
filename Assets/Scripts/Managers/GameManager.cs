using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    // WTF SINGLETONS HOW DARE Y-
    #region Singleton
    public static GameManager singleton;
    private void Awake()
    {     
        if (singleton != this)
            singleton = this;
    }
    #endregion

    [Header("Game Settings")]
    public int minPlayers = 1;
    public float timeTrialLength = 30.0f;
    
    private static List<NetworkIdentity> readyPlayers = new List<NetworkIdentity>();

    [Header("UI Elements")]
    public GameObject waitingForHost;
    public GameObject startGameButton, winnerText;

    /// <summary>
    /// Tell the server the Client is ready to play.
    /// </summary>
    /// <param name="netId">The NetID of the ready Client.</param>
    [Command(ignoreAuthority = true)]
    public void CmdReadyPlayer(NetworkIdentity netId)
    {
        readyPlayers.Add(netId);
        Debug.Log("Player: " + netId.connectionToClient.address + " has connected.");

        // Reached playable number of connections
        if(NetworkServer.connections.Count >= minPlayers)
        {
            RpcHostReadyUp();
        }
    }

    /// <summary>
    /// Tells Host/Client to toggle respective ready UI
    /// </summary>
    [ClientRpc]
    public void RpcHostReadyUp()
    {
        if(isServer)
        {
            // Host: Start game button
            startGameButton.SetActive(true);
        }
        else
        {
            // Client: Waiting for Host
            waitingForHost.SetActive(true);
        }
    }

    /// <summary>
    /// Starts the Game
    /// </summary>
    [ClientRpc]
    public void RpcOnStartGame()
    {
        // Remove Client/Host UI
        if(isServer)
        {
            startGameButton.SetActive(false);
        }
        else
            waitingForHost.SetActive(false);

        ClientScene.localPlayer.GetComponent<Player>().enabled = true;
        CursorManager.ToggleCursor(false);
    }

    /// <summary>
    /// Ends the current game session.
    /// </summary>
    [ClientRpc]
    public void RpcEndGame()
    {
        // Prevent client move
        foreach (NetworkIdentity player in readyPlayers)
        {
            if(player == ClientScene.localPlayer)
            {
                player.GetComponent<Player>().enabled = false;
                break;
            }
        }
        winnerText.SetActive(true);

        Invoke("LoadCelebrateScene", 3.0f);
    }

    [Client]
    public void LoadCelebrateScene()
    {
        SceneManager.LoadScene(1);
    }
}