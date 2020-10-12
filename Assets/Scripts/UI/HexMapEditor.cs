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
    [Tooltip("instance reference to the HexGrid in the scene")]
    public HexGrid hexGrid;

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
        // turn off grid
        terrainMaterial.DisableKeyword("GRID_ON");
    }

    /// <summary>
    /// Unity Method; Start() is called before the first frame update
    /// </summary>
    protected void Start()
    {
        // disable hex map editor
        SetEditMode(false);
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
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
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
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            EditCells(currentCell);
        }
    }

    /// <summary>
    /// TODO: comment GetCellUnderCursor
    /// </summary>
    /// <returns></returns>
    HexCell GetCellUnderCursor()
    {
        // Ray for camera to mouse position in world space
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    /// <summary>
    /// TODO: Comment this
    /// </summary>
    /// <param name="toggle"></param>
	public void SetEditMode(bool toggle)
    {
        // toggle on editor mode
        enabled = toggle;

        // reset hexcell display
        hexGrid.SetCellLabel(0); // turn off cells

        // display the bottom panel when in edit mode
        foreach (Transform panel in editorPanels)
        {
            panel.gameObject.SetActive(toggle);

            // HACK: hardcoded string references, this could be expanded into its own class
            if (panel.name == "Cell Label Panel")
            {
                // reset cell UI toggle in bottom panel
                panel.Find("Cell Panel").Find("Toggle ---").GetComponent<Toggle>().isOn = true;
            }
        }

        // toggle game UI
        FindObjectOfType<HexGameUI>().enabled = !toggle;

        // clear paths existing on the hex grid
        HexPathfinding.ClearPath();

        // toggle map visibility when in edit mode by setting a global shader keyword
        if (toggle) Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        else Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
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
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        // top half of cells, excluding the center
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
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
        hexGrid.StopAllCoroutines();

        hexGrid.SetCellLabel(index);
    }

    /// <summary>
    /// TODO: comment func CreateUnit
    /// </summary>
    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit) // if the cell exists and the cell does not have a unit...
        {
            hexGrid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f));
        }
    }

    /// <summary>
    /// TODO: comment func Destroy Unit
    /// </summary>
    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit) // if the cell exists and the cell does have a unit...
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }

    #endregion
}