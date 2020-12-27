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
        DebugPlayer otherPlayer = other.GetComponentInParent<DebugPlayer>();

        ServerSeePlayer(otherPlayer);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        DebugPlayer otherPlayer = other.GetComponentInParent<DebugPlayer>();
        
        ServerHidePlayer(otherPlayer);
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

    [Server]
    private void ServerSeePlayer(DebugPlayer player)
    {
        RpcSeePlayer(player);

        if (!hasAuthority) return;

        player.playerBody.SetActive(true);

        Debug.Log($"I am the server player and I can now see {player.name}!");
    }

    [Server]
    private void ServerHidePlayer(DebugPlayer player)
    {
        RpcSeePlayer(player);

        if (!hasAuthority) return;

        player.playerBody.SetActive(false);

        Debug.Log($"I am the server player and I can no longer see {player.name}..");
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

    [ClientRpc]
    private void RpcSeePlayer(DebugPlayer player)
    {
        if (!hasAuthority) return;

        player.playerBody.SetActive(true);

        Debug.Log($"I can now see {player.name}!");
    }

    [ClientRpc]
    private void RpcHidePlayer(DebugPlayer player)
    {
        if (!hasAuthority) return;

        player.playerBody.SetActive(false);

        Debug.Log($"I can now see {player.name}!");
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
