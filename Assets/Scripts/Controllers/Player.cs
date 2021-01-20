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

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    public List<Unit> MyUnits { get; set; } = new List<Unit>();

    #endregion
    /************************************************************/
    #region Unity Functions

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
        if (!isServer || !hasAuthority) return; // TODO: validate this works

        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
    }

    protected virtual void Unsubscribe()
    {
        if (!isServer || !hasAuthority) return;

        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;
    }

    private void HandleOnFortSpawned(Fort fort)
    {
        if (fort.MyTeam != MyTeam) return;

        MyForts.Add(fort);
    }

    private void HandleOnFortDespawned(Fort fort)
    {
        MyForts.Remove(fort);
    }

    protected virtual void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.MyTeam != MyTeam) return;

        MyUnits.Add(unit);
    }

    protected virtual void HandleOnUnitDepawned(Unit unit)
    {
        MyUnits.Remove(unit);
    }

    [Server]
    protected virtual void HandleServerOnStartTurn()
    {
        MoveCount = 0;
    }

    #endregion
}