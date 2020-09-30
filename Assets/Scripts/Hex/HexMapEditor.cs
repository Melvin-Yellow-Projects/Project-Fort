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

/// <summary>
/// Class for editing a hex map/grid
/// </summary>
public class HexMapEditor : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	/* Cached References */
	[Header("Cached References")]
	[Tooltip("instance reference to the HexGrid in the scene")]
	public HexGrid hexGrid;

	[Tooltip("prefab reference to the HexGrid material")]
	public Material terrainMaterial;

	/* Settings */
	// redacted

	/* Private & Protected Variables */
	bool editMode;

	int brushSize;

    /// <summary>
    /// tracks which label is to be displayed on the cells, a value of 1 initializes the cells to
    /// cube coordinates
    /// </summary>
	int activeCellLabelType = 1;

	int activeTerrainTypeIndex;

	bool applyElevation = true;
	private int activeElevation;

	HexCell previousCell;
	HexCell searchFromCell;
	HexCell searchToCell;

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
	/// Unity Method; Update() is called once per frame
	/// </summary>
	protected void Update()
	{
		// TODO: convert GetMouseButton to a specific input action
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

    /// <summary>
    /// Handles the input from a player
    /// </summary>
	protected void HandleInput()
	{
        // Ray and RaycastHit for camera to mouse position in world space
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		// did we hit anything? then color that HexCell
		if (Physics.Raycast(inputRay, out hit))
		{
			// draw line for 1 second
			Debug.DrawLine(inputRay.origin, hit.point, Color.white, 1f);

			HexCell currentCell = hexGrid.GetCell(hit.point);

			// edit the cells given the hit position if in edit mode
			if (editMode)
			{
				EditCells(currentCell);
			}
			else if (
                Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell
                && activeCellLabelType == 2
			) // in nav mode HACK: this is kinda weird b0ss
			{
				if (searchFromCell)
				{
					searchFromCell.DisableHighlight();
				}
				searchFromCell = currentCell;
				searchFromCell.EnableHighlight(Color.blue);
				if (searchToCell)
				{
					hexGrid.FindPath(searchFromCell, searchToCell);
				}

			}
			else if (searchFromCell && searchFromCell != currentCell && activeCellLabelType == 2)
			{
				searchToCell = currentCell;
				hexGrid.FindPath(searchFromCell, searchToCell);
			}
		}
	}

    /// <summary>
    /// TODO: Comment this
    /// </summary>
    /// <param name="toggle"></param>
	public void SetEditMode(bool toggle)
	{
		editMode = toggle;

		// stop navigation calculation
		hexGrid.StopAllCoroutines();

		// display cell labels when not in edit mode
		hexGrid.UpdateCellUI(activeCellLabelType);

		// display cell UI when not in edit mode
		hexGrid.ShowCellUI(!toggle);

        // display the bottom panel when not in edit mode
		transform.Find("Bottom Panel").gameObject.SetActive(!toggle);
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

    public void UpdateCellUI(int index)
    {
		activeCellLabelType = index;

		// stop navigation calculation
		hexGrid.StopAllCoroutines();

		hexGrid.UpdateCellUI(index);
	}

	#endregion
}