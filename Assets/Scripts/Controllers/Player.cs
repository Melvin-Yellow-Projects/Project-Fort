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
    int moveCount = 0; // HACK: wrong, other clients shouldnt know this data.

    [SyncVar(hook = nameof(HookOnResources))]
    int resources = 0; // HACK: wrong, other clients shouldnt know this data, or should they? 

    bool hasEndedTurn = false;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Server event for when a player has been defeated
    /// </summary>
    /// <subscriber class="GameOverHandler">handles the defeated player</subscriber>
    public static event Action<Player, WinConditionType> ServerOnPlayerDefeat;

    /// <summary>
    /// Client event for when a player's resources have updated
    /// </summary>
    //public static event Action ClientOnResourcesUpdated;

    /// <summary>
    /// Client event for when a player has ended their turn
    /// FIXME: shouldnt this event just be for the Human Player?
    /// </summary>
    /// <subscriber class="PlayerMenu">Updates the player menu status</subscriber>
    public static event Action ClientOnHasEndedTurn; 

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

    public int Resources
    {
        get
        {
            return resources;
        }
        set
        {
            resources = value;
        }
    }

    public bool HasEndedTurn
    {
        get
        {
            return hasEndedTurn;
        }
        set
        {
            hasEndedTurn = value;
            if (isServer && connectionToClient != null) TargetSetHasEndedTurn(value);
        }
    }

    public List<Unit> MyUnits { get; set; } = new List<Unit>(); 

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Unity Functions

    //protected void Awake()
    //{
    //    DontDestroyOnLoad(gameObject);
    //    if (isServer || hasAuthority) Subscribe();
    //}

    //protected void OnDestroy()
    //{
    //    if (isServer || hasAuthority) Unsubscribe();
    //}

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);
        Subscribe();
        resources = GameMode.StartingPlayerResources;
    }

    [Server]
    public override void OnStopServer()
    {
        // HACK: maybe event should fire after unsub()
        // HACK: not certain this works
        if (GameNetworkManager.IsGameInProgress)
            ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Disconnect);
        Unsubscribe();
    }

    [Command]
    protected void CmdEndTurn()
    {
        HasEndedTurn = true;
        GameManager.Singleton.ServerTryEndTurn();
    }

    [Command]
    protected void CmdCancelEndTurn()
    {
        HasEndedTurn = false;
    }

    [Command]
    protected void CmdSetAction(UnitData data)
    {
        if (GameManager.IsEconomyPhase || GameManager.IsPlayingTurn) return;
        if (!CanMove()) return;

        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        // TODO: verify that a player can't send the cell theyre currently on
        if (data.MyUnit.Movement.ServerSetAction(data)) MoveCount--;
    }

    [Command]
    protected void CmdClearAction(UnitData data)
    {
        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        if (data.MyUnit.Movement.ServerClearAction()) MoveCount++;
    }

    /// <summary>
    /// HACK: is unitId Validation needed?
    /// HACK: should this return bool?
    /// </summary>
    /// <param name="unitId"></param>
    /// <param name="cell"></param>
    [Command] 
    protected void CmdTryBuyUnit(int unitId, HexCell cell)
    {
        if (!GameManager.IsEconomyPhase) return;
        if (cell.MyUnit) return;

        //if (0 <= unitId && unitId < Unit.Prefabs.Count)
        Unit unit = Unit.Prefabs[unitId];
        if (Resources < unit.Resources) return;

        bool canBuy = false;
        for (int i = 0; !canBuy && i < MyForts.Count; i++)
        {
            Fort fort = MyForts[i];
            canBuy = fort.IsBuyCell(cell);
        }

        if (!canBuy) return;

        Unit instance = Instantiate(unit);
        instance.MyCell = cell;
        instance.MyTeam.SetTeam(MyTeam);
        instance.Movement.Orientation = UnityEngine.Random.Range(0, 360f);

        NetworkServer.Spawn(instance.gameObject, connectionToClient);

        Resources -= unit.Resources;
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [Client]
    public override void OnStartClient()
    {
        if (!isClientOnly) return;

        DontDestroyOnLoad(gameObject);

        GameManager.Players.Add(this);

        if (!hasAuthority) return;

        Subscribe();
    }

    [Client]
    public override void OnStopClient()
    {
        if (!isClientOnly) return;

        GameManager.Players.Remove(this); // host will try and remove twice

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

    [TargetRpc]
    public void TargetSetHasEndedTurn(bool status)
    {
        if (!isServer) HasEndedTurn = status;
        ClientOnHasEndedTurn?.Invoke(); 
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public bool CanMove()
    {
        return (MoveCount > 0);
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
        Debug.LogWarning("Subscribing Player as a Server");

        UnitDeath.ServerOnUnitDeath += HandleServerOnUnitDeath;

        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;

        GameManager.ServerOnStartRound += HandleServerOnStartRound;
        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
        GameManager.ServerOnPlayTurn += HandleServerOnPlayTurn;
    }

    protected virtual void Unsubscribe()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        Debug.LogError($"Unsubbing Player {name}");
        if (!isServer) return;
        Debug.LogWarning("Unsubscribing Player as a Server");

        UnitDeath.ServerOnUnitDeath -= HandleServerOnUnitDeath;

        Fort.ServerOnFortCaptured -= HandleServerOnFortCaptured;

        GameManager.ServerOnStartRound -= HandleServerOnStartRound;
        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;
        GameManager.ServerOnPlayTurn -= HandleServerOnPlayTurn;
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
    private void HandleServerOnUnitDeath(Unit unit)
    {
        MyUnits.Remove(unit);
        if (connectionToClient != null) HandleTargetOnUnitDeath(unit.netIdentity);

        if (MyUnits.Count == 0) ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Routed);
    }

    [TargetRpc] // HACK: this could be UnitData? ...but i mean it is coming from the server so idk
    private void HandleTargetOnUnitDeath(NetworkIdentity unitNetId)
    {
        if (isServer) return;

        MyUnits.Remove(unitNetId.GetComponent<Unit>());
    }

    [Server]
    protected virtual void HandleServerOnStartRound()
    {
        MoveCount = 0;
        HasEndedTurn = false;
    }

    [Server]
    protected virtual void HandleServerOnStartTurn()
    {
        MoveCount = GameMode.MovesPerTurn;
        HasEndedTurn = false;
    }

    [Server]
    protected virtual void HandleServerOnPlayTurn()
    {
        HasEndedTurn = true;
    }

    [Client]
    protected virtual void HookOnResources(int oldValue, int newValue) { }

    [Client]
    protected virtual void HookOnMoveCount(int oldValue, int newValue) { }

    #endregion
}