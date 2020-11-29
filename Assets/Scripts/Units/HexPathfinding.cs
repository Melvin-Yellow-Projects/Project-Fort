﻿/**
 * File Name: HexPathfinding.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: October 12, 2020
 * 
 * Additional Comments:
 *      This file is an extension of HexGrid.cs. The original version of this file can be found 
 *      here: https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial 
 *      series: Hex Map; this file has been updated it to better fit this project
 **/
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPathfinding : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    /// <summary>
    /// singleton instance of this class
    /// </summary>
    static HexPathfinding instance;

    /// <summary>
    /// priority queue data structure
    /// </summary>
    static HexCellPriorityQueue searchFrontier;

	/// <summary>
	/// TODO: comment var; HACK: im certain this variable isn't needed, but it might speed up
	/// computation
	/// </summary>
	static int searchFrontierPhase;
    
    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        instance = this;
        HexPathfinding.SayHi();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public static bool CanAddCellToPath(Unit unit, HexCell cell)
    {
		HexPath path = unit.Path;
		if (!path.EndCell.IsNeighbor(cell)) return false;

		if (!IsValidCellForSearch(unit, path.EndCell, cell, isUsingQueue: false)) return false;

		if (!IsValidEdgeForSearch(unit, path.EndCell, cell)) return false;

		return true;
    }

    // TODO: comment FindPath
    public static List<HexCell> FindPath(Unit unit, HexCell startCell, HexCell endCell)
    {
        startCell.PathFrom = null; 
        return Search(unit, startCell, endCell);
    }

	/// <summary>
	/// TODO: comment Breadth-First Search function
	/// HACK: this function is mega long
	/// HACK: cells[i].Distance and cells[i].PathFrom are not cleared from previous searches, it's
	/// not necessary to do so... but it might make future features or debugging easier
	/// </summary>
	/// <param name="unit"></param>
	/// <param name="fromCell"></param>
	/// <param name="toCell"></param>
	/// <returns></returns>
	private static List<HexCell> Search(Unit unit, HexCell startCell, HexCell endCell)
	{
		searchFrontierPhase += 2; // initialize new search frontier phase

		// initialize the search priority queue
		if (searchFrontier == null) searchFrontier = new HexCellPriorityQueue();
		else searchFrontier.Clear();

        // add the starting cell to the queue
        startCell.SearchPhase = searchFrontierPhase;
        startCell.Distance = 0;
		searchFrontier.Enqueue(startCell);

		// as long as there is something in the queue, keep searching
		while (searchFrontier.Count > 0)
		{
			// pop current cell 
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			// check if we've found the target cell
			if (current == endCell)
			{
                return GetPathCells(startCell, endCell);
			}

			int currentTurn = (current.Distance - 1) / unit.Speed;

			// search all neighbors of the current cell
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				// check if the neighbors are valid cells to search
				HexCell neighbor = current.GetNeighbor(d);
				if (IsValidCellForSearch(unit, current, neighbor, isUsingQueue: true) &&
                    IsValidEdgeForSearch(unit, current, neighbor))
				{
					// if they are valid, calculate distance and add them to the queue
					int moveCost = GetMoveCostCalculation(current, neighbor);

					// distance is calculated from move cost
					int distance = current.Distance + moveCost;
					int turn = (distance - 1) / unit.Speed;

					// this adjusts the distance if there is left over movement
					// TODO: is this the system we want?
					if (turn > currentTurn) distance = turn * unit.Speed + moveCost;

					// adding a new cell that hasn't been updated
					if (neighbor.SearchPhase < searchFrontierPhase)
					{
						neighbor.SearchPhase = searchFrontierPhase;
						neighbor.Distance = distance;
						//neighbor.UpdateLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
						neighbor.PathFrom = current;

						// because our lowest distance cost is 1, heuristic is just the DistanceTo()
						neighbor.SearchHeuristic =
							neighbor.coordinates.DistanceTo(endCell.coordinates);

						searchFrontier.Enqueue(neighbor);
					}
					else if (distance < neighbor.Distance) // adjusting cell that's already in queue
					{
						int oldPriority = neighbor.SearchPriority;
						neighbor.Distance = distance;
						//neighbor.UpdateLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
						neighbor.PathFrom = current;
						searchFrontier.Change(neighbor, oldPriority);
					}
				}
			}
		}
		return new List<HexCell>();
	}

	private static List<HexCell> GetPathCells(HexCell startCell, HexCell endCell)
    {
		List<HexCell> cells = new List<HexCell>();

		for (HexCell c = endCell; c != startCell; c = c.PathFrom) cells.Add(c);

		cells.Add(startCell); // since the path is in reverse order...
		cells.Reverse(); // let's reverse it so it's easier to work with

		return cells;
	}

	/// <summary>
	/// todo: comment IsValidCellForSearch; UNDONE: add rivers and water
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	private static bool IsValidCellForSearch(Unit unit, HexCell current, HexCell neighbor,
		bool isUsingQueue)
	{
		// invalid if neighbor is null or if the cell is already out of the queue
		if (isUsingQueue && (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)) return false;

		// if a Unit exists on this cell
		if (neighbor.Unit && neighbor.Unit.Team == unit.Team) return false; // TODO: check unit type

		// invalid if cell is unexplored
		if (!neighbor.IsExplored) return false;

		// neighbor is a valid cell
		return true;
	}

	private static bool IsValidEdgeForSearch(Unit unit, HexCell current, HexCell neighbor)
	{
		// invalid if there is a river inbetween
		//if (current.GetEdgeType(neighbor) == river) return false;

		// invalid if edge between cells is a cliff
		if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) return false;

		// neighbor is a valid cell
		return true;
	}

	/// <summary>
	/// TODO: comment GetMoveCostCalculation; Should this be move cost or Distance calculation?
	/// UNDONE: add rivers, water, edge type calculation, and other
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	public static int GetMoveCostCalculation(HexCell current, HexCell neighbor)
	{
		// starting move cost
		int moveCost = 0;

		if (current.TerrainTypeIndex == 1) // if grass 
		{
			moveCost += 1;
		}
		else
		{
			HexEdgeType edgeType = current.GetEdgeType(neighbor);
			moveCost += (edgeType == HexEdgeType.Flat) ? 2 : 3;
		}

        return moveCost;
	}

    // HACK: this might be a bit expensive
    public static int GetMoveCostCalculation(List<HexCell> cells)
    {
		int moveCost = 0;

        for (int i = 0; i < cells.Count - 1; i++)
        {
			HexCell current = cells[i];
			HexCell neighbor = cells[i + 1];

			moveCost += GetMoveCostCalculation(current, neighbor);

		}
		return moveCost;
    }

	/// <summary>
	/// TODO: comment GetVisibleCells
	/// HACK: this is also soooo close to Search
	/// HACK: verify visibility calculations, will most likely need an update
	/// </summary>
	/// <param name="fromCell"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	private static List<HexCell> GetVisibleCells(HexCell fromCell, int range)
	{
		List<HexCell> visibleCells = ListPool<HexCell>.Get();

		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);

		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;
			visibleCells.Add(current);

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase ||
					!neighbor.Explorable)
				{
					continue;
				}

				int distance = current.Distance + 1;

				// adds view elevation to dist calc; TODO: verify this line
				if (distance + neighbor.ViewElevation > range ||
					distance > fromCoordinates.DistanceTo(neighbor.coordinates))
				{
					continue;
				}

				if (neighbor.SearchPhase < searchFrontierPhase)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance)
				{
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}

	public static void IncreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].IncreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public static void DecreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].DecreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

    private static void SayHi()
    {
        instance.StartCoroutine(DisplayPath());
    }

    private static IEnumerator DisplayPath()
    {
        Debug.Log("HexPathfinding DisplayPath() IEnumerator Checking In");
        yield return null;
    }

	#endregion

}