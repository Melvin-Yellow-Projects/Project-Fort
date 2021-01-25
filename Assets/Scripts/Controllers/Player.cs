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
    #region Variables

    [SyncVar(hook = nameof(HookOnMoveCount))]
    private int moveCount = 1;

    #endregion
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

    public int MoveCount
    {
        get
        {
            return moveCount;
        }
        set
        {
            moveCount = value;
        }
    }

    public List<Unit> MyUnits { get; set; } = new List<Unit>(); 

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);
        Subscribe();
    }

    public override void OnStopServer()
    {
        Unsubscribe();
    }

    [Command]
    public virtual void CmdSetAction(UnitData data)
    {
        if (GameManager.IsPlayingTurn) return;
        if (MoveCount > GameMode.Singleton.MovesPerTurn) return;

        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        // TODO: verify that a player can't send the cell theyre currently on
        if (data.MyUnit.Movement.ServerSetAction(data)) MoveCount++;
    }

    [Command]
    public virtual void CmdClearAction(UnitData data)
    {
        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        if (data.MyUnit.Movement.ServerClearAction()) MoveCount--;
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (!isClientOnly) return;

        DontDestroyOnLoad(gameObject);

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

    // HACK: can probably replace with psuedo Handler function since it's specifically called by a
    // handler
    [TargetRpc] 
    public void TargetAddFort(NetworkConnection target, NetworkIdentity fortNetId)
    {
        if (isServer) return;

        MyForts.Add(fortNetId.GetComponent<Fort>());
    }

    // HACK: can probably replace with psuedo Handler function since it's specifically called by a
    // handler
    [TargetRpc]
    public void TargetRemoveFort(NetworkConnection target, NetworkIdentity fortNetId)
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

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        if (!isServer) return;

        UnitDeath.ServerOnUnitDeath += HandleServerOnUnitDeath;

        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;

        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
    }

    protected virtual void Unsubscribe()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        if (!isServer) return;

        UnitDeath.ServerOnUnitDeath -= HandleServerOnUnitDeath;

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
    private void HandleServerOnUnitDeath(Unit unit)
    {
        MyUnits.Remove(unit);
        if (connectionToClient != null) HandleTargetOnUnitDeath(unit.netIdentity);

        if (MyUnits.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Annihilation);
    }

    [TargetRpc]
    private void HandleTargetOnUnitDeath(NetworkIdentity unitNetId)
    {
        if (isServer) return;

        MyUnits.Remove(unitNetId.GetComponent<Unit>());
    }

    [Server] // HACK: this function is so jank
    private void HandleServerOnFortCaptured(Fort fort, Team newTeam)
    {
        if (MyTeam == fort.MyTeam)
        {
            MyForts.Remove(fort);
            if (connectionToClient != null) TargetRemoveFort(connectionToClient, fort.netIdentity);

            if (MyForts.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Conquest);
        }
        else if (MyTeam == newTeam)
        {
            MyForts.Add(fort);
            if (connectionToClient != null) TargetAddFort(connectionToClient, fort.netIdentity);
        }
    }

    private void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.MyTeam != MyTeam) return;

        MyUnits.Add(unit);
    }

    [Server]
    private void HandleServerOnStartTurn()
    {
        MoveCount = 1;
    }

    [Client]
    protected virtual void HookOnMoveCount(int oldValue, int newValue) { }

    #endregion
}