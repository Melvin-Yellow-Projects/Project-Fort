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

        UpdateCurrentCell();
        if (selectedPiece) DoPathfinding();
    }

    protected override void OnDestroy()
    {
        if (currentCell) currentCell.DisableHighlight();
        base.OnDestroy();
    }

    #endregion
    /************************************************************/
    #region Client Input Functions

    [Client]
    private void Command(InputAction.CallbackContext context)
    {
        if (!currentCell) return;

        if (GameManager.IsEconomyPhase)
        {
            CmdTryBuyPiece(PlayerDisplay.PieceId, currentCell);
        }
        else
        {
            if (selectedPiece)
            {
                CmdSetAction(PieceData.Instantiate(selectedPiece));
                PlayerDisplay.RefreshMoveCountText();
                DeselectPiece();
            }
            else
            {
                //DeselectPieceAndClearItsPath();
                SelectPiece(currentCell.MyPiece);
            }
        }
    }

    [Client]
    private void Cancel(InputAction.CallbackContext context)
    {
        if (GameManager.IsEconomyPhase && currentCell)
        {
            CmdTrySellPiece(currentCell);
        }
        else
        {
            DeselectPieceAndClearItsPath();
        }
    }

    [Client]
    private void Toggle(InputAction.CallbackContext context)
    {
        isShowingPiecePaths = !isShowingPiecePaths;

        if (isShowingPiecePaths) foreach (Piece piece in MyPieces) piece.Movement.Path.Show();
        else foreach (Piece piece in MyPieces) piece.Movement.Path.Hide();
    }

    [Client]
    private void Clear(InputAction.CallbackContext context)
    {
        foreach (Piece piece in MyPieces) CmdClearAction(PieceData.Instantiate(piece));
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartLocalPlayer()
    {
        AuthoritySubscribe();
    }

    [Client]
    private void UpdateCurrentCell()
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
    private void DoPathfinding()
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
    private void SelectPiece(Piece piece)
    {
        if (!MyPieces.Contains(piece)) return;
        //if (!piece) return; // THIS LINE IS FOR DEBUG PURPOSES (allows you to control enemies)

        if (!piece.Movement.CanMove) return;

        if (piece) CmdClearAction(PieceData.Instantiate(piece));

        if (!CanMove()) return;

        selectedPiece = piece;
        selectedPiece.IsSelected = true;
    }

    [Client]
    private void DeselectPiece()
    {
        if (!selectedPiece) return;

        if (!isShowingPiecePaths) selectedPiece.Movement.Path.Hide();

        selectedPiece.IsSelected = false;
        selectedPiece = null;
    }

    [Client]
    private void DeselectPieceAndClearItsPath()
    {
        if (!selectedPiece) return;

        selectedPiece.Movement.Path.Clear();

        DeselectPiece();

        //Debug.Log("There is a piece to DeselectPieceAndClearItsPath");
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    protected override void AuthoritySubscribe()
    {
        GameManager.Client_OnStartRound += HandleClientOnStartRound;
        GameManager.Client_OnStopEconomyPhase += HandleClientOnStopEconomyPhase;
        GameManager.Client_OnPlayTurn += HandleClientOnPlayTurn;
        GameManager.Client_OnStopTurn += HandleClientOnStopTurn;

        controls = new Controls();
        controls.Player.Command.performed += Command;
        controls.Player.Cancel.performed += Cancel;
        controls.Player.Toggle.performed += Toggle;
        controls.Player.Clear.performed += Clear;
        controls.Enable();

        base.AuthoritySubscribe();
    }

    protected override void AuthorityUnsubscribe()
    {
        GameManager.Client_OnStartRound -= HandleClientOnStartRound;
        GameManager.Client_OnStopEconomyPhase -= HandleClientOnStopEconomyPhase;
        GameManager.Client_OnPlayTurn -= HandleClientOnPlayTurn;
        GameManager.Client_OnStopTurn -= HandleClientOnStopTurn;

        controls.Dispose();

        base.AuthorityUnsubscribe();
    }

    [Client]
    private void HandleClientOnStartRound()
    {
        foreach (Fort fort in MyForts) fort.ShowBuyCells();
        foreach (Piece piece in MyPieces) piece.Movement.Display.HideDisplay();
    }

    [Client]
    private void HandleClientOnStopEconomyPhase()
    {
        foreach (Fort fort in MyForts) fort.HideBuyCells();
        foreach (Piece piece in MyPieces) piece.Movement.Display.ShowDisplay();
    }

    [Client]
    private void HandleClientOnPlayTurn()
    {
        // HACK: i dont think you need to clear it's path, the path shouldn't be set
        DeselectPieceAndClearItsPath(); 
        controls.Disable();
    }

    [Client]
    private void HandleClientOnStopTurn()
    {
        controls.Enable();
    }

    [Client]
    protected override void HookOnCredits(int oldValue, int newValue)
    {
        if (PlayerDisplay.MyPlayer) PlayerDisplay.RefreshCreditsText();
    }

    [Client]
    protected override void HookOnMoveCount(int oldValue, int newValue)
    {
        PlayerDisplay.RefreshMoveCountText();
    }

    #endregion
}