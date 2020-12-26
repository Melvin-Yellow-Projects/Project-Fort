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
