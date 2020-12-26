using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugNetworkManager : NetworkManager
{
    #region Properties

    public List<DebugPlayer> Players { get; } = new List<DebugPlayer>();

    #endregion

    #region Server Functions

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        DebugPlayer player = conn.identity.GetComponent<DebugPlayer>();

        Players.Add(player);

        player.DisplayName = $"Player {Players.Count}";
    }

    #endregion
}
