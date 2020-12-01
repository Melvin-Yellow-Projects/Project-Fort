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
    }

    private void OnDestroy()
    {
        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;
    }

    protected void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // verify pointer is not on top of GUI
        {
            UpdateCurrentCell();

            if (Input.GetMouseButtonDown(0)) // HACK: hardcoded input / left click
            {
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1)) //right click
                {
                    //DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    void UpdateCurrentCell()
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

    void DoSelection()
    {
        if (currentCell)
        {
            if (selectedUnit) selectedUnit.IsSelected = false;

            selectedUnit = currentCell.MyUnit;

            if (selectedUnit)
            {
                if (selectedUnit.Team == Team)
                {
                    selectedUnit.Path.Clear();
                    selectedUnit.IsSelected = true;
                }
                else
                {
                    selectedUnit = null;
                }
            }
        }
    }

    void DoPathfinding()
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
            Debug.Log("currentCell:" + currentCell.name);
            selectedUnit.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedUnit.Path.Show();
    }

    #endregion

    /********** MARK: Class Handler Functions **********/
    #region Class Functions

    private void HandleOnUnitSpawned(Unit unit)
    {
        if (unit.Team == Team) myUnits.Add(unit);
    }

    private void HandleOnUnitDepawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    #endregion
}