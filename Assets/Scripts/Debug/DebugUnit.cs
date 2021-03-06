﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;


public class DebugUnit : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] TMP_Text displayNameText;
    [SerializeField] GameObject unitBody;

    [Header("Settings")]
    [SyncVar]
    [SerializeField] float speed = 1;

    /* Other Variables */
    [SyncVar(hook = nameof(HookOnDisplayName))]
    string displayName;

    //GameObject prefab = null;

    #endregion
    /************************************************************/
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
            displayNameText.text = displayName;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void FixedUpdate()
    {
        if (!hasAuthority) return;

        if (Input.GetKey("a")) CmdMoveUnit(-1);
        if (Input.GetKey("d")) CmdMoveUnit(1);
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Command]
    private void CmdMoveUnit(float direction)
    {
        // validation logic would go here
        //if (!hasAuthority) return;

        MoveUnit(direction);
        //RpcMoveUnit(direction); // Does Not Need This Line if NetworkTransform is attached
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        displayNameText.enabled = true;
        DebugPlayer.OnClientPlayerJoined += HandleOnClientPlayerJoined;

        if (!hasAuthority) return;
    }

    public override void OnStopClient()
    {
        DebugPlayer.OnClientPlayerJoined -= HandleOnClientPlayerJoined;
        displayNameText.enabled = false;
    }

    [ClientRpc]
    private void RpcMoveUnit(float direction)
    {
        if (!isClientOnly) return;
        MoveUnit(direction);
    }
    #endregion
    /************************************************************/
    #region Class Functions

    private void MoveUnit(float direction)
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed;

        transform.position = pos;
    }

    #endregion
    /************************************************************/
    #region Handle Functions

    private void HandleOnClientPlayerJoined()
    {
        displayNameText.text = displayName;
    }

    private void HookOnDisplayName(string oldValue, string newValue)
    {
        displayNameText.text = displayName;
    }

    #endregion
}
