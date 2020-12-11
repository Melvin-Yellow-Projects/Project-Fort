/**
 * File Name: HexMapEditor.cs
 * Description: Class to edit a Hex Map
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 10, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Class for editing a hex map/grid
/// </summary>
public class HexMapEditor : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("prefab reference to the HexGrid material")]
    public Material terrainMaterial;

    [Tooltip("an array of editor panels")]
    public Transform[] editorPanels;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    int brushSize;

    int activeTerrainTypeIndex;

    bool applyElevation = true;
    private int activeElevation;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
    }

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// HACK: direct manipulation of input
    /// </summary>
    protected void Update()
    {
        // TODO: convert GetMouseButton to a specific input action
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.I))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.U)) CreateUnit(true);
                    else CreateUnit(false);
                }
                return;
            }
        }
        //previousCell = null;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Handles the input from a player
    /// HACK: raw input from left shift is used, should be an action
    /// </summary>
    protected void HandleInput()
    {
        HexCell currentCell = HexGrid.Singleton.GetCell();
        if (currentCell)
        {
            EditCells(currentCell);
        }
    }

    /// <summary>
    /// Sets the map editor's brush size; size correlates to how many neighbors to edit after the
    /// targeted cell (a brush size of 0 only edits the targeted cell)
    /// </summary>
    /// <param name="size">new size</param>
    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    /// <summary>
    /// Selects a color within HexMapEditor's available colors; a value of -1 disables color editing
    /// TODO: rewrite method desc
    /// </summary>
    /// <param name="index">index of color to select</param>
    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    /// <summary>
    /// Toggles elevation editing
    /// </summary>
    /// <param name="toggle">enables or disables elevation editting</param>
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /// <summary>
    /// Sets the elevation for the map editor; this function is independent of SetApplyElevation and
    /// will not enable elevation editing if it is turned off
    /// </summary>
    /// <param name="elevation"></param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    /// <summary>
    /// Edits all HexCells within the brush range starting from the given cell; uses the given
    /// cell's HexCoordinates to loop around all neighbors
    /// </summary>
    /// <param name="center">targeted cell to edit</param>
	void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        // bottom half of cells
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(HexGrid.Singleton.GetCell(new HexCoordinates(x, z)));
            }
        }

        // top half of cells, excluding the center
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(HexGrid.Singleton.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    /// <summary>
    /// Edits a given HexCell, assigning it new information
    /// </summary>
    /// <param name="cell">HexCell to be editted</param>
    void EditCell(HexCell cell)
    {
        if (cell == null) return;

        if (activeTerrainTypeIndex >= 0) cell.TerrainTypeIndex = activeTerrainTypeIndex;

        if (applyElevation) cell.Elevation = activeElevation;
    }

    // TODO: comment ShowGrid
    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    /// <summary>
    /// TODO: write update cell ui func
    /// </summary>
    /// <param name="index"></param>
    public void UpdateCellUI(int index)
    {
        // stop navigation calculation
        HexGrid.Singleton.StopAllCoroutines();

        HexGrid.Singleton.SetCellLabel(index);
    }

    /// <summary>
    /// TODO: comment func CreateUnit
    /// </summary>
    void CreateUnit(bool team)
    {
        HexCell cell = HexGrid.Singleton.GetCell();
        if (cell && !cell.MyUnit) // if the cell exists and the cell does not have a unit...
        {
            Unit unit = Instantiate(Unit.prefab);
            unit.Team = (team) ? 0 : 1;
            HexGrid.Singleton.AddUnit(unit, cell, Random.Range(0f, 360f));
        }
    }

    /// <summary>
    /// TODO: comment func Destroy Unit
    /// </summary>
    void DestroyUnit()
    {
        HexCell cell = HexGrid.Singleton.GetCell();
        if (cell && cell.MyUnit) // if the cell exists and the cell does have a unit...
        {
            HexGrid.Singleton.RemoveUnit(cell.MyUnit);
        }
    }

    #endregion
}