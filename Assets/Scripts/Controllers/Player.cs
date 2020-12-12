/**
 * File Name: HexGameUI.cs
 * Description: TODO: comment script
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// TODO: comment class
/// </summary>
public class Player : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Cached References */

    HexCell currentCell;

    Unit selectedUnit;

    HexDirection selectedDirection;

    bool hasCurrentCellUpdated = false;

    List<Unit> myUnits = new List<Unit>();

    Controls controls;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public int Team { get; set; } = 0;

    public int MoveCount { get; set; } = 1;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a command has been invoked or revoked
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    public static event Action OnCommandChange;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        //terrainMaterial.DisableKeyword("GRID_ON");
        Shader.DisableKeyword("HEX_MAP_EDIT_MODE");

        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;

        GameManager.OnStartTurn += HandleOnStartTurn;
        GameManager.OnStartMoveUnits += HandleOnStartMoveUnits;
        GameManager.OnStopMoveUnits += HandleOnStopMoveUnits;

        // HACK: this should be done some other way
        PlayerMenu menu = FindObjectOfType<PlayerMenu>();
        menu.player = this;

        controls = new Controls();
        controls.Player.Selection.performed += DoSelection;

        controls.Player.Command.performed += DoCommand;

        controls.Enable();
    }

    private void OnDestroy()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        GameManager.OnStartTurn -= HandleOnStartTurn;
        GameManager.OnStartMoveUnits -= HandleOnStartMoveUnits;
        GameManager.OnStopMoveUnits -= HandleOnStopMoveUnits;

        controls.Dispose();
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
        if (MoveCount > GameMode.Singleton.MovesPerTurn) return;

        if (currentCell && selectedUnit)
        {
            selectedUnit.HasAction = true;
            DeselectUnit();
        }

        MoveCount++;
        OnCommandChange?.Invoke();
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

        if (Input.GetKey("left shift"))
        {
            // can backtrack
            selectedUnit.Path.AddCellToPath(currentCell, canBackTrack: true);
        }
        else
        {
            // can't backtrack
            selectedUnit.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedUnit.Path.Show();
    }

    private void SelectUnit(Unit unit)
    {
        // if this unit does not belong to the player, exit
        if (!myUnits.Contains(unit)) return;

        if (!unit.CanMove) return;

        selectedUnit = unit;
        if (selectedUnit.HasAction)
        {
            selectedUnit.HasAction = false;
            MoveCount--;
            OnCommandChange?.Invoke();
        }
        selectedUnit.IsSelected = true;
        selectedUnit.Path.Clear();
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
        if (selectedUnit) selectedUnit.Path.Clear();
        DeselectUnit();
    }

    #endregion

    /********** MARK: Handler Functions **********/
    #region Handler Functions

    private void HandleOnStartTurn()
    {
        MoveCount = 1;
        OnCommandChange?.Invoke(); // HACK: there has to be a better way
    }

    private void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.Team == Team)
        {
            myUnits.Add(unit);
            unit.ToggleMovementDisplay();
        }
    }

    private void HandleOnUnitDepawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void HandleOnStartMoveUnits()
    {
        DeselectUnitAndClearItsPath();
        controls.Disable();
    }

    private void HandleOnStopMoveUnits()
    {
        controls.Enable();
    }

    #endregion
}