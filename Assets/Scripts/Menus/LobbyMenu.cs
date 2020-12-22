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
    //[SerializeField] RawImage[] playerSteamImages = new RawImage[0];

    bool hasSubscribed = false;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void StartGame()
    {
        //NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    public void Subscribe()
    {
        if (hasSubscribed) return;
        hasSubscribed = true;

        GameNetworkManager.OnClientConnected += HandleOnClientConnected;
        //RTSPlayerInfo.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        //RTSPlayerInfo.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void Unsubscribe()
    {
        GameNetworkManager.OnClientConnected -= HandleOnClientConnected;
        //RTSPlayerInfo.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        //RTSPlayerInfo.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void HandleOnClientConnected()
    {
        gameObject.SetActive(true);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    private void ClientHandleInfoUpdated()
    {
        List<HumanPlayer> players = GameNetworkManager.Singleton.HumanPlayers;

        for (int i = 0; i < players.Count; i++)
        {
            // TODO: add player avatar and name
            playerNameTexts[i].text = $"Player {i + 1}";
            playerNameTexts[i].GetComponent<EllipsisSetter>().enabled = false;
            //playerNameTexts[i].text = players[i].GetComponent<RTSPlayerInfo>().DisplayName;
            //playerSteamImages[i].texture = players[i].GetComponent<RTSPlayerInfo>().DisplayTexture;
        }

        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player";
            playerNameTexts[i].GetComponent<EllipsisSetter>().enabled = true;
            //playerSteamImages[i].texture = null;
        }

        startGameButton.interactable = (players.Count >= 2);
    }

    #endregion
}