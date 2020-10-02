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
using System.IO;

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

	[Tooltip("noise source for Hex Metrics")]
	public Texture2D noiseSource;

	/* Settings */
	[Header("Settings")]
	[Tooltip("number of cell in the x direction; effectively width")]
	public int cellCountX = 20;

	[Tooltip("number of cell in the z direction; effectively height")]
	public int cellCountZ = 15;

	/* Private & Protected Variables */

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

    // priority queue data structure
	HexCellPriorityQueue searchFrontier;

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
		//HexMetrics.InitializeHashGrid(seed);

		CreateMap(cellCountX, cellCountZ);
	}

	/// <summary>
	/// Unity Method; This function is called when the object becomes enabled and active
	/// </summary>
	protected void OnEnable()
	{
		// this class serves as an intermediate for HexMetrics
		HexMetrics.noiseSource = noiseSource;
		//HexMetrics.InitializeHashGrid(seed);
		CreateMap(cellCountX, cellCountZ);
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public bool CreateMap(int x, int z)
	{
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

        // set global cell index
		cell.globalIndex = i;

		// instantiate the cell's label under the grid's Canvas TODO: rewrite comment
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(cellPosition.x, cellPosition.z);

        // set label reference to cell's UI RectTransform
        cell.uiRectTransform = label.rectTransform;

		// set cell label to cube coordinates
		cell.LabelTypeIndex = 1;

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

    /// <summary>
    /// TODO: comment ShowCellUI
    /// </summary>
    /// <param name="visible"></param>
    public void ShowCellUI(bool visible)
    {
        // stops any pathfinding and goes into new mode
		StopAllCoroutines();

		for (int i = 0; i < chunks.Length; i++)
        {
			chunks[i].ShowCellUI(visible);
        }
    }

	// TODO: comment UpdateCellUI
	public void UpdateCellUI(int index)
    {
        for (int i = 0; i < cells.Length; i++)
        {
			cells[i].LabelTypeIndex = index;
        }
	}

	// TODO: comment FindDistancesTo
	public void FindPath(HexCell fromCell, HexCell toCell)
	{
		StopAllCoroutines();
		StartCoroutine(Search(fromCell, toCell));
	}

	// TODO: comment Breadth-First Search function
	private IEnumerator Search(HexCell fromCell, HexCell toCell)
	{
        // initialize the search priority queue
		if (searchFrontier == null) searchFrontier = new HexCellPriorityQueue();
		else searchFrontier.Clear();

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Distance = int.MaxValue;
			cells[i].PathFrom = null;
            cells[i].DisableHighlight(); // disable previous highlights
		}

        // rehighlight the from and to cells
		fromCell.EnableHighlight(Color.blue);
		toCell.EnableHighlight(Color.red);

        // "frequency of 60 iterations per second is slow enough that we can see what's happening"
        // I might be using a different value
        //WaitForSeconds delay = new WaitForSeconds(1 / 120f); // fast
        //WaitForSeconds delay = new WaitForSeconds(1 / 60f); // medium
        WaitForSeconds delay = new WaitForSeconds(1 / 30f); // slow

        // add the starting cell to the queue
        fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);

		// as long as there is something in the queue, keep searching
		while (searchFrontier.Count > 0)
		{
            // wait for delay time
			yield return delay;

			// pop current cell 
			HexCell current = searchFrontier.Dequeue();

			// check if we've found the target cell
			if (current == toCell)
			{
				HighlightCellPath(current);
				break;
			}

            // search all neighbors of the current cell
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
                // check if the neighbors are valid cells to search
				HexCell neighbor = current.GetNeighbor(d);
				if (IsValidCellForSearch(current, neighbor))
				{
					// if they are valid, calculate distance and add them to the queue
					int distance = GetDistanceCalculation(current, neighbor);

                    // adding a new cell that hasn't been updated
                    if (neighbor.Distance == int.MaxValue)
                    {
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;

						// because our lowest distance cost is 1, heuristic is just the DistanceTo()
						neighbor.SearchHeuristic =
                            neighbor.coordinates.DistanceTo(toCell.coordinates);

                        searchFrontier.Enqueue(neighbor);
                    }
                    else if (distance < neighbor.Distance) // adjusting cell that's already in queue
                    {
                        int oldPriority = neighbor.SearchPriority;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current; 
                        searchFrontier.Change(neighbor, oldPriority);
                    }
				}
			}
		}
	}

	/// <summary>
	/// TODO: comment GetDistanceCalculation; UNDONE: add rivers, water, edge type calculation, and
    /// other
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	private int GetDistanceCalculation(HexCell current, HexCell neighbor)
    {
        // starting distance
		int distance = current.Distance;

		//if (current.HasRoadThroughEdge(d)) // roads
		if (neighbor.TerrainTypeIndex == 1) // if grass 
        {
			distance += 1;
		}
        else
        {
			HexEdgeType edgeType = current.GetEdgeType(neighbor);
			distance += (edgeType == HexEdgeType.Flat) ? 5 : 10;

            // if there is special terrain features
			//distance += neighbor.UrbanLevel + neighbor.FarmLevel +
			//			neighbor.PlantLevel;
		}

		return distance;
	}

	/// <summary>
	/// todo: comment IsValidCellForSearch; UNDONE: add rivers and water
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	private bool IsValidCellForSearch(HexCell current, HexCell neighbor)
    {
		// invalid if neighbor is null
		if (neighbor == null) return false;

		// invalid if cell is underwater
		//if (neighbor.IsUnderwater) return false;

        // invalid if there is a river inbetween
        //if (current.GetEdgeType(neighbor) == river) return false;

		// invalid if edge between cells is a cliff
		if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) return false;

        // neighbor is a valid cell
		return true;
    }

	/// <summary>
	/// TODO: comment HighlightCellPath
	/// </summary>
	/// <param name="current"></param>
	private void HighlightCellPath(HexCell current)
    {
		current = current.PathFrom;
		while (current.PathFrom != null)
		{
			current.EnableHighlight(Color.white);
			current = current.PathFrom;
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
	}

    /// <summary>
    /// TODO comment load
    /// </summary>
    /// <param name="reader"></param>
	public void Load(BinaryReader reader, int header)
	{
		// stop searching when another map is loaded
		StopAllCoroutines();

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

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Load(reader);
		}
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Refresh();
		}
	}

	#endregion
}
