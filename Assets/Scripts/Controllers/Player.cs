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


/// <summary>
/// TODO: comment class
/// </summary>
public class Player : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("instance reference to the HexGrid in the scene")]
    public HexGrid grid; // HACK: remove this variable from Player

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

        GameManager.OnStartMoveUnits += HandleOnStartMoveUnits;
        GameManager.OnStopMoveUnits += HandleOnStopMoveUnits;

        controls = new Controls();
        controls.Player.Selection.performed += DoSelection;

        controls.Player.Command.performed += DoCommand;

        controls.Enable();
    }

    private void OnDestroy()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;

        GameManager.OnStartMoveUnits -= HandleOnStartMoveUnits;
        GameManager.OnStopMoveUnits -= HandleOnStopMoveUnits;
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

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell();
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
        selectedUnit = unit;
        selectedUnit.HasAction = false;
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

    /********** MARK: Input Functions **********/
    #region Input Functions

    private void DoSelection(InputAction.CallbackContext ctx)
    {
        if (currentCell)
        {
            DeselectUnitAndClearItsPath();

            Unit unit = currentCell.MyUnit;

            if (unit && unit.Team == Team) SelectUnit(unit);
        }
    }

    private void DoCommand(InputAction.CallbackContext ctx)
    {
        if (currentCell && selectedUnit)
        {
            selectedUnit.HasAction = true;
            DeselectUnit();
        }
    }

    #endregion

    /********** MARK: Handler Functions **********/
    #region Handler Functions

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