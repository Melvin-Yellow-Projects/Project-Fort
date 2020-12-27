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
            //displayNameText.text = value;
        }
    }

    #endregion
    /************************************************************/
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

    public override void OnStartServer()
    {
        DebugPlayer.OnClientPlayerJoined += HandleOnClientPlayerJoined;
    }

    [Command]
    private void CmdMovePlayer(float direction)
    {
        RpcMovePlayer(direction);
    }

    [Server]
    private void ServerSeeUnit(DebugUnit unit)
    {
        RpcSeeUnit(unit.netIdentity);

        if (!hasAuthority) return;

        unit.displayNameText.gameObject.SetActive(true);
        unit.unitBody.SetActive(true);

        Debug.Log($"I am the server unit and I can now see {unit.name}!");
    }

    [Server]
    private void ServerHideUnit(DebugUnit unit)
    {
        RpcHideUnit(unit.netIdentity);

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
        if (!isClientOnly) return;

        DebugPlayer.OnClientPlayerJoined += HandleOnClientPlayerJoined;
    }

    public override void OnStopClient()
    {
        DebugPlayer.OnClientPlayerJoined -= HandleOnClientPlayerJoined;
    }

    [ClientRpc]
    private void RpcMovePlayer(float direction)
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed;

        transform.position = pos;
    }

    [ClientRpc]
    private void RpcSeeUnit(NetworkIdentity unitIdentity)
    {
        if (!hasAuthority) return;

        // spawn unit
        //Instantiate(unitIdentity.gameObject);

        Debug.Log($"I can now see {unitIdentity.name}!");
    }

    [ClientRpc]
    private void RpcHideUnit(NetworkIdentity unitIdentity)
    {
        if (!hasAuthority) return;

        // destroy unit
        //Destroy(unitIdentity.gameObject);

        Debug.Log($"I can now see {unitIdentity.name}!");
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
}
