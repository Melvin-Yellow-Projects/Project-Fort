/**
 * File Name: PreLobbyMenu.cs
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
using UnityEngine.UI;
using Mirror;
using Steamworks;
using TMPro;

public class PreLobbyMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] GameObject joiningLobbyMenu = null;
    [SerializeField] TMP_InputField addressInput = null;

    [SerializeField] TMP_Text debugText = null;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        if (GameNetworkManager.IsUsingSteam) InitializeCallbacks();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void HostLobby()
    {
        if (GameNetworkManager.IsUsingSteam) HostLobbyWithSteam();
        else NetworkManager.singleton.StartHost();
    }

    public void JoinLobby()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();
    }

    public void CancelJoinLobby()
    {
        // TODO: this needs to disconnect the client that calls this function
        //GameNetworkManager.Singleton.StopClient();
        //NetworkClient.connection.Disconnect();
        NetworkManager.singleton.StopClient();
        Debug.LogWarning("Cancel Join Lobby Not Yet Implemented");
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameNetworkManager.OnClientConnectEvent += HandleOnClientConnectEvent;
        //GameNetworkManager.OnClientDisconnected += HandleOnClientDisconnected;
    }

    private void Unsubscribe()
    {
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
        //GameNetworkManager.OnClientDisconnected -= HandleOnClientDisconnected;
    }

    private void HandleOnClientConnectEvent()
    {
        joiningLobbyMenu.SetActive(false);
        gameObject.SetActive(false);
    }

    #endregion
    /************************************************************/
    #region Steam Functions

    private void InitializeCallbacks()
    {
        Debug.Log("starting SetupSteamCallbacks");
        debugText.text += ">starting SetupSteamCallbacks\n";

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        Debug.Log("completed SetupSteamCallbacks");
        debugText.text += ">completed SetupSteamCallbacks\n";
    }

    private void HostLobbyWithSteam()
    {
        Debug.Log("starting SteamMatchmaking.CreateLobby");
        debugText.text += ">starting SteamMatchmaking.CreateLobby\n";

        SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypeFriendsOnly,
            NetworkManager.singleton.maxConnections
        );

        Debug.Log("completed SteamMatchmaking.CreateLobby");
        debugText.text += ">completed SteamMatchmaking.CreateLobby\n";
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("starting OnLobbyCreated");
        debugText.text += ">starting OnLobbyCreated\n";

        if (callback.m_eResult != EResult.k_EResultOK)
        {
            //landingPagePanel.SetActive(true);
            Debug.LogError("Callback didn't work idk");
            return;
        }

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        GameNetworkManager.LobbyId = lobbyId.m_SteamID;

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(
            lobbyId,
            "HostAddress",
            SteamUser.GetSteamID().ToString());

        Debug.Log("completed OnLobbyCreated");
        debugText.text += ">completed OnLobbyCreated\n";

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("starting OnGameLobbyJoinRequested");
        debugText.text += ">starting OnGameLobbyJoinRequested\n";

        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

        Debug.Log("completed OnGameLobbyJoinRequested");
        debugText.text += ">completed OnGameLobbyJoinRequested\n";
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("starting OnLobbyEntered");
        debugText.text += ">starting OnLobbyEntered\n";

        if (NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "HostAddress");

        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();

        //landingPagePanel.SetActive(false);
        gameObject.SetActive(false); // HACK: this might be wrong...?

        Debug.Log("completed OnLobbyEntered");
        debugText.text += ">completed OnLobbyEntered\n";
    }

    #endregion

}
