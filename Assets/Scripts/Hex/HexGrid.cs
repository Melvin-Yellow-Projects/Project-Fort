/**
 * File Name: HexGrid.cs
 * Description: Script to track the data of a hex map/grid
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Map/grid of HexCells
/// </summary>
public class HexGrid : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	/* Cached References */
	[Header("Cached References")]
	[Tooltip("reference to the HexCell prefab")]
	public HexCell cellPrefab;

	[Tooltip("reference to the HexCell prefab")]
	public Text cellLabelPrefab;

	/* Settings */
	[Header("Settings")]
	[Tooltip("number of cols or x offset coordinates")]
	public int width = 6;

	[Tooltip("number of rows or z offset coordinates")]
	public int height = 6;

	[Tooltip("default/initial cell color")]
	public Color defaultColor = Color.white;

	/* Private & Protected Variables */
	private HexCell[] cells;

	private Canvas gridCanvas;

	private HexMesh hexMesh;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	/// <summary>
	/// Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	protected void Awake()
	{
        // get class's Canvas and Mesh
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		// create cells for each hex row
		cells = new HexCell[height * width];

        int i = 0;
        for (int z = 0; z < height; z++) // new row 
		{
            for (int x = 0; x < width; x++) // fill row
            {
				CreateCell(x, z, i++);
			}
		}
	}

	/// <summary>
	/// Unity Method; Start() is called before the first frame update
	/// </summary>
	protected void Start()
	{
		hexMesh.Triangulate(cells);
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	/// <summary>
	/// Instantiates and properly initializes a HexCell prefab 
	/// </summary>
	/// <param name="x">x hex offset coordinate</param>
	/// <param name="z">z hex offset coordinate</param>
	/// <param name="i">cell's index within array cells</param>
	private void CreateCell(int x, int z, int i)
	{
		// initialize cell position
		Vector3 cellPosition;
		cellPosition.x = x;
		cellPosition.y = 0f;
		cellPosition.z = z;
        
        // calculate x position
        cellPosition.x += (z / 2f); // offset x by half of z (horizontal shift)
		cellPosition.x -= (z / 2); // undo offset with integer math (this will effect odd rows) 
		cellPosition.x *= (2f * HexMetrics.innerRadius); // multiply by correct z hex metric

        // calculate z position
        cellPosition.z *= (1.5f * HexMetrics.outerRadius);

        // instantiate cell under the grid at its calculated position
        HexCell cell = Instantiate<HexCell>(
			cellPrefab, cellPosition, Quaternion.identity, transform
		);
		cells[i] = cell;

		// calculate cell's coordinates
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		// instantiate the cell's label under the grid's Canvas 
		Text label = Instantiate<Text>(cellLabelPrefab, gridCanvas.transform);
		label.rectTransform.anchoredPosition = new Vector2(cellPosition.x, cellPosition.z);
		label.text = cell.coordinates.ToStringOnSeparateLines(); // cube coordinates

		// UNDONE: offset coordinates
		//label.text = "i:" + i.ToString() + "\nx:" + x.ToString() + "\nz:" + z.ToString(); 

		// set label reference to cell's UI RectTransform
		cell.uiRectTransform = label.rectTransform;

		// assign the cell the grid's default color
		cell.color = defaultColor;

		// skip first column, then connect West cell neighbors
		if (x > 0) cell.SetNeighbor(HexDirection.W, cells[i - 1]);

		// skip first row, then connect remaining cells
		if (z > 0)
		{
			// is z even? then connect even rows cells' Southeast & Southwest neighbors
			if ((z % 2) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]); // (i - width) gets neighbor i
                if (x > 0) cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1) cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
            }
        }
	}

	/// <summary>
    /// Gets the cell within the hex grid given a world position; assumes the position is a legal
    /// position
    /// </summary>
    /// <param name="position">world position to be converted</param>
    /// <returns>a HexCell contained by the grid</returns>
	public HexCell GetCell(Vector3 position)
	{
		// gets the relative local position
		Vector3 localPosition = transform.InverseTransformPoint(position);

		// converts local position into HexCoordinates 
		HexCoordinates coordinates = HexCoordinates.FromPosition(localPosition);

		// get a cell's index from the coordinates
		int index = coordinates.X + (coordinates.Z * width) + (coordinates.Z / 2);

        // return cell using index
		return cells[index];
	}

	/// <summary>
    /// Retriangulates the grid
    /// </summary>
	public void Refresh()
	{
		hexMesh.Triangulate(cells);
	}

	#endregion
}
