/**
 * File Name: LobbyMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class LobbyMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] SaveLoadMenu saveLoadMenu = null;
    [SerializeField] GameSettingsMenu gameSettingsMenu = null;

    [SerializeField] Button startGameButton = null;
    [SerializeField] LobbyItem[] lobbyItems = null;

    bool hasSubscribed = false;

    #endregion
    /************************************************************/
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
    /************************************************************/
    #region Class Functions

    public void StartGame()
    {
        if (ArePlayersOnDifferentTeams()) return;

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

    private bool ArePlayersOnDifferentTeams()
    {
        for (int i = 0; i < GameManager.Players.Count - 1; i++)
        {
            Player player = GameManager.Players[i];

            for (int j = i + 1; j < GameManager.Players.Count; j++)
            {
                if (player.MyTeam == GameManager.Players[j].MyTeam) return true;
            }
        }

        return false;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    public void Subscribe()
    {
        if (hasSubscribed) return;
        hasSubscribed = true;

        GameNetworkManager.OnClientConnectEvent += HandleOnClientConnectEvent;
        GameNetworkManager.OnClientDisconnectEvent += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate += HandlePartyLeaderStateChange;
        Team.ClientOnChangeTeam += RefreshLobbyItems;
    }

    public void Unsubscribe()
    {
        hasSubscribed = false;
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
        GameNetworkManager.OnClientDisconnectEvent += RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate -= RefreshLobbyItems;
        PlayerInfo.ClientOnPlayerInfoUpdate -= HandlePartyLeaderStateChange;
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

        startGameButton.interactable = !ArePlayersOnDifferentTeams() &&
            (GameManager.Players.Count >= GameNetworkManager.MinConnections);
    }
    
    private void HandlePartyLeaderStateChange()
    {
        if (!NetworkClient.connection.identity) return;

        bool isLeader = NetworkClient.connection.identity.GetComponent<PlayerInfo>().IsPartyLeader;

        startGameButton.gameObject.SetActive(isLeader);

        gameSettingsMenu.Get();
    }

    #endregion
}