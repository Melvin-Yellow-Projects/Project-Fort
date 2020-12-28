using System.Collections;
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
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void FixedUpdate()
    {
        if (!hasAuthority) return; // HACK: could be replaced with a [clientcallback] or something

        if (Input.GetKey("a")) CmdMoveUnit(-1);
        if (Input.GetKey("d")) CmdMoveUnit(1);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        DebugUnit otherUnit = other.GetComponentInParent<DebugUnit>();

        ServerSeeUnit(otherUnit);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        DebugUnit otherUnit = other.GetComponentInParent<DebugUnit>();

        ServerHideUnit(otherUnit);
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Command]
    private void CmdMoveUnit(float direction)
    {
        if (!hasAuthority) return;

        RpcMoveUnit(direction);
    }

    [Server]
    private void ServerSeeUnit(DebugUnit unit)
    {
        //RpcSeeUnit(unit.netIdentity);

        //TargetSeeUnit(unit.netIdentity);

        if (!hasAuthority) return;

        unit.displayNameText.gameObject.SetActive(true);
        unit.unitBody.SetActive(true);

        Debug.Log($"I am the server unit and I can now see {unit.name}!");
    }

    [Server]
    private void ServerHideUnit(DebugUnit unit)
    {
        //RpcHideUnit(unit.netIdentity);

        if (!hasAuthority) return;

        unit.displayNameText.gameObject.SetActive(false);
        unit.unitBody.SetActive(false);

        Debug.Log($"I am the server unit and I can no longer see {unit.name}..");
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        DebugPlayer.OnClientPlayerJoined += HandleOnClientPlayerJoined;

        if (hasAuthority)
        {
            displayNameText.gameObject.SetActive(true);
            unitBody.SetActive(true);
        }
        else
        {
            NetworkServer.UnSpawn(gameObject);
        }
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
    private void TargetSeeUnit()
    {

    }

    //[ClientRpc]
    //private void RpcSeeUnit(NetworkIdentity unitIdentity)
    //{
    //    if (!hasAuthority) return;

    //    NetworkServer.Spawn(unitIdentity.gameObject);

    //    Debug.Log($"I can now see {unitIdentity.name}!");
    //}

    //[ClientRpc]
    //private void RpcHideUnit(NetworkIdentity unitIdentity)
    //{
    //    if (!hasAuthority) return;

    //    NetworkServer.UnSpawn(unitIdentity.gameObject);

    //    Debug.Log($"I can no longer see {unitIdentity.name}..");
    //}

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
}
