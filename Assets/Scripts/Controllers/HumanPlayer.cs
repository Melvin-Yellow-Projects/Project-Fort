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
    /********** MARK: Variables **********/
    #region Variables

    HexCell currentCell;

    Unit selectedUnit;

    bool hasCurrentCellUpdated = false;

    Controls controls;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    protected void Start()
    {
        PlayerMenu.Singleton.MyPlayer = this; // TODO: this needs to occur on client
    }

    protected void Update()
    {
        // HACK: is this still needed?

        if (!EventSystem.current.IsPointerOverGameObject()) // verify pointer is not on top of GUI
        {
            UpdateCurrentCell();
            if (selectedUnit) DoPathfinding();
        }
    }

    #endregion

    /********** MARK: Input Functions **********/
    #region Input Functions

    private void DoSelection(InputAction.CallbackContext context)
    {
        if (currentCell)
        {
            DeselectUnitAndClearItsPath();

            SelectUnit(currentCell.MyUnit);
        }
    }

    private void DoCommand(InputAction.CallbackContext context)
    {
        if (MoveCount >= GameMode.Singleton.MovesPerTurn) return;

        if (currentCell && selectedUnit && selectedUnit.Movement.HasAction)
        {
            DeselectUnit();
            MoveCount++;
            PlayerMenu.RefreshMoveCountText();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void UpdateCurrentCell()
    {
        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();
        if (cell != currentCell)
        {
            if (currentCell) currentCell.DisableHighlight();
            if (cell && cell.IsExplored) cell.EnableHighlight(new Color(1f, 0f, 0f, 0.6f));

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
        if (!myUnits.Contains(unit)) return;
        //if (!unit) return; // THIS LINE IS FOR DEBUG PURPOSES (allows you to control enemies)

        if (!unit.Movement.CanMove) return;

        if (unit.Movement.HasAction)
        {
            unit.Movement.Path.Clear();
            MoveCount--;
            PlayerMenu.RefreshMoveCountText();
        }
        if (MoveCount >= GameMode.Singleton.MovesPerTurn) return;

        selectedUnit = unit;
        selectedUnit.IsSelected = true;
    }

    private void DeselectUnit()
    {
        if (selectedUnit)
        {
            selectedUnit.IsSelected = false;
            selectedUnit = null;
        }
    }

    private void DeselectUnitAndClearItsPath()
    {
        if (selectedUnit) selectedUnit.Movement.Path.Clear();
        DeselectUnit();
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    protected override void Subscribe()
    {
        base.Subscribe();

        GameManager.OnPlayTurn += HandleOnPlayTurn;
        GameManager.OnStopTurn += HandleOnStopTurn;

        controls = new Controls();
        controls.Player.Selection.performed += DoSelection;
        controls.Player.Command.performed += DoCommand;
        controls.Enable();
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        GameManager.OnPlayTurn -= HandleOnPlayTurn;
        GameManager.OnStopTurn -= HandleOnStopTurn;

        controls.Dispose();
    }

    protected override void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.MyTeam != MyTeam) return;

        myUnits.Add(unit);
        unit.Movement.Display.ToggleMovementDisplay();
    }

    protected override void HandleOnStartTurn()
    {
        base.HandleOnStartTurn();
        PlayerMenu.RefreshMoveCountText();
    }

    private void HandleOnPlayTurn()
    {
        DeselectUnitAndClearItsPath();
        controls.Disable();
    }

    private void HandleOnStopTurn()
    {
        controls.Enable();
    }

    #endregion
}