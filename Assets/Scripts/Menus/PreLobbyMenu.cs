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
using TMPro;

public class PreLobbyMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] TMP_InputField addressInput = null;
    [SerializeField] GameObject joiningLobbyMenu = null;
    [SerializeField] GameObject lobbyMenu = null;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void OnEnable()
    {
        Subscribe();
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
        lobbyMenu.SetActive(true);

        // TODO: Steam integration

        NetworkManager.singleton.StartHost();
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
        Debug.LogWarning("Cancel Joing Lobby Not Yet Implemented");
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameNetworkManager.OnClientConnected += HandleOnClientConnected;
        GameNetworkManager.OnClientDisconnected += HandleOnClientDisconnected;
    }

    private void Unsubscribe()
    {
        GameNetworkManager.OnClientConnected -= HandleOnClientConnected;
        GameNetworkManager.OnClientDisconnected -= HandleOnClientDisconnected;
    }

    private void HandleOnClientConnected()
    {
        joiningLobbyMenu.SetActive(false);
        gameObject.SetActive(false);
    }

    private void HandleOnClientDisconnected()
    {
        joiningLobbyMenu.SetActive(false);
    }

    #endregion
}
