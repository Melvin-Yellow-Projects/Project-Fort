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

    [SyncVar(hook = nameof(HookOnCredits))]
    int credits = 0; // HACK: wrong, other clients shouldnt know this data, or should they? 

    bool hasEndedTurn = false;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Client event for when a player's resources have updated
    /// </summary>
    //public static event Action ClientOnResourcesUpdated;

    /// <summary>
    /// Client event for when a player has ended their turn
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

    public PlayerInfo Info
    {
        get
        {
            return GetComponent<PlayerInfo>();
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

    public int Credits
    {
        get
        {
            return credits;
        }
        set
        {
            credits = value;
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
            if (isServer && connectionToClient != null)
                //TargetSetHasEndedTurn(connectionToClient, value);
                TargetSetHasEndedTurn(connectionToClient, value);
        }
    }

    public List<Piece> MyPieces { get; set; } = new List<Piece>(); 

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Unity Functions

    protected virtual void Start()
    {
        // HACK: this should just show the defeat, not the player has won screen
        //if (isServer && MyPieces.Count == 0 && MyForts.Count == 0)
        //    ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Disconnect);
    }

    protected virtual void OnDestroy()
    {
        ServerUnsubscribe(); // HACK brute force
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);
        ServerSubscribe();
    }

    public override void OnStopServer()
    {
        // HACK: maybe event should fire after unsub()
        // HACK: not certain this works
        //if (GameManager.IsGameInProgress)
        //    GameOverHandler.Singleton.ServerPlayerHasLost(this, WinConditionType.Disconnect);
        ServerUnsubscribe();

        Debug.Log($"{name} OnStopServer");
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
    protected void CmdSetAction(PieceData data)
    {
        //if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        ServerSetAction(connectionToClient.identity.GetComponent<Player>(), data);
    }

    [Server]
    protected void ServerSetAction(Player player, PieceData data)
    {
        if (GameManager.IsEconomyPhase || GameManager.IsPlayingTurn) return;
        if (!CanMove()) return;

        if (player.MyTeam != data.MyPiece.MyTeam) return;

        // TODO: verify that a player can't send the cell theyre currently on
        if (data.MyPiece.Movement.ServerSetMove(data)) MoveCount--;
    }

    [Command]
    protected void CmdClearAction(PieceData data)
    {
        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        if (data.MyPiece.Movement.ServerClearMove()) MoveCount++;
    }

    /// <summary>
    /// HACK: is pieceId Validation needed?
    /// HACK: should this return bool?
    /// </summary>
    /// <param name="pieceId"></param>
    /// <param name="cell"></param>
    [Command] 
    protected void CmdTryBuyPiece(int pieceId, HexCell cell)
    {
        ServerTryBuyPiece(pieceId, cell);
    }

    [Server]
    protected void ServerTryBuyPiece(int pieceId, HexCell cell)
    {
        if (!GameManager.IsEconomyPhase) return;
        if (cell.MyPiece) return;

        //if (0 <= pieceId && pieceId < Piece.Prefabs.Count)
        Piece piece = Piece.Prefabs[pieceId];
        if (Credits < piece.Credits) return;

        if (!CanBuyOnCell(cell)) return;

        // HACK: create piece instantiation method
        Piece instance = Instantiate(piece);
        instance.MyCell = cell;
        instance.MyTeam.SetTeam(MyTeam);

        NetworkServer.Spawn(instance.gameObject, connectionToClient);

        Credits -= piece.Credits;
    }

    [Command]
    protected void CmdTrySellPiece(HexCell cell)
    {
        if (!GameManager.IsEconomyPhase) return;

        if (MyPieces.Count == 1) return;

        if (!cell.MyPiece || cell.MyPiece.MyTeam != MyTeam) return;

        if (!CanBuyOnCell(cell)) return;

        Credits += cell.MyPiece.Credits;

        cell.MyPiece.Die();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (isServer) return;
        DontDestroyOnLoad(gameObject);

        GameManager.Players.Add(this);
    }

    public override void OnStopClient()
    {
        if (hasAuthority) AuthorityUnsubscribe();

        GameManager.Players.Remove(this); // HACK host will try and remove twice; idk how to stop it
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
    public void TargetSetHasEndedTurn(NetworkConnection target, bool status)
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

    private bool CanBuyOnCell(HexCell cell)
    {
        for (int i = 0; i < MyForts.Count; i++)
        {
            Fort fort = MyForts[i];
            if (fort.IsBuyCell(cell)) return true;
        }

        return false;
    }

    private void EndMyTurn()
    {

    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    // HACK this line does not work, subscribe needs to happen on server and authoritive client
    protected virtual void ServerSubscribe()
    {
        Debug.LogWarning($"ServerSubscribe on {name}");
        Piece.OnPieceSpawned += HandleOnPieceSpawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        PieceDeath.ServerOnPieceDeath += HandleServerOnPieceDeath;

        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;

        GameManager.ServerOnStartRound += HandleServerOnStartRound;
        GameManager.ServerOnStartTurn += HandleServerOnStartTurn;
        GameManager.ServerOnPlayTurn += HandleServerOnPlayTurn;
    }

    protected virtual void ServerUnsubscribe()
    {
        Debug.LogWarning($"ServerUnsubscribe on {name}");
        Piece.OnPieceSpawned -= HandleOnPieceSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        PieceDeath.ServerOnPieceDeath -= HandleServerOnPieceDeath;

        Fort.ServerOnFortCaptured -= HandleServerOnFortCaptured;

        GameManager.ServerOnStartRound -= HandleServerOnStartRound;
        GameManager.ServerOnStartTurn -= HandleServerOnStartTurn;
        GameManager.ServerOnPlayTurn -= HandleServerOnPlayTurn;
    }

    protected virtual void AuthoritySubscribe()
    {
        if (isServer) return;
        Debug.LogWarning($"AuthoritySubscribe on {name}");
        Piece.OnPieceSpawned += HandleOnPieceSpawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;
    }

    protected virtual void AuthorityUnsubscribe()
    {
        Debug.LogWarning($"AuthorityUnsubscribe on {name}");
        Piece.OnPieceSpawned -= HandleOnPieceSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;
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
    private void HandleServerOnFortCaptured(Fort fort, int previousTeamId)
    {
        if (MyTeam == fort.MyTeam)
        {
            Debug.Log($"{name} is adding {fort.name} they just gained");

            MyForts.Add(fort);
            if (connectionToClient != null) TargetAddFort(connectionToClient, fort.netIdentity); 
        }
        else if (MyTeam.Id == previousTeamId)
        {
            Debug.LogWarning($"{name} is removing {fort.name} they just lost");

            MyForts.Remove(fort);
            if (connectionToClient != null) TargetRemoveFort(connectionToClient, fort.netIdentity);
        }
    }

    private void HandleOnPieceSpawned(Piece piece)
    {
        if (piece.MyTeam != MyTeam) return;

        MyPieces.Add(piece);
    }

    [Server]
    private void HandleServerOnPieceDeath(Piece piece)
    {
        MyPieces.Remove(piece);
        if (connectionToClient != null) HandleTargetOnPieceDeath(piece.netIdentity);
    }

    [TargetRpc] // HACK: this could be PieceData? ...but i mean it is coming from the server so idk
    private void HandleTargetOnPieceDeath(NetworkIdentity pieceNetId)
    {
        if (isServer) return;

        MyPieces.Remove(pieceNetId.GetComponent<Piece>());
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

        MoveCount = GameSession.MovesPerTurn;
        HasEndedTurn = false;
    }

    [Server]
    protected virtual void HandleServerOnPlayTurn()
    {
        HasEndedTurn = true;
    }

    [Client]
    protected virtual void HookOnCredits(int oldValue, int newValue) { }

    [Client]
    protected virtual void HookOnMoveCount(int oldValue, int newValue) { }

    #endregion
}