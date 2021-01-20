﻿/**
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

    public List<Unit> MyUnits { get; set; } = new List<Unit>(); // TODO: change it to be just server

    public List<Fort> MyForts { [Server] get; [Server] set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Unity Functions

    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //}

    public override void OnStartAuthority()
    {
        // HACK: this probably doesn't belong here, but calling it on awake causes authority errors
        Subscribe();
    }

    ///// <summary>
    ///// Unity Method; Awake() is called before Start() upon GameObject creation
    ///// </summary>
    //protected virtual void Awake()
    //{
    //    Subscribe();
    //}

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    protected virtual void Subscribe()
    {
        if (!isClientOnly || !hasAuthority) return; // TODO: validate this works

        Debug.Log($"Player {name} is either server or has authority of it's own player object");

        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;

        if (!isClientOnly) return;

        Debug.Log($"Player is the server {name}");

        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;
    }

    protected virtual void Unsubscribe()
    {
        if (!isServer || !hasAuthority) return;

        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;
        
        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;

        if (!isServer) return;

        Fort.ServerOnFortCaptured -= HandleServerOnFortCaptured;
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

            if (MyForts.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Conquest);
        }
        else if (MyTeam == newTeam)
        {
            MyForts.Add(fort);
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