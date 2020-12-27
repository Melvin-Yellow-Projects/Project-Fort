using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;

public class DebugPlayer : NetworkBehaviour
{
    #region Variables
    [SerializeField] TMP_Text displayNameText;

    [SerializeField] GameObject playerBody;

    [SyncVar(hook = nameof(HookOnDisplayName))]
    string displayName;

    [SyncVar]
    [SerializeField] float speed = 1;

    #endregion

    #region Events

    public static event Action ClientPlayerInfoUpdated;

    #endregion

    #region Properties

    public string DisplayName
    {
        get
        {
            return displayName;
        }

        [Server]
        set
        {
            displayName = value;
        }
    }

    #endregion

    #region Unity Functions

    private void FixedUpdate()
    {
        if (!hasAuthority) return;

        if (Input.GetKey("a")) CmdMovePlayer(-1);
        if (Input.GetKey("d")) CmdMovePlayer(1);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        DebugPlayer otherPlayer = other.GetComponent<DebugPlayer>();
        // see player
        otherPlayer.playerBody.SetActive(true);

        Debug.Log($"{name} can now see {other.name}!");
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        DebugPlayer otherPlayer = other.GetComponent<DebugPlayer>();
        // hide player
        otherPlayer.playerBody.SetActive(false);

        Debug.Log($"{name} can no longer see {other.name}..");
    }

    #endregion

    #region Server Functions

    public override void OnStartServer()
    {
        ClientPlayerInfoUpdated += HandleClientPlayerInfoUpdated;
    }

    [Command]
    private void CmdMovePlayer(float direction)
    {
        RpcMovePlayer(direction);
    }

    private void ServerSeePlayer()
    {

    }

    private void ServerHidePlayer()
    {

    }

    #endregion

    #region Client Functions

    public override void OnStartClient()
    {
        if (!isClientOnly) return;

        ClientPlayerInfoUpdated += HandleClientPlayerInfoUpdated;
    }

    public override void OnStopClient()
    {
        ClientPlayerInfoUpdated -= HandleClientPlayerInfoUpdated;
    }

    [ClientRpc]
    private void RpcMovePlayer(float direction)
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed;

        transform.position = pos;
    }

    private void ClientSeePlayer()
    {

    }

    private void ClientHidePlayer()
    {

    }

    #endregion

    #region Handle Functions

    private void HandleClientPlayerInfoUpdated()
    {
        displayNameText.text = displayName;
    }

    private void HookOnDisplayName(string oldValue, string newValue)
    {
        ClientPlayerInfoUpdated?.Invoke();
    }

    #endregion

}
