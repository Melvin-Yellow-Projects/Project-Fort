using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class DebugPlayer : NetworkBehaviour
{
    /************************************************************/
    #region Events

    public static event Action OnClientPlayerJoined;

    #endregion
    /************************************************************/
    #region Properties

    public DebugUnit MyUnit { get; [Server] set; }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    { 
        OnClientPlayerJoined?.Invoke();
    }

    public override void OnStopClient()
    {
        OnClientPlayerJoined?.Invoke();
    }

    #endregion
    /************************************************************/
}
