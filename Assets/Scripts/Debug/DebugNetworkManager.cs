using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugNetworkManager : NetworkManager
{
    /************************************************************/
    #region Variables

    [SerializeField] DebugUnit unitPrefab;

    #endregion
    /************************************************************/
    #region Properties

    public List<DebugPlayer> Players { get; } = new List<DebugPlayer>();

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        DebugPlayer player = conn.identity.GetComponent<DebugPlayer>();

        Players.Add(player);

        //GameObject unitPrefab = spawnPrefabs[0];

        DebugUnit unit = Instantiate(unitPrefab);
        unit.RegisterSpawnHandler();
        NetworkServer.Spawn(unit.gameObject, conn);

        unit.DisplayName = $"Player {Players.Count}";
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        DebugPlayer player = conn.identity.GetComponent<DebugPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);


    }

    public override void OnStopServer()
    {
        Players.Clear();
    }

    #endregion
    /************************************************************/
}
