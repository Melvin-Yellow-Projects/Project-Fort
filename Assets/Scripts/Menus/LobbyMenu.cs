/**
 * File Name: LobbyMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 *      TODO: Add Dapper Dino as one of the authors
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class LobbyMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] SaveLoadMenu saveLoadMenu = null;

    [SerializeField] Button startGameButton = null;
    [SerializeField] LobbyItem[] lobbyItems = null;

    bool hasSubscribed = false;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    // HACK: Since awake and on destroy are only called on previously active game objects, the
    // sub and unsub methods are called by MainMenu.cs. This is jank, but it works. maybe do over?

    //private void Awake()
    //{
    //    Subscribe();
    //}

    //private void OnDestroy()
    //{
    //    Unsubscribe();
    //}

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void StartGame()
    {
        bool foundSameTeam = false;
        for (int i = 0; !foundSameTeam && i < GameManager.Players.Count - 1; i++)
        {
            Player player = GameManager.Players[i];

            for (int j = i + 1; !foundSameTeam && j < GameManager.Players.Count; j++)
            {
                foundSameTeam = (player.MyTeam == GameManager.Players[j].MyTeam);
            }
        }

        if (foundSameTeam) return;

        saveLoadMenu.Open(3);
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected) // are you a host?
        {
            NetworkManager.singleton.StopHost();
        }
        else // you must be a client
        {
            NetworkManager.singleton.StopClient();
        }

        // this reloads the start menu, it's the lazy way rather than turning on/off various UI
        SceneLoader.LoadStartScene();
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    public void Subscribe()
    {
        if (hasSubscribed) return;
        hasSubscribed = true;

        GameNetworkManager.OnClientConnectEvent += HandleOnClientConnectEvent;
        GameNetworkManager.OnClientDisconnectEvent += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate += HandlePartyOwnerStateChange;
        Team.ClientOnChangeTeam += RefreshLobbyItems;
    }

    public void Unsubscribe()
    {
        hasSubscribed = false;
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
        GameNetworkManager.OnClientDisconnectEvent += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate -= RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate -= HandlePartyOwnerStateChange;
        Team.ClientOnChangeTeam -= RefreshLobbyItems;
    }

    private void HandleOnClientConnectEvent()
    {
        gameObject.SetActive(true);
    }

    private void RefreshLobbyItems()
    {
        for (int i = 0; i < GameManager.Players.Count; i++)
        {
            lobbyItems[i].SetPlayer(GameManager.Players[i]);
        }

        for (int i = GameManager.Players.Count; i < lobbyItems.Length; i++)
        {
            lobbyItems[i].ClearPlayer();
        }

        startGameButton.interactable =
            (GameManager.Players.Count >= GameNetworkManager.MinConnections);
    }
    
    private void HandlePartyOwnerStateChange()
    {
        if (!NetworkClient.connection.identity) return;

        bool isLeader = NetworkClient.connection.identity.GetComponent<PlayerInfo>().IsPartyLeader;

        startGameButton.gameObject.SetActive(isLeader);
    }

    #endregion
}