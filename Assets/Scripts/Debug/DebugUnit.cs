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
            //displayNameText.text = value;
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

    //[ServerCallback]
    //private void OnTriggerEnter(Collider other)
    //{
    //    DebugUnit otherUnit = other.GetComponentInParent<DebugUnit>();

    //    ServerShowUnit(otherUnit);
    //}

    //[ServerCallback]
    //private void OnTriggerExit(Collider other)
    //{
    //    DebugUnit otherUnit = other.GetComponentInParent<DebugUnit>();

    //    ServerHideUnit(otherUnit);
    //}

    #endregion
    /************************************************************/
    #region Server Functions

    [Command]
    private void CmdMoveUnit(float direction)
    {
        RpcMoveUnit(direction);
    }

    [Server]
    private void ServerShowUnit(DebugUnit unit)
    {
        if (hasAuthority) // shows unit for host
        {
            unit.displayNameText.gameObject.SetActive(true);
            unit.unitBody.SetActive(true);
        }
        else
        {
            TargetShowUnit(connectionToClient, unit.netIdentity);
        }
    }

    [Server]
    private void ServerHideUnit(DebugUnit unit)
    {
        if (hasAuthority) // hides unit for host
        {
            unit.displayNameText.gameObject.SetActive(false);
            unit.unitBody.SetActive(false);
        }
        else
        {
            TargetHideUnit(connectionToClient, unit.netIdentity);
        }
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        DebugPlayer.OnClientPlayerJoined += HandleOnClientPlayerJoined;

        if (!hasAuthority) return;
    }

    public override void OnStopClient()
    {
        DebugPlayer.OnClientPlayerJoined -= HandleOnClientPlayerJoined;
    }

    [ClientRpc]
    private void RpcMoveUnit(float direction)
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed;

        transform.position = pos;
    }

    [TargetRpc]
    private void TargetShowUnit(NetworkConnection target, NetworkIdentity unitIdentity)
    {
        DebugUnit unit = unitIdentity.GetComponent<DebugUnit>();

        Debug.Log($"I need to show unit{unit.name}");
    }

    [TargetRpc]
    private void TargetHideUnit(NetworkConnection target, NetworkIdentity unitIdentity)
    {
        DebugUnit unit = unitIdentity.GetComponent<DebugUnit>();

        Debug.Log($"I need to hide unit{unit.name}");
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
    /************************************************************/
    #region Handle Functions

    [TargetRpc]
    public void TargetRegisterPrefab(NetworkConnection target)
    {
        DebugUnit unitInstance = Instantiate(gameObject).GetComponent<DebugUnit>();

        System.Guid unitAssetId = unitInstance.GetComponent<NetworkIdentity>().assetId;

        //ClientScene.UnregisterSpawnHandler(unitAssetId);

        ClientScene.RegisterPrefab(unitInstance.gameObject, HandleSpawnUnit, HandleUnSpawnUnit);
        ClientScene.RegisterSpawnHandler(unitAssetId, HandleSpawnUnit, HandleUnSpawnUnit);

        NetworkServer.Spawn(unitInstance.gameObject, unitAssetId, target);

        //unit.DisplayName = $"Player {Players.Count}";
    }

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
