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

	[Tooltip("reference to the HexCell Label prefab")]
	public Text cellLabelPrefab;

	[Tooltip("reference to the HexGridChunk prefab")]
	public HexGridChunk chunkPrefab;

	/* Settings */
	[Header("Settings")]
	[Tooltip("number of chunk columns")]
	public int chunkCountX = 4;

	[Tooltip("number of chunk rows")]
	public int chunkCountZ = 3;

	[Tooltip("default/initial cell color")]
	public Color defaultColor = Color.white;

	/* Private & Protected Variables */

    /// <summary>
    /// width? TODO: rewrite
    /// </summary>
	private int cellCountX;

    /// <summary>
    /// height? TODO: rewrite
    /// </summary>
	private int cellCountZ;

	HexGridChunk[] chunks;

	private HexCell[] cells;

	public Texture2D noiseSource; // TODO: i dont like using this file as a intermediary 

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	/// <summary>
	/// Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	protected void Awake()
	{
        // Set HexMetrics's noise
		HexMetrics.noiseSource = noiseSource;

        // initialize width and height
		cellCountX = chunkCountX * HexMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        // create chunks
		CreateChunks();

		// create cells 
		CreateCells();
	}

	/// <summary>
	/// Unity Method; This function is called when the object becomes enabled and active
	/// </summary>
	protected void OnEnable()
	{
		HexMetrics.noiseSource = noiseSource;
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	void CreateChunks()
	{
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		int i = 0;
		for (int z = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				HexGridChunk chunk = Instantiate(chunkPrefab); // TODO: change line
				chunks[i++] = chunk;
				chunk.transform.SetParent(transform);
			}
		}
	}

	void CreateCells()
	{
		cells = new HexCell[cellCountZ * cellCountX];

		int i = 0;
		for (int z = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

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

		// instantiate cell under the grid at its calculated position TODO: rewrite comment
		HexCell cell = Instantiate<HexCell>(cellPrefab);
		cells[i] = cell;
		cell.transform.localPosition = cellPosition;

		// assign the cell the grid's default color
		cell.Color = defaultColor;

		// calculate cell's coordinates
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		// instantiate the cell's label under the grid's Canvas TODO: rewrite comment
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(cellPosition.x, cellPosition.z);
		label.text = cell.coordinates.ToStringOnSeparateLines(); // cube coordinates

		// UNDONE: offset coordinates
		//label.text = "i:" + i.ToString() + "\nx:" + x.ToString() + "\nz:" + z.ToString(); 

		// set label reference to cell's UI RectTransform
		cell.uiRectTransform = label.rectTransform;

        // set cell's elevation
		cell.Elevation = 0;

        // Neighbor Logic
		// assign cell neighbors; skip first column, then connect West cell neighbors
		if (x > 0) cell.SetNeighbor(HexDirection.W, cells[i - 1]);

		// skip first row, then connect remaining cells
		if (z > 0)
		{
			// is z even? then connect even rows cells' Southeast & Southwest neighbors
			if ((z % 2) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]); // (i - width) gets neighbor i
                if (x > 0) cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1) cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
            }
        }

        // add cell to its chunk
		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk(int x, int z, HexCell cell)
	{
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
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
		int index = coordinates.X + (coordinates.Z * cellCountX) + (coordinates.Z / 2);

        // return cell using index
		return cells[index];
	}

	#endregion
}
