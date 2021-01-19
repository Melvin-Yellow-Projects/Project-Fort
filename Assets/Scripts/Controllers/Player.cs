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

    protected List<Unit> myUnits = new List<Unit>();

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

    public int MoveCount { get; [Server] set; } = 1;

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
        if (!hasAuthority) return;

        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;

        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
    }

    protected virtual void Unsubscribe()
    {
        if (!hasAuthority) return;

        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;
    }

    protected virtual void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.MyTeam != MyTeam) return;

        myUnits.Add(unit);
    }

    protected virtual void HandleOnUnitDepawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    [Server]
    protected virtual void HandleServerOnStartTurn()
    {
        MoveCount = 0;
    }

    #endregion
}