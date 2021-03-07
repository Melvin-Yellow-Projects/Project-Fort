/**
 * File Name: HumanPlayer.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 20, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using Mirror;

public class HumanPlayer : Player
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] MapCamera mapCamera = null;

    public HexCell currentCell; // HACK this shouldnt be public

    Piece selectedPiece;

    bool hasCurrentCellUpdated = false;

    Controls controls;

    bool isShowingPiecePaths = true;

    #endregion
    /************************************************************/
    #region Properties

    public bool IsReadyForMapData { get; set; } = false;

    #endregion
    /************************************************************/
    #region Non-Networked

    #region Unity Functions

    protected void OnEnable()
    {
        mapCamera.enabled = true;
        PlayerDisplay.MyPlayer = this;
        PlayerDisplay.RefreshCreditsText();
    }

    protected void LateUpdate() 
    {
        // this is in late update so the player cursor always shows on top of hexes

        // other human players should be disabled 
        //if (!hasAuthority) return; 

        // HACK: is this still needed?
        // verify pointer is not on top of GUI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Client_UpdateCurrentCell();
        if (selectedPiece) Client_DoPathfinding();
    }

    protected override void OnDestroy()
    {
        if (currentCell) currentCell.DisableHighlight();
        base.OnDestroy();
    }

    #endregion

    #endregion
    /************************************************************/
    #region Client

    #region SyncVars

    [Client]
    protected override void SyncVar_credits(int oldValue, int newValue)
    {
        if (PlayerDisplay.MyPlayer) PlayerDisplay.RefreshCreditsText();
    }

    [Client]
    protected override void SyncVar_moveCount(int oldValue, int newValue)
    {
        PlayerDisplay.RefreshMoveCountText();
    }

    #endregion

    #region Mirror Functions

    public override void OnStartLocalPlayer()
    {
        Client_AuthoritySubscribe();
    }

    #endregion

    #region Client Input Functions

    [Client]
    private void Client_Command(InputAction.CallbackContext context)
    {
        if (!currentCell) return;

        if (GameManager.IsEconomyPhase)
        {
            Cmd_TryBuyPiece(PlayerDisplay.PieceId, currentCell);
        }
        else
        {
            if (selectedPiece)
            {
                Cmd_SetAction(PieceData.Instantiate(selectedPiece));
                PlayerDisplay.RefreshMoveCountText();
                Client_DeselectPiece();
            }
            else
            {
                //DeselectPieceAndClearItsPath();
                Client_SelectPiece(currentCell.MyPiece);
            }
        }
    }

    [Client]
    private void Client_Cancel(InputAction.CallbackContext context)
    {
        if (GameManager.IsEconomyPhase && currentCell)
        {
            Cmd_TrySellPiece(currentCell);
        }
        else
        {
            Client_DeselectPieceAndClearItsPath();
        }
    }

    [Client]
    private void Client_Toggle(InputAction.CallbackContext context)
    {
        isShowingPiecePaths = !isShowingPiecePaths;

        if (isShowingPiecePaths) foreach (Piece piece in MyPieces) piece.Movement.Path.Show();
        else foreach (Piece piece in MyPieces) piece.Movement.Path.Hide();
    }

    [Client]
    private void Client_Clear(InputAction.CallbackContext context)
    {
        foreach (Piece piece in MyPieces) Cmd_ClearAction(PieceData.Instantiate(piece));
    }

    #endregion

    #region Client Functions

    [Client]
    private void Client_UpdateCurrentCell()
    {
        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();

        // HACK: this color is hardcoded, it should probably reflect the team color?
        //if (cell && cell.IsExplored) cell.EnableHighlight(new Color(1f, 0f, 0f, 0.6f));
        if (cell && cell.IsExplored)
            cell.EnableHighlight(MyTeam.TeamColor * new Vector4(0.8f, 0.8f, 0.8f, 1f));

        if (cell != currentCell)
        {
            if (currentCell)
            {
                currentCell.DisableHighlight();
                if (!isShowingPiecePaths && currentCell.MyPiece)
                    currentCell.MyPiece.Movement.Path.Hide();
            }

            currentCell = cell;
            if (currentCell && currentCell.MyPiece && currentCell.MyPiece.MyTeam == MyTeam)
                currentCell.MyPiece.Movement.Path.Show();

            hasCurrentCellUpdated = true; // whether or not current cell has updated
        }
        else
        {
            hasCurrentCellUpdated = false;
        }
    }

    [Client]
    private void Client_DoPathfinding()
    {
        if (!hasCurrentCellUpdated || currentCell == null) return;

        if (Input.GetKey("left shift")) // HACK: this is a hardcoded input
        {
            selectedPiece.Movement.Path.AddCellToPath(currentCell, canBackTrack: true);
        }
        else
        {
            selectedPiece.Movement.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedPiece.Movement.Path.Show();
    }

    [Client]
    private void Client_SelectPiece(Piece piece)
    {
        if (!MyPieces.Contains(piece)) return;
        //if (!piece) return; // THIS LINE IS FOR DEBUG PURPOSES (allows you to control enemies)

        if (!piece.Movement.CanMove) return;

        if (piece) Cmd_ClearAction(PieceData.Instantiate(piece));

        if (!CanMove()) return;

        selectedPiece = piece;
        selectedPiece.IsSelected = true;
    }

    [Client]
    private void Client_DeselectPiece()
    {
        if (!selectedPiece) return;

        if (!isShowingPiecePaths) selectedPiece.Movement.Path.Hide();

        selectedPiece.IsSelected = false;
        selectedPiece = null;
    }

    [Client]
    private void Client_DeselectPieceAndClearItsPath()
    {
        if (!selectedPiece) return;

        selectedPiece.Movement.Path.Clear();

        Client_DeselectPiece();

        //Debug.Log("There is a piece to DeselectPieceAndClearItsPath");
    }

    #endregion

    #region Event Handler Functions

    [Client]
    protected override void Client_AuthoritySubscribe()
    {
        GameManager.Client_OnStartRound += Client_HandleOnStartRound;
        GameManager.Client_OnStopEconomyPhase += Client_HandleOnStopEconomyPhase;
        GameManager.Client_OnStartTurn += Client_HandleOnStartTurn;
        GameManager.Client_OnPlayTurn += Client_HandleOnPlayTurn;
        GameManager.Client_OnStopTurn += Client_HandleOnStopTurn;

        controls = new Controls();
        controls.Player.Command.performed += Client_Command;
        controls.Player.Cancel.performed += Client_Cancel;
        controls.Player.Toggle.performed += Client_Toggle;
        controls.Player.Clear.performed += Client_Clear;
        controls.Enable();

        base.Client_AuthoritySubscribe();
    }

    // HACK this is not server protected because otherwise it does not get called
    protected override void Client_AuthorityUnsubscribe()
    {
        GameManager.Client_OnStartRound -= Client_HandleOnStartRound;
        GameManager.Client_OnStopEconomyPhase -= Client_HandleOnStopEconomyPhase;
        GameManager.Client_OnStartTurn -= Client_HandleOnStartTurn;
        GameManager.Client_OnPlayTurn -= Client_HandleOnPlayTurn;
        GameManager.Client_OnStopTurn -= Client_HandleOnStopTurn;

        controls.Dispose();

        base.Client_AuthorityUnsubscribe();
    }

    [Client]
    private void Client_HandleOnStartRound()
    {
        hasEndedTurn = false;
        foreach (Fort fort in MyForts) fort.ShowBuyCells();
        foreach (Piece piece in MyPieces) piece.Movement.Display.HideDisplay();
    }

    [Client]
    private void Client_HandleOnStopEconomyPhase()
    {
        foreach (Fort fort in MyForts) fort.HideBuyCells();
        foreach (Piece piece in MyPieces) piece.Movement.Display.ShowDisplay();
    }

    [Client]
    private void Client_HandleOnStartTurn()
    {
        hasEndedTurn = false;
    }

    [Client]
    private void Client_HandleOnPlayTurn()
    {
        // HACK: i dont think you need to clear it's path, the path shouldn't be set
        Client_DeselectPieceAndClearItsPath(); 
        controls.Disable();
    }

    [Client]
    private void Client_HandleOnStopTurn()
    {
        controls.Enable();
    }

    #endregion

    #endregion
}