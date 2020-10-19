﻿/**
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
using System.IO;

/// <summary>
/// Map/grid of HexCells
/// </summary>
public class HexGrid : MonoBehaviour
{
	/********** MARK: Public Variables **********/
	#region Public Variables

	/* Cached References */
	[Header("Cached References")]
	[Tooltip("reference to the HexCell prefab")] // TODO: move this to initializer and make a static hexcell function
	public HexCell cellPrefab;

	[Tooltip("reference to the HexCell Label prefab")] // TODO: move this to initializer and make a static hexcell function
    public Text cellLabelPrefab;

	[Tooltip("reference to the HexGridChunk prefab")]
	public HexGridChunk chunkPrefab;

	/* Settings */
	[Header("Settings")]
	[Tooltip("number of cell in the x direction; effectively width")]
	public int cellCountX = 20;

	[Tooltip("number of cell in the z direction; effectively height")]
	public int cellCountZ = 15;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /// <summary>
    /// number of chunk columns
    /// </summary>
    private int chunkCountX;

	/// <summary>
	/// number of chunk rows
	/// </summary>
	private int chunkCountZ;

	// references to the grid's chunks and cells
	private HexGridChunk[] chunks;
	private HexCell[] cells;

	List<HexUnit> units = new List<HexUnit>();

	HexCellShaderData cellShaderData;

	#endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
	{
		cellShaderData = gameObject.AddComponent<HexCellShaderData>();

		cellShaderData.Grid = this;

		CreateMap(cellCountX, cellCountZ);
	}

	/// <summary>
	/// Unity Method; This function is called when the object becomes enabled and active
	/// </summary>
	protected void OnEnable()
	{
        //HexMetrics.InitializeHashGrid(seed);
        ResetVisibility();
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public bool CreateMap(int x, int z)
	{
		ClearUnits();

		// destroy previous cells and chunks
		if (chunks != null)
		{
			for (int i = 0; i < chunks.Length; i++)
			{
				Destroy(chunks[i].gameObject); // TODO: destroy chunks, no?
			}
		}

		// verify valid map; UNDONE: add support for chunks that are partially filled with cells
		if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
		{
			Debug.LogError("Unsupported map size.");
			return false;
		}

		// initialize number of cells
		cellCountX = x;
		cellCountZ = z;

		// initialize number of chunks
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

        // initialize shader data
		cellShaderData.Initialize(cellCountX, cellCountZ);

		CreateChunks(); // create chunks
		CreateCells(); // create cells

		return true;
	}

	/// <summary>
	/// Creates chunks of cells, builds grid row by row
	/// </summary>
	protected void CreateChunks()
	{
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		int index = 0;
		for (int z = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				chunks[index] = Instantiate(chunkPrefab);
				chunks[index].transform.SetParent(transform);
				index++;
			}
		}
	}

	/// <summary>
	/// Creates each cell in the grid, builds grid row by row
	/// </summary>
	protected void CreateCells()
	{
		cells = new HexCell[cellCountZ * cellCountX];

		int index = 0;
		for (int z = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, index++);
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

		// calculate cell's coordinates
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // set cell's shader data
		cell.ShaderData = cellShaderData;

		// set cell index
		cell.Index = i;
        
        // set if a cell is explorable
        cell.Explorable = x > 0 && z > 0 && x < cellCountX - 1 && z < cellCountZ - 1;

        // set cell name
        cell.name = "hexcell " + i.ToString();

		// instantiate the cell's label under the grid's Canvas TODO: rewrite comment
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(cellPosition.x, cellPosition.z);

		// set label reference to cell's UI RectTransform
		cell.uiRectTransform = label.rectTransform;

        // set cell's elevation
        cell.Elevation = 0;

		/****** Neighbor Logic *****/

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

	/// <summary>
	/// Adds a cell to its corresponding chunk
	/// </summary>
	/// <param name="x">cell's offset coordinate X</param>
	/// <param name="z">cell's offset coordinate Z</param>
	/// <param name="cell">cell to add to its chunk</param>
	void AddCellToChunk(int x, int z, HexCell cell)
	{
		// gets the corresponding chunk given the offset x and z
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;

		// fetch chunk with chunk index calculation
		int chunkIndex = chunkX + chunkZ * chunkCountX;
		HexGridChunk chunk = chunks[chunkIndex];

		// gets the local index for x and z
		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

		// add the cell to the chunk using the local cell index
		int localCellIndex = localX + localZ * HexMetrics.chunkSizeX;
		chunk.AddCell(localCellIndex, cell);
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
		return cells[index]; // BUG: out of bounds error when editing top most cells
	}

	/// <summary>
	/// Gets the corresponding cell given the HexCoordinates
	/// </summary>
	/// <param name="coordinates">a cell's coordinates</param>
	/// <returns>a HexCell</returns>
	public HexCell GetCell(HexCoordinates coordinates)
	{
		// z coordinate validation
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) return null;

		// x coordinate validation
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) return null;

		// gets cell through index calculation
		int index = x + z * cellCountX;
		return cells[index];
	}

    public HexCell GetCell()
    {
        return GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

	/// <summary>
	/// TODO: comment GetCell and touch up vars
	/// </summary>
	/// <returns></returns>
	public HexCell GetCell(Ray inputRay)
	{
		RaycastHit hit;

		// did we hit anything? then return that HexCell
		if (Physics.Raycast(inputRay, out hit))
		{
			// draw line for 1 second
			Debug.DrawLine(inputRay.origin, hit.point, Color.white, 1f);

			return GetCell(hit.point);
		}

		// nothing was found
		return null;
	}

    public Vector3 GetRelativeBridgePoint(Vector3 position)
    {
        // gets the relative local position
        Vector3 localPosition = transform.InverseTransformPoint(position);

        // converts local position into HexCoordinates 
        HexCoordinates coordinates = HexCoordinates.FromPosition(localPosition);

        // get a cell's index from the coordinates
        int index = coordinates.X + (coordinates.Z * cellCountX) + (coordinates.Z / 2);

        // get relative direction
        HexDirection direction = HexMetrics.GetRelativeDirection(position - cells[index].Position);

        // return edge midpoint
        return cells[index].Position + HexMetrics.GetBridge(direction);
    }

    /// <summary>
	/// TODO: comment GetEdgeMidpoint and touch up vars
	/// </summary>
	/// <returns></returns>
	public Vector3 GetRelativeBridgePoint(Ray inputRay)
    {
        RaycastHit hit;

        // did we hit anything? then return that HexCell
        if (Physics.Raycast(inputRay, out hit))
        {
            // draw line for 1 second
            Debug.DrawLine(inputRay.origin, hit.point, Color.blue, 1f);

            return GetRelativeBridgePoint(hit.point);
        }

        // nothing was found
        return new Vector3(); // HACK: idk if this works dawg
    }

    // TODO: comment SetCellLabel
    public void SetCellLabel(int index)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].LabelTypeIndex = index;
        }
    }

	public void AddUnit(HexUnit unit, HexCell location, float orientation)
	{
		units.Add(unit);
		unit.transform.SetParent(transform, false); // HACK: parent the unit to the hex grid... hmm
		unit.Location = location;
		unit.Orientation = orientation;
	}

	public void RemoveUnit(HexUnit unit)
	{
		units.Remove(unit);
		unit.Die();
	}

    public void ClearPaths()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Path = null;
        }
    }

	void ClearUnits()
	{
		for (int i = 0; i < units.Count; i++)
		{
            units[i].Path = null;

            units[i].Die();
		}
		units.Clear();
	}

	public void ResetVisibility()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].ResetVisibility();
		}

		for (int i = 0; i < units.Count; i++)
		{
			HexUnit unit = units[i];
			HexPathfinding.IncreaseVisibility(unit.Location, unit.VisionRange);
		}
	}

	/// <summary>
	/// TODO: comment save
	/// </summary>
	/// <param name="writer"></param>
	public void Save(BinaryWriter writer)
	{
		writer.Write(cellCountX);
		writer.Write(cellCountZ);

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Save(writer);
		}

		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Save(writer);
		}
	}

	/// <summary>
	/// TODO comment load
	/// </summary>
	/// <param name="reader"></param>
	public void Load(BinaryReader reader, int header)
	{
		ClearUnits();

		int x = 20, z = 15;
		if (header >= 1)
		{
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}

		// we dont need to make another map if it's the same size as the existing one
		if (x != cellCountX || z != cellCountZ)
		{
			// check if map failed to be created
			if (!CreateMap(x, z)) return;
		}

		bool originalImmediateMode = cellShaderData.ImmediateMode;
		cellShaderData.ImmediateMode = true;

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Load(reader, header);
		}
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Refresh();
		}

		if (header >= 2) // TODO: update safe files
		{
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++)
			{
				HexUnit.Load(reader, this);
			}
		}

		cellShaderData.ImmediateMode = originalImmediateMode;
	}

	#endregion
}
