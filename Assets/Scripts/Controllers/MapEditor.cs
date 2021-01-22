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
 *      
 *      HACK: the UI for this class is very functional and not well done
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

    int teamIndex = 1;

    Controls controls;
    bool isSelectionPressed = false;
    bool isDeletionPressed = false;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    /// Toggles elevation editing
    /// </summary>
    public bool IsSettingElevation { get; set; } = true;

    /// <summary>
    /// Gets whether or not the Terrain can be set
    /// </summary>
    public bool IsSettingTerrain
    {
        get
        {
            return (activeTerrainTypeIndex >= 0);
        }
    }

    /// <summary>
    /// Gets whether or not a Unit can be set
    /// </summary>
    public bool IsSettingUnits
    {
        get
        {
            return (activeUnitTypeIndex >= 0);
        }
    }

    public bool IsSettingForts { get; set; } = false;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        controls = new Controls();

        controls.MapEditor.Selection.performed += OnSelection;
        controls.MapEditor.Selection.canceled += OnSelection;

        controls.MapEditor.Deletion.performed += OnDeletion;
        controls.MapEditor.Deletion.canceled += OnDeletion;

        controls.Enable();
        enabled = false;
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    private void LateUpdate()
    {
        HandleInput();
    }

    /// <summary>
    /// Unity Method; Called when the GameObject is destroyed
    /// </summary>
    private void OnDestroy()
    {
        controls.Dispose();
    }

    #endregion

    /********** MARK: Input Functions **********/
    #region Input Functions

    /// <summary>
    /// Called when the Selection Action is performed or canceled 
    /// </summary>
    /// <param name="context">binding for this callback, button</param>
    private void OnSelection(InputAction.CallbackContext context)
    {
        isSelectionPressed = context.ReadValueAsButton();
        enabled = context.ReadValueAsButton();
    }

    /// <summary>
    /// Called when the Deletion Action is performed or canceled 
    /// </summary>
    /// <param name="context">binding for this callback, button with modifier</param>
    private void OnDeletion(InputAction.CallbackContext context)
    {
        isDeletionPressed = context.ReadValueAsButton();
        enabled = context.ReadValueAsButton();
    }

    /// <summary>
    /// Handles the deletion and selection input for the Map Editor Action Map
    /// </summary>
    private void HandleInput()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HexCell currentCell = HexGrid.Singleton.GetCellUnderMouse();
        if (!currentCell) return;

        if (isDeletionPressed) // do deletion
        {
            ClearCellOfUnitsAndForts(currentCell);
        }
        else if (isSelectionPressed) // do selection
        {
            if (IsSettingForts) CreateFort(currentCell);
            else if (IsSettingUnits) CreateUnit(currentCell);
            else EditCells(currentCell);
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
    /// Sets the elevation for the map editor; this function is independent of IsSettingElevation
    /// </summary>
    /// <param name="elevation">new elevation</param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    /// <summary>
    /// Sets the terrain material for the map
    /// </summary>
    /// <param name="index">index of material type</param>
    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    /// <summary>
    /// Sets the unit type to be used when a new unit is created
    /// </summary>
    /// <param name="index">index of unit type</param>
    public void SetUnitTypeIndex(int index)
    {
        activeUnitTypeIndex = index;
    }

    /// <summary>
    /// Sets which team to use when a new unit or fort is created
    /// </summary>
    /// <param name="index"></param>
    public void SetTeamIndex(float index)
    {
        teamIndex = (int)index;
    }

    /// <summary>
    /// Edits all HexCells within the brush range starting from the given cell; uses the given
    /// cell's HexCoordinates to loop around all neighbors
    /// </summary>
    /// <param name="center">targeted cell to edit</param>
	private void EditCells(HexCell center)
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
    private void EditCell(HexCell cell)
    {
        if (cell == null) return;

        if (IsSettingTerrain) cell.TerrainTypeIndex = activeTerrainTypeIndex;

        if (IsSettingElevation) cell.Elevation = activeElevation;
    }

    /// <summary>
    /// Toggles the grid for the terrain material
    /// </summary>
    /// <param name="visible"></param>
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
    /// Sets the cell's label on the grid
    /// </summary>
    /// <param name="index">type of label</param>
    public void UpdateCellUI(int index)
    {
        HexGrid.Singleton.SetCellLabel(index);
    }

    private void CreateUnit(HexCell cell)
    {
        if (cell.MyUnit) return;

        Unit unit = Instantiate(Unit.Prefabs[activeUnitTypeIndex]);

        unit.MyCell = cell;
        unit.MyTeam.TeamIndex = teamIndex;
        unit.Movement.Orientation = Random.Range(0, 360f);

        HexGrid.Singleton.ParentTransformToGrid(unit.transform);
    }

    private void CreateFort(HexCell cell)
    {
        if (cell.MyFort) return;

        Fort fort = Instantiate(Fort.Prefab);

        fort.MyCell = cell;
        fort.MyTeam.TeamIndex = teamIndex;
        fort.Orientation = Random.Range(0, 360f);

        HexGrid.Singleton.ParentTransformToGrid(fort.transform);
    }

    private void ClearCellOfUnitsAndForts(HexCell cell)
    {
        if (cell.MyUnit) cell.MyUnit.Die(isPlayingAnimation: false);
        if (cell.MyFort) Destroy(cell.MyFort.gameObject);
    }

    #endregion
}