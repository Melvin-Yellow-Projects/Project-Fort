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

    HexCell currentCell;

    Unit selectedUnit;

    bool hasCurrentCellUpdated = false;

    Controls controls;

    #endregion
    /************************************************************/
    #region Properties

    public bool HasCreatedMap { get; [Server] set; } = false;

    #endregion
    /************************************************************/
    #region Unity Functions

    protected void OnEnable()
    {
        PlayerMenu.MyPlayer = this;
        PlayerMenu.RefreshResourcesText();
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
        if (selectedUnit) DoPathfinding();
    }
    #endregion

    /************************************************************/
    #region Server Functions

    [Command] // HACK: i dont like this function here
    public void CmdStartGame() 
    {
        if (!GetComponent<PlayerInfo>().IsPartyOwner) return;

        if (GameNetworkManager.IsGameInProgress) return;

        GameNetworkManager.Singleton.ServerStartGame();
    }

    #endregion
    /************************************************************/
    #region Input Functions

    private void DoSelection(InputAction.CallbackContext context)
    {
        if (!currentCell) return;

        if (currentCell.MyFort && currentCell.MyFort.hasAuthority) Debug.Log("My Fort!");

        DeselectUnitAndClearItsPath();

        SelectUnit(currentCell.MyUnit);
    }

    private void DoCommand(InputAction.CallbackContext context)
    {
        if (selectedUnit) CmdSetAction(UnitData.Instantiate(selectedUnit));

        PlayerMenu.RefreshMoveCountText();

        DeselectUnit();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void UpdateCurrentCell()
    {
        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();

        // HACK: this color is hardcoded, it should probably reflect the team color?
        if (cell && cell.IsExplored) cell.EnableHighlight(new Color(1f, 0f, 0f, 0.6f));

        if (cell != currentCell)
        {
            if (currentCell) currentCell.DisableHighlight();

            currentCell = cell;
            hasCurrentCellUpdated = true; // whether or not current cell has updated
        }
        else
        {
            hasCurrentCellUpdated = false;
        }
    }

    private void DoPathfinding()
    {
        if (!hasCurrentCellUpdated || currentCell == null) return;

        if (Input.GetKey("left shift")) // HACK: this is a hardcoded input
        {
            selectedUnit.Movement.Path.AddCellToPath(currentCell, canBackTrack: true);
        }
        else
        {
            selectedUnit.Movement.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedUnit.Movement.Path.Show();
    }

    private void SelectUnit(Unit unit)
    {
        if (!MyUnits.Contains(unit)) return;
        //if (!unit) return; // THIS LINE IS FOR DEBUG PURPOSES (allows you to control enemies)

        if (!unit.Movement.CanMove) return;

        if (unit) CmdClearAction(UnitData.Instantiate(unit));

        if (!CanMove()) return;

        selectedUnit = unit;
        selectedUnit.IsSelected = true;
    }

    private void DeselectUnit()
    {
        if (!selectedUnit) return;

        selectedUnit.IsSelected = false;
        selectedUnit = null;
    }

    private void DeselectUnitAndClearItsPath()
    {
        if (!selectedUnit) return;

        selectedUnit.Movement.Path.Clear();

        DeselectUnit();

        //Debug.Log("There is a Unit to DeselectUnitAndClearItsPath");
    }
    #endregion
    /************************************************************/
    #region Event Handler Functions

    protected override void Subscribe()
    {
        base.Subscribe();

        if (!hasAuthority) return;

        PlayerMenu.ClientOnEndTurnButtonPressed += HandleClientOnEndTurnButtonPressed;

        GameManager.ClientOnPlayTurn += HandleClientOnPlayTurn;
        GameManager.ClientOnStopTurn += HandleClientOnStopTurn;

        controls = new Controls();
        controls.Player.Selection.performed += DoSelection;
        controls.Player.Command.performed += DoCommand;
        controls.Enable();
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        if (!hasAuthority) return;

        PlayerMenu.ClientOnEndTurnButtonPressed -= HandleClientOnEndTurnButtonPressed;

        GameManager.ClientOnPlayTurn -= HandleClientOnPlayTurn;
        GameManager.ClientOnStopTurn -= HandleClientOnStopTurn;

        controls.Dispose();
    }

    [Client]
    protected void HandleClientOnStartTurn() // FIXME: this function is not called
    {
        PlayerMenu.RefreshMoveCountText(); 
    }

    [Client]
    private void HandleClientOnPlayTurn()
    {
        // HACK: i dont think you need to clear it's path, the path shouldn't be set
        DeselectUnitAndClearItsPath(); 
        controls.Disable();
    }

    [Client]
    private void HandleClientOnStopTurn()
    {
        controls.Enable();
    }

    [Client]
    private void HandleClientOnEndTurnButtonPressed()
    {
        // HasEndedTurn has not been updated yet, it's a state behind
        if (HasEndedTurn) CmdCancelEndTurn(); 
        else CmdEndTurn();
    }

    [Client]
    protected override void HookOnResources(int oldValue, int newValue)
    {
        if (PlayerMenu.MyPlayer) PlayerMenu.RefreshResourcesText();
    }

    [Client]
    protected override void HookOnMoveCount(int oldValue, int newValue)
    {
        PlayerMenu.RefreshMoveCountText();
    }

    #endregion
}