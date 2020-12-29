using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugNetworkManager : NetworkManager
{
    /************************************************************/
    #region Variables

    [SerializeField] GameObject unitPrefab;
    System.Guid unitAssetId;

    #endregion
    /************************************************************/
    #region Properties

    public List<DebugPlayer> Players { get; } = new List<DebugPlayer>();

    #endregion
    /************************************************************/
    #region Server Functions

    bool isInitialized = false;

    [Server]
    public void InitializePrefab()
    {
        if (isInitialized) return;

        isInitialized = true;

        unitAssetId = unitPrefab.GetComponent<NetworkIdentity>().assetId;

        //ClientScene.UnregisterSpawnHandler(unitAssetId);

        ClientScene.RegisterPrefab(unitPrefab, HandleSpawnUnit, HandleUnSpawnUnit);

        //ClientScene.RegisterSpawnHandler(unitAssetId, HandleSpawnUnit, HandleUnSpawnUnit);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        InitializePrefab();

        DebugPlayer player = conn.identity.GetComponent<DebugPlayer>();

        Players.Add(player);

        DebugUnit unit = Instantiate(unitPrefab).GetComponent<DebugUnit>();

        NetworkServer.Spawn(unit.gameObject, unitAssetId, conn);

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
    #region Handle Functions

    public GameObject HandleSpawnUnit(SpawnMessage msg)
    {
        Debug.Log("Calling Custom Spawn Method");
        //return Instantiate(spawnPrefabs[0], msg.position, msg.rotation);
        return null;
    }

    public void HandleUnSpawnUnit(GameObject spawned)
    {
        Debug.Log("Calling Custom UnSpawn Method");
        Destroy(spawned);
    }

    #endregion
}
