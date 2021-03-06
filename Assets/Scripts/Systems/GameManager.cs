/**
 * File Name: GameManager.cs
 * Description: Manages scene loading and persistent data
 * 
 * Authors: Will Lacey
 * Date Created: October 22, 2020
 * 
 * Additional Comments: 
 * 
 *      HACK: this maybe could be a Monobehaviour
 *      HACK: functions can be updated to static methods
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// 
/// </summary>
public class GameManager : NetworkBehaviour
{
    /************************************************************/
    #region Properties

    public static GameManager Singleton { get; private set; }

    public static bool IsGameInProgress { get; private set; } = false;

    public static int RoundCount { get; private set; }

    public static int TurnCount { get; private set; }

    public static bool IsEconomyPhase { get; private set; } = false;

    public static bool IsPlayingTurn { get; private set; } = false;

    public static float TurnTimer { get; private set; }

    public static List<Player> Players { get; set; } = new List<Player>();

    #endregion
    /************************************************************/
    #region Non-Networked

    #region Unity Functions

    private void Awake()
    {
        enabled = false;
        Singleton = this;

        IsGameInProgress = false;
        RoundCount = 0;
        TurnCount = 0;
        IsPlayingTurn = false;
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    [ServerCallback]
    private void LateUpdate()
    {
        // wait for timer to end or for players to end their turn
        if (Time.time <= TurnTimer) return;

        //enabled = false;
        if (isServer) Server_EndTurn();
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

    #endregion

    #region Timer Functions

    private void ResetTimer()
    {
        TurnTimer = Time.time + GameSession.TurnTimerLength;
        if (isServer) enabled = true;
    }

    #endregion

    #endregion
    /************************************************************/
    #region Server

    #region Server Game Events

    /// <summary>
    /// Server event for when a new round has begun
    /// </summary>
    /// <subscriber class="Piece">refreshes piece's movement</subscriber>
    public static event Action Server_OnStartRound;

    /// <summary>
    /// Server event for when a new turn has begun
    /// </summary>
    /// <subscriber class="Player">refreshes a player's turn/move status</subscriber>
    public static event Action Server_OnStartTurn;

    /// <summary>
    /// Server event for turn is playing
    /// </summary>
    /// <subscriber class="Player">refreshes a player's turn status</subscriber>
    public static event Action Server_OnPlayTurn;

    /// <summary>
    /// Server event for turn has stopped playing
    /// </summary>
    /// <subscriber class="Fort">checks to see if team has updated</subscriber>
    /// <subscriber class="PieceMovement">sets a piece's movement to 0 if it has moved</subscriber>
    public static event Action Server_OnStopTurn;

    #endregion

    #region Game Flow Functions

    [Server]
    public void Server_StartGame()
    {
        Debug.LogWarning($"Starting Game with {Players.Count} Players");

        IsGameInProgress = true;
        IsPlayingTurn = false;
        RoundCount = 0;

        foreach (Player player in Players) player.Credits = GameSession.StartingCredits;

        Server_StartRound();
    }

    [Server]
    public void Server_StartRound() 
    {
        if (!IsGameInProgress) return;

        RoundCount++;
        TurnCount = 0;
        IsEconomyPhase = true;

        foreach (Player player in Players)
            player.Credits += player.MyForts.Count * GameSession.CreditsPerFort;

        Server_OnStartRound?.Invoke();
        Rpc_InvokeOnStartRoundEvent();

        // update timer and its text 
        if (GameSession.IsUsingTurnTimer) ResetTimer();  
    }

    [Server]
    private void Server_StartTurn()
    {
        if (!IsGameInProgress) return;

        TurnCount++;

        Server_OnStartTurn?.Invoke();
        Rpc_InvokeOnStartTurnEvent();

        // update timer and its text
        if (GameSession.IsUsingTurnTimer) ResetTimer();
    }

    [Server]
    public void Server_PlayTurn()
    {
        StopAllCoroutines();
        StartCoroutine(Server_PlayTurn(8));
    }

    [Server] // HACK:  pieces are looped over several times
    private IEnumerator Server_PlayTurn(int numberOfTurnSteps)
    {
        IsPlayingTurn = true;
        Server_OnPlayTurn?.Invoke();
        Rpc_InvokeOnPlayTurnEvent();

        // How many Moves/Steps pieces can Utilize
        for (int step = 0; step < numberOfTurnSteps; step++)
        {
            Server_StartTurnStep();

            yield return Server_WaitForPieces();

            Server_CompleteTurnStep();
        }

        Server_OnStopTurn?.Invoke();
        Rpc_InvokeOnStopTurnEvent();

        IsPlayingTurn = false;

        Server_CheckIfPlayerLost();

        // Finished Turn, either start new one or start a new round
        if (TurnCount >= GameSession.TurnsPerRound) Server_StartRound();
        else Server_StartTurn();
    }

    [Server]
    public void Server_EndTurn()
    {
        enabled = false;
        if (IsEconomyPhase)
        {
            Server_CheckIfPlayerLost();

            IsEconomyPhase = false;
            Rpc_InvokeOnStopEconomyPhaseEvent();
            Server_StartTurn();
        }
        else
        {
            Server_PlayTurn();
        }
    }

    [Server]
    public static void Server_StopGame()
    {
        if (!IsGameInProgress) return;

        Players.Clear();
        IsGameInProgress = false;
    }

    #endregion

    #region Other Game Functions

    [Server]
    public void Server_TryEndTurn()
    {
        bool playTurn = true;
        foreach (Player player in Players) playTurn &= player.HasEndedTurn;

        if (playTurn && !IsPlayingTurn) Server_EndTurn();
    }

    [Server]
    private void Server_StartTurnStep()
    {
        // Moving pieces
        for (int i = HexGrid.Pieces.Count - 1; i >= 0; i--)
        {
            Piece piece = HexGrid.Pieces[i];
            if (piece.IsDying || piece.WillDie)
            {
                if (piece.WillDie) piece.Die();
                HexGrid.Pieces.Remove(piece);
                piece.CollisionHandler.gameObject.SetActive(false);
            }
            else
            {
                piece.Movement.Server_DoStep(); // TODO: correct number of steps
            }
        }
    }

    [Server]
    private IEnumerator Server_WaitForPieces()
    {
        // Waiting for piece
        for (int i = 0; i < HexGrid.Pieces.Count; i++)
        {
            Piece piece = HexGrid.Pieces[i];
            if (piece.Movement.IsEnRoute)
            {
                i = -1;
                yield return null;
            }
        }
    }

    [Server]
    private void Server_CompleteTurnStep()
    {
        // Setting new cell for pieces now that they moved
        foreach (Piece piece in HexGrid.Pieces)  piece.Movement.Server_CompleteTurnStep(); 
    }

    [Server]
    public void Server_CheckIfPlayerLost()
    {
        for (int i = Players.Count; i > 0; i--)
        {
            if (!IsGameInProgress) return;

            Player player = Players[i - 1];

            Debug.Log($"{player.name} has {player.MyForts.Count} forts");
            if (player.MyForts.Count == 0)
                GameOverHandler.Singleton.ServerPlayerHasLost(player, WinConditionType.Conquest);
            else if (player.MyPieces.Count == 0)
                GameOverHandler.Singleton.ServerPlayerHasLost(player, WinConditionType.Routed);
        }
    }

    #endregion

    #endregion
    /************************************************************/
    #region Client

    #region Client Game Events

    /// <summary>
    /// Client event for when a new round has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    /// <subscriber class="HumanPlayer">updates player's buy cells and piece displays</subscriber>
    /// <subscriber class="MapCamera">refreshes 'Next' index</subscriber>
    public static event Action Client_OnStartRound;

    /// <summary>
    /// Client event for when the Economy Phase ends
    /// </summary>
    /// <subscriber class="HumanPlayer">updates player's buy cells and piece displays</subscriber>
    public static event Action Client_OnStopEconomyPhase;

    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    /// <subscriber class="MapCamera">refreshes 'Next' index</subscriber>
    public static event Action Client_OnStartTurn;

    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="HumanPlayer">disables controls when pieces are moving</subscriber>
    public static event Action Client_OnPlayTurn;

    /// <summary>
    /// Client event for turn has stopped playing
    /// </summary>
    /// <subscriber class="HumanPlayer">enables controls when pieces are moving</subscriber>
    public static event Action Client_OnStopTurn;

    #endregion

    #region RPC Game Event Function Calls

    [ClientRpc]
    private void Rpc_InvokeOnStartRoundEvent()
    {
        if (LoadingDisplay.Singleton) LoadingDisplay.Done();

        Debug.Log("Rpc_InvokeOnStartRoundEvent");
        if (isClientOnly)
        {
            if (GameSession.IsUsingTurnTimer) ResetTimer();

            IsEconomyPhase = true;
            RoundCount++;
            TurnCount = 0;

            // TODO: check to see if connection is still active, or if this line is needed
            //NetworkClient.connection.identity.GetComponent<Player>().Resources += 100;
        }

        Client_OnStartRound?.Invoke();
    }

    [ClientRpc]
    private void Rpc_InvokeOnStopEconomyPhaseEvent()
    {
        if (isClientOnly) IsEconomyPhase = false;
        Client_OnStopEconomyPhase?.Invoke();
    }

    [ClientRpc]
    private void Rpc_InvokeOnStartTurnEvent()
    {
        Debug.Log("Rpc_InvokeOnStartTurnEvent");
        if (isClientOnly)
        {
            if (GameSession.IsUsingTurnTimer) ResetTimer();
            TurnCount++;
        }
        Client_OnStartTurn?.Invoke();
    }

    [ClientRpc]
    private void Rpc_InvokeOnPlayTurnEvent()
    {
        //Debug.Log("Rpc_InvokOnPlayTurnEvent");
        Client_OnPlayTurn?.Invoke();
    }

    [ClientRpc]
    private void Rpc_InvokeOnStopTurnEvent()
    {
        //Debug.Log("Rpc_InvokeOnStopTurnEvent");
        Client_OnStopTurn?.Invoke();
    }

    #endregion

    #endregion
}
