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

	/* Settings */
	[Header("Settings")]
	[Tooltip("a list of available map colors")]
	public Color[] colors;

	/* Private & Protected Variables */
	private Color activeColor;

	private int activeElevation;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	/// <summary>
	/// Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	protected void Awake()
	{
		SelectColor(0);
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

			// edit the cell given the hit position
			EditCell(hexGrid.GetCell(hit.point));
		}
	}

    /// <summary>
    /// Selects a color within HexMapEditor's available colors
    /// </summary>
    /// <param name="index">index of color to select</param>
	public void SelectColor(int index)
	{
		activeColor = colors[index];
	}

    /// <summary>
    /// Sets the elevation for the map editor
    /// </summary>
    /// <param name="elevation"></param>
	public void SetElevation(float elevation)
	{
		activeElevation = (int)elevation;
	}

	/// <summary>
    /// Edits a given HexCell, assigning it new information
    /// </summary>
    /// <param name="cell">HexCell to be editted</param>
	void EditCell(HexCell cell)
	{
		cell.Color = activeColor;
		cell.Elevation = activeElevation;
	}

	#endregion
}