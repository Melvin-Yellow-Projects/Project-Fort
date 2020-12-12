/**
 * File Name: MapEditor.cs
 * Description: Class to edit a hex Map
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 10, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      Previously known as HexMapEditor.cs
 **/

using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Class for editing a hex map/grid
/// </summary>
public class MapEditor : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("prefab reference to the HexGrid material")]
    public Material terrainMaterial;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    int brushSize;

    int activeElevation;
    int activeTerrainTypeIndex;
    int activeUnitTypeIndex = -1;

    int unitTeamIndex;

    Controls controls;
    bool isSelectionPressed = false;
    bool isDeletionPressed = false;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /// <summary>
    /// Toggles elevation editing
    /// </summary>
    public bool IsSettingElevation { get; set; } = true;

    public bool IsSettingTerrain
    {
        get
        {
            return (activeTerrainTypeIndex >= 0);
        }
    }

    public bool IsSettingUnits
    {
        get
        {
            return (activeUnitTypeIndex >= 0);
        }
    }

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

        controls = new Controls();

        controls.MapEditor.Selection.performed += OnSelection;
        controls.MapEditor.Selection.canceled += OnSelection;

        controls.MapEditor.Deletion.performed += OnDeletion;
        controls.MapEditor.Deletion.canceled += OnDeletion;

        controls.Enable();
        enabled = false;
    }

    private void LateUpdate()
    {
        HandleInput();
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    #endregion

    /********** MARK: Input Functions **********/
    #region Input Functions

    private void OnSelection(InputAction.CallbackContext context)
    {
        isSelectionPressed = context.ReadValueAsButton();
        enabled = context.ReadValueAsButton();
    }

    private void OnDeletion(InputAction.CallbackContext context)
    {
        isDeletionPressed = context.ReadValueAsButton();
        enabled = context.ReadValueAsButton();
    }

    private void HandleInput()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isDeletionPressed) // do deletion
            {
                DestroyUnit();
            }
            else if (isSelectionPressed) // do selection
            {
                HexCell currentCell = HexGrid.Singleton.GetCellUnderMouse();

                if (currentCell && IsSettingUnits) CreateUnit();
                else if (currentCell && IsSettingTerrain) EditCells(currentCell);
            }
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

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
    /// Sets the elevation for the map editor; this function is independent of SetApplyElevation and
    /// will not enable elevation editing if it is turned off
    /// </summary>
    /// <param name="elevation"></param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
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

    public void SetUnitTypeIndex(int index)
    {
        activeUnitTypeIndex = index;
    }

    public void SetUnitTeamIndex(float index)
    {
        unitTeamIndex = (int)index;
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

        if (IsSettingTerrain) cell.TerrainTypeIndex = activeTerrainTypeIndex;

        if (IsSettingElevation) cell.Elevation = activeElevation;
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
    void CreateUnit()
    {
        bool team = (unitTeamIndex == 0);

        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();
        if (cell && !cell.MyUnit) // if the cell exists and the cell does not have a unit...
        {
            Unit unit = Instantiate(Unit.prefab);
            unit.Team = (team) ? 0 : 1;
            HexGrid.Singleton.LoadUnitOntoGrid(unit, cell, Random.Range(0f, 360f));
        }
    }

    /// <summary>
    /// TODO: comment func Destroy Unit
    /// </summary>
    void DestroyUnit()
    {
        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();
        if (cell && cell.MyUnit) // if the cell exists and the cell does have a unit...
        {
            cell.MyUnit.Die();
        }
    }

    #endregion
}