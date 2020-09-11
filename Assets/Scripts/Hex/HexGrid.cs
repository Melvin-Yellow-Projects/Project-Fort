/**
 * File Name: HexGrid.cs
 * Description: TODO: write this
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
	public int width = 8;

	[Tooltip("number of rows or z offset coordinates")]
	public int height = 5;

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
	///     Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	protected void Awake()
	{
        // get class's Canvas and Mesh
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		// create cells for each hex column
		cells = new HexCell[height * width];
		for (int x = 0, i = 0; x < width; x++) // new column
		{
			for (int z = 0; z < height; z++) // fill column
			{
				CreateCell(x, z, i++);
			}
		}
	}

	/// <summary>
	///     Unity Method; Start() is called before the first frame update
	/// </summary>
	protected void Start()
	{
		hexMesh.Triangulate(cells);
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	/// <summary>
	///     Instantiates and properly initializes a HexCell prefab 
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
		cellPosition.x *= (1.5f * HexMetrics.outerRadius);

		// calculate z position
		cellPosition.z += (x / 2f); // offset z by half of x (verticle shift)
		cellPosition.z -= (x / 2); // undo offset with integer math (this will effect odd rows)
		cellPosition.z *= (2f * HexMetrics.innerRadius); // multiply by correct z hex metric

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

        // assign the cell the grid's default color
        cell.color = defaultColor;

		// skip first row, then connect South cell neighbors
		if (z > 0) cell.SetNeighbor(HexDirection.S, cells[i - 1]);

        // skip first column, then connect remaining cells
        if (x > 0)
        {
            // is x even? then connect even column cells' Northwest & Southwest neighbors
            if ((x & 1) == 0)
            {
				cell.SetNeighbor(HexDirection.NW, cells[i - height]); // grabs correct index
                if (z > 0) cell.SetNeighbor(HexDirection.SW, cells[i - height - 1]); 
			}
			else // connect odd column column cells' Northwest & Southwest neighbors
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - height]);
				if (z < height - 1) cell.SetNeighbor(HexDirection.NW, cells[i - height + 1]);
			}
        }
}

    /// <summary>
    ///     Colors a cell at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
	public void ColorCell(Vector3 position, Color color)
	{
        // gets the relative local position
		Vector3 localPosition = transform.InverseTransformPoint(position);

        // converts local position into HexCoordinates 
		HexCoordinates coordinates = HexCoordinates.FromPosition(localPosition);

        // get a cell's index from the coordinates
		int index = coordinates.Z + (coordinates.X * height) + (coordinates.X / 2);

		// change a cell's color using the cell index
		HexCell cell = cells[index];
		cell.color = color;

        // retriangulate grid
		hexMesh.Triangulate(cells);
	}

	#endregion
}
