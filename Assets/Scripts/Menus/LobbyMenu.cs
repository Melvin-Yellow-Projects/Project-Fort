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
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] Button startGameButton = null;
    [SerializeField] TMP_Text[] playerNameTexts = new TMP_Text[0];
    [SerializeField] RawImage[] playerSteamImages = new RawImage[0];

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
        PlayerInfo.ClientOnPlayerInfoUpdate += UpdatePlayerTags;
        PlayerInfo.ClientOnPlayerInfoUpdate += HandlePartyOwnerStateChange;
    }

    public void Unsubscribe()
    {
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
        PlayerInfo.ClientOnPlayerInfoUpdate -= UpdatePlayerTags;
        PlayerInfo.ClientOnPlayerInfoUpdate -= HandlePartyOwnerStateChange;
    }

    private void HandleOnClientConnectEvent()
    {
        gameObject.SetActive(true);
    }

    private void UpdatePlayerTags()
    {
        for (int i = 0; i < GameManager.Players.Count; i++)
        {
            Player player = GameManager.Players[i];

            playerNameTexts[i].text = $"Player {i + 1}";
            playerNameTexts[i].GetComponent<EllipsisSetter>().enabled = false;
            playerNameTexts[i].text = player.GetComponent<PlayerInfo>().PlayerName;
            playerSteamImages[i].texture = player.GetComponent<PlayerInfo>().DisplayTexture;
        }

        for (int i = GameManager.Players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player";
            playerNameTexts[i].GetComponent<EllipsisSetter>().enabled = true;
            playerSteamImages[i].texture = null;
        }

        startGameButton.interactable =
            (GameManager.Players.Count >= GameNetworkManager.MinConnections);
    }

    private void HandlePartyOwnerStateChange()
    {
        if (!NetworkClient.connection.identity) return;
        if (!NetworkClient.connection.identity.GetComponent<PlayerInfo>().IsPartyOwner) return;

        startGameButton.gameObject.SetActive(true);
    }

    #endregion
}