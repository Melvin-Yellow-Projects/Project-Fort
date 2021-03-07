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

    [SyncVar(hook = nameof(SyncVar_moveCount))]
    int moveCount = 0; // HACK: wrong, other clients shouldnt know this data.

    [SyncVar(hook = nameof(SyncVar_credits))]
    int credits = 0; // HACK: wrong, other clients shouldnt know this data, or should they? 

    // TODO: this should probably be a syncvar so clients know who still is taking their turn
    protected bool hasEndedTurn = false;

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

            if (isClientOnly)
            {
                Cmd_HasEndedTurn(value);
            }
            else
            {
                if (hasEndedTurn) GameManager.Singleton.Server_TryEndTurn();
            }
        }
    }

    public List<Piece> MyPieces { get; set; } = new List<Piece>(); 

    public List<Fort> MyForts { get; set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Non-Networked

    #region Unity Functions

    protected virtual void Start()
    {
        // HACK: this should just show the defeat, not the player has won screen
        //if (isServer && MyPieces.Count == 0 && MyForts.Count == 0)
        //    ServerOnPlayerDefeat?.Invoke(this, WinConditionType.Disconnect);
    }

    protected virtual void OnDestroy()
    {
        Server_Unsubscribe(); // HACK brute force
    }

    #endregion

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

    #endregion

    #region Event Handler Functions

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

    private void HandleOnPieceSpawned(Piece piece)
    {
        if (piece.MyTeam != MyTeam) return;

        MyPieces.Add(piece);
    }

    #endregion

    #endregion
    /************************************************************/
    #region Server

    #region Mirror Functions

    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);
        Server_Subscribe();
    }

    public override void OnStopServer()
    {
        // HACK: maybe event should fire after unsub()
        // HACK: not certain this works
        //if (GameManager.IsGameInProgress)
        //    GameOverHandler.Singleton.ServerPlayerHasLost(this, WinConditionType.Disconnect);
        Server_Unsubscribe();

        Debug.Log($"{name} OnStopServer");
    }

    #endregion

    #region Server Functions

    [Server]
    protected void Server_SetAction(Player player, PieceData data)
    {
        if (GameManager.IsEconomyPhase || GameManager.IsPlayingTurn) return;
        if (!CanMove()) return;

        if (player.MyTeam != data.MyPiece.MyTeam) return;

        // TODO: verify that a player can't send the cell theyre currently on
        if (data.MyPiece.Movement.Server_SetMove(data)) MoveCount--;
    }

    [Server]
    protected void Server_TryBuyPiece(int pieceId, HexCell cell)
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

    #endregion

    #region Server Commands

    [Command]
    private void Cmd_HasEndedTurn(bool hasEndedTurn)
    {
        HasEndedTurn = hasEndedTurn;
    }

    [Command]
    protected void Cmd_SetAction(PieceData data)
    {
        //if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        Server_SetAction(connectionToClient.identity.GetComponent<Player>(), data);
    }
    
    [Command]
    protected void Cmd_ClearAction(PieceData data)
    {
        if (!data.DoesConnectionHaveAuthority(connectionToClient)) return;

        if (data.MyPiece.Movement.Server_ClearMove()) MoveCount++;
    }

    /// <summary>
    /// HACK: is pieceId Validation needed?
    /// HACK: should this return bool?
    /// </summary>
    /// <param name="pieceId"></param>
    /// <param name="cell"></param>
    [Command] 
    protected void Cmd_TryBuyPiece(int pieceId, HexCell cell)
    {
        Server_TryBuyPiece(pieceId, cell);
    }

    [Command]
    protected void Cmd_TrySellPiece(HexCell cell)
    {
        if (!GameManager.IsEconomyPhase) return;

        if (MyPieces.Count == 1) return;

        if (!cell.MyPiece || cell.MyPiece.MyTeam != MyTeam) return;

        if (!CanBuyOnCell(cell)) return;

        Credits += cell.MyPiece.Credits;

        cell.MyPiece.Die();
    }

    #endregion

    #region Event Handler Functions

    // HACK this line does not work, subscribe needs to happen on server and authoritive client
    [Server]
    protected virtual void Server_Subscribe()
    {
        Debug.LogWarning($"ServerSubscribe on {name}");
        Piece.OnPieceSpawned += HandleOnPieceSpawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        PieceDeath.Server_OnPieceDeath += Server_HandleOnPieceDeath;

        Fort.Server_OnFortCaptured += Server_HandleOnFortCaptured;

        GameManager.Server_OnStartRound += Server_HandleOnStartRound;
        GameManager.Server_OnStartTurn += Server_HandleOnStartTurn;
        GameManager.Server_OnPlayTurn += Server_HandleOnPlayTurn;
    }

    [Server]
    protected virtual void Server_Unsubscribe()
    {
        Debug.LogWarning($"ServerUnsubscribe on {name}");
        Piece.OnPieceSpawned -= HandleOnPieceSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        PieceDeath.Server_OnPieceDeath -= Server_HandleOnPieceDeath;

        Fort.Server_OnFortCaptured -= Server_HandleOnFortCaptured;

        GameManager.Server_OnStartRound -= Server_HandleOnStartRound;
        GameManager.Server_OnStartTurn -= Server_HandleOnStartTurn;
        GameManager.Server_OnPlayTurn -= Server_HandleOnPlayTurn;
    }

    [Server] // HACK: this function is so jank
    private void Server_HandleOnFortCaptured(Fort fort, int previousTeamId)
    {
        if (MyTeam == fort.MyTeam)
        {
            Debug.Log($"{name} is adding {fort.name} they just gained");

            MyForts.Add(fort);
            if (connectionToClient != null) Target_AddFort(connectionToClient, fort.netIdentity);
        }
        else if (MyTeam.Id == previousTeamId)
        {
            Debug.LogWarning($"{name} is removing {fort.name} they just lost");

            MyForts.Remove(fort);
            if (connectionToClient != null) Target_RemoveFort(connectionToClient, fort.netIdentity);
        }
    }

    [Server]
    private void Server_HandleOnPieceDeath(Piece piece)
    {
        MyPieces.Remove(piece);
        if (connectionToClient != null) Target_HandleOnPieceDeath(piece.netIdentity);
    }

    [Server]
    protected virtual void Server_HandleOnStartRound()
    {
        MoveCount = 0;
        HasEndedTurn = false;
    }

    [Server]
    protected virtual void Server_HandleOnStartTurn()
    {

        MoveCount = GameSession.MovesPerTurn;
        HasEndedTurn = false;
    }

    [Server]
    protected virtual void Server_HandleOnPlayTurn()
    {
        HasEndedTurn = true;
    }

    #endregion

    #endregion
    /************************************************************/
    #region Client

    #region SyncVars

    [Client]
    protected virtual void SyncVar_credits(int oldValue, int newValue) { }

    [Client]
    protected virtual void SyncVar_moveCount(int oldValue, int newValue) { }

    #endregion

    #region Mirror Functions

    public override void OnStartClient()
    {
        if (isServer) return;
        DontDestroyOnLoad(gameObject);

        GameManager.Players.Add(this);
    }

    public override void OnStopClient()
    {
        if (hasAuthority) Client_AuthorityUnsubscribe();

        GameManager.Players.Remove(this); // HACK host will try and remove twice; idk how to stop it
    }

    #endregion

    #region Client Functions

    // HACK: can probably replace with psuedo Handler function since it's specifically called by a
    // handler
    [TargetRpc] 
    public void Target_AddFort(NetworkConnection target, NetworkIdentity fortNetId)
    {
        if (isServer) return;
        MyForts.Add(fortNetId.GetComponent<Fort>());
    }

    // HACK: can probably replace with psuedo Handler function since it's specifically called by a
    // handler
    [TargetRpc]
    public void Target_RemoveFort(NetworkConnection target, NetworkIdentity fortNetId)
    {
        if (isServer) return;
        MyForts.Remove(fortNetId.GetComponent<Fort>());
    }

    #endregion

    #region Event Handler Functions

    [Client]
    protected virtual void Client_AuthoritySubscribe()
    {
        if (isServer) return;
        Debug.LogWarning($"AuthoritySubscribe on {name}");
        Piece.OnPieceSpawned += HandleOnPieceSpawned;

        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;
    }

    [Client]
    protected virtual void Client_AuthorityUnsubscribe()
    {
        Debug.LogWarning($"AuthorityUnsubscribe on {name}");
        Piece.OnPieceSpawned -= HandleOnPieceSpawned;

        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;
    }

    [TargetRpc] // HACK: this could be PieceData? ...but i mean it is coming from the server so idk
    private void Target_HandleOnPieceDeath(NetworkIdentity pieceNetId)
    {
        if (isServer) return;

        MyPieces.Remove(pieceNetId.GetComponent<Piece>());
    }

    #endregion

    #endregion
}