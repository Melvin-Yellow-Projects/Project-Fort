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
        SceneLoader.StopConnectionAndLoadStartScene();
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
        GameSession.OnClientDisconnectEvent += RefreshLobbyItems;

        PlayerInfo.ClientOnPartyLeaderChanged += HandleClientOnPartyLeaderChanged;
        PlayerInfo.ClientOnPlayerInfoUpdate += RefreshLobbyItems;
        
        Team.ClientOnChangeTeam += RefreshLobbyItems;
    }

    public void Unsubscribe()
    {
        hasSubscribed = false;
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
        GameSession.OnClientDisconnectEvent -= RefreshLobbyItems;

        PlayerInfo.ClientOnPartyLeaderChanged -= HandleClientOnPartyLeaderChanged;
        PlayerInfo.ClientOnPlayerInfoUpdate -= RefreshLobbyItems;
        
        Team.ClientOnChangeTeam -= RefreshLobbyItems;
    }

    private void HandleOnClientConnectEvent()
    {
        gameObject.SetActive(true);
    }

    private void HandleClientOnPartyLeaderChanged()
    {
        if (!NetworkClient.connection.identity) return; // HACK is this lined needed?

        bool isLeader = NetworkClient.connection.identity.GetComponent<PlayerInfo>().IsPartyLeader;

        startGameButton.gameObject.SetActive(isLeader);

        gameSettingsMenu.Interactable = isLeader;

        RefreshLobbyItems();
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

    #endregion
}