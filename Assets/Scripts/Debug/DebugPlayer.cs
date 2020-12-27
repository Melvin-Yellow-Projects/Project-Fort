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

    private void Update()
    {
        if (!hasAuthority) return;

        if (Input.GetKey("a")) CmdMovePlayer(-1);
        if (Input.GetKey("d")) CmdMovePlayer(1);
    }

    #endregion

    #region Server Functions

    public override void OnStartServer()
    {
        ClientPlayerInfoUpdated += HandleClientPlayerInfoUpdated;
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

    [Command]
    public void CmdMovePlayer(float direction)
    {
        //RpcMovePlayer(direction);

        Vector3 pos = transform.position;

        pos.x += direction * speed * Time.deltaTime;

        Debug.Log(Time.deltaTime);

        transform.position = pos;
    }

    [ClientRpc]
    public void RpcMovePlayer(float direction)
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed * Time.deltaTime;

        Debug.Log(Time.deltaTime);

        transform.position = pos;
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
