/**
 * File Name: Player.cs
 * Description: TODO: comment script
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *      
 *      Previously known as HexGameUI.cs
 **/

using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// TODO: comment class
/// </summary>
public abstract class Player : NetworkBehaviour
{
    /************************************************************/
    #region Class Events

    // FIXME: Verify Player Menu

    /// <summary>
    /// Server event for when a player has been defeated
    /// </summary>
    /// <subscriber class="GameOverHandler">handles the defeated player</subscriber>
    public static event Action<Player, WinConditionType> ServerOnPlayerDefeat;

    #endregion
    /************************************************************/
    #region Properties

    public Team MyTeam
    {
        get
        {
            return GetComponent<Team>();
        }
    }

    public int MoveCount { get; [Server] set; } = 1; // FIXME: Change this such that it works

    public List<Unit> MyUnits { get; set; } = new List<Unit>(); 

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        Subscribe();
    }

    public override void OnStopServer()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (!isClientOnly) return;

        GameManager.Players.Add(this);

        if (!hasAuthority) return;

        Subscribe();
    }

    public override void OnStopClient()
    {
        if (!isClientOnly) return;

        GameManager.Players.Remove(this);

        if (!hasAuthority) return;

        Unsubscribe();
    }

    [TargetRpc]
    public void RpcAddFort(NetworkConnection conn, NetworkIdentity fortNetId)
    {
        if (isServer) return;

        MyForts.Add(fortNetId.GetComponent<Fort>());
    }

    [TargetRpc]
    public void RpcRemoveFort(NetworkConnection conn, NetworkIdentity fortNetId)
    {
        if (isServer) return;

        MyForts.Remove(fortNetId.GetComponent<Fort>());
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    // HACK this line does not work, subscribe needs to happen on server and authoritive client
    protected virtual void Subscribe()
    {  
        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        if (!isServer) return;

        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;

        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
    }

    protected virtual void Unsubscribe()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        if (!isServer) return;

        Fort.ServerOnFortCaptured -= HandleServerOnFortCaptured;

        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;
    }

    private void HandleOnFortSpawned(Fort fort)
    {
        if (fort.MyTeam != MyTeam) return;

        MyForts.Add(fort);
    }

    // HACK: this is probably unneeded
    private void HandleOnFortDespawned(Fort fort)
    {
        MyForts.Remove(fort);
    }

    [Server]
    private void HandleServerOnFortCaptured(Fort fort, Team newTeam)
    {
        if (MyTeam == fort.MyTeam)
        {
            MyForts.Remove(fort);
            RpcRemoveFort(connectionToClient, fort.netIdentity);

            // HACK: this is a temp fix
            if (!isServer) return;
            if (MyForts.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Conquest);
        }
        else if (MyTeam == newTeam)
        {
            MyForts.Add(fort);
            RpcAddFort(connectionToClient, fort.netIdentity);

            //Debug.LogWarning($"team has {MyForts.Count} forts out of {HexGrid.Forts.Count} total");
            // HACK: computer players are not yet handled
            if (!GameSession.Singleton.IsOnline && MyForts.Count == HexGrid.Forts.Count)
            {
                ServerOnPlayerDefeat?.Invoke(null, WinConditionType.TEST);
            }
        }

    }

    protected virtual void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.MyTeam != MyTeam) return;

        MyUnits.Add(unit);
    }

    // HACK: on death would be more useful
    protected virtual void HandleOnUnitDepawned(Unit unit)
    {
        MyUnits.Remove(unit);

        if (!isServer) return;

        if (MyUnits.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Annihilation);
    }

    [Server]
    protected virtual void HandleServerOnStartTurn()
    {
        MoveCount = 0;
    }

    #endregion
}