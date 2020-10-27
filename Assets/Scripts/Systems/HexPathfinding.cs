/**
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

    static HexPath manualPath;
    
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

    public static HexPath BuildPath(HexUnit unit, HexCell nextCell, HexDirection newNextDirection)
    {
        // local var initialization
        HexPath path = (unit.HasPath) ? unit.Path : new HexPath(unit); // BUG: must be a new instance of path
        HexCell currentCell = (unit.HasPath) ? unit.Path.LastAction.EndCell : unit.MyCell;
        HexDirection currentDirection = (unit.HasPath) ? 
            path.LastAction.EndDirection : unit.Direction;
        HexPathAction action;

        if (currentCell == nextCell)
        {
            Debug.Log("new rotation... unless same rotation");
            //HexPathAction action = new HexPathAction(currentCell, currentDirection, newNextDirection);
        }
        else
        {
            bool isValid = false;
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (currentCell.GetNeighbor(d) == nextCell)
                {
                    isValid = true;
                    
                    Debug.Log("new move...rotation will be backwards");

                    action = new HexPathAction(currentCell, nextCell, d);
                    path.AddPathAction(action);
                }
            }
            if (!isValid) Debug.LogError("Failed to find new move");
        }
        
        return path; // an existing path, plus the new cell or rotation
    }

    // TODO: comment FindPath
    public static HexPath FindPath(
        HexUnit unit, HexCell startCell, HexCell endCell, HexDirection endDirection)
    {
        startCell.PathFrom = null; 
        return Search(unit, startCell, endCell, endDirection);
    }

    /// <summary>
    /// TODO: comment Breadth-First Search function
    /// HACK: this function is mega long
    /// HACK: cells[i].Distance and cells[i].PathFrom are not cleared from previous searches, it's
    /// not necessary to do so... but it might make future features or debugging easier
    /// </summary>
    /// <param name="fromCell"></param>
    /// <param name="toCell"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    private static HexPath Search(
        HexUnit unit, HexCell startCell, HexCell endCell, HexDirection endDirection)
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
			if (current == endCell) return new HexPath(unit, startCell, endCell, endDirection);

			int currentTurn = (current.Distance - 1) / unit.Speed;

			// search all neighbors of the current cell
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				// check if the neighbors are valid cells to search
				HexCell neighbor = current.GetNeighbor(d);
				if (IsValidCellForSearch(current, neighbor))
				{
					// if they are valid, calculate distance and add them to the queue
					int moveCost = GetMoveCostCalculation(current, neighbor, unit);

                    // distance is calculated from move cost; TODO: i think if distance is a float,
                    // you can give a slight penalty for changing directions, making straighter
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
		return null;
	}

	/// <summary>
	/// TODO: comment GetMoveCostCalculation; Should this be move cost or Distance calculation?
	/// UNDONE: add rivers, water, edge type calculation, and other
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	private static int GetMoveCostCalculation(HexCell current, HexCell neighbor, HexUnit unit)
	{
		// starting move cost
		int moveCost = 0;

		if (current.TerrainTypeIndex == 1) // if grass 
		{
			moveCost += 10;
		}
		else
		{
			HexEdgeType edgeType = current.GetEdgeType(neighbor);
			moveCost += (edgeType == HexEdgeType.Flat) ? 20 : 30;
		}

        /* flank rotation calculation */

        // current unit direction
        HexDirection inDirection; 
        if (current.PathFrom) inDirection = HexMetrics.GetDirection(current.PathFrom, current);
        else inDirection = current.Unit.Direction; // HACK: cant this just be unit.Direction?

        // next unit direction
        HexDirection outDirection; 
        outDirection = HexMetrics.GetDirection(current, neighbor);

        if (HexMetrics.IsFlank(inDirection, outDirection)) moveCost += 10;
        else if (inDirection == outDirection) moveCost += 1; // straight penalty for aesthetic
        // TODO: Verify line above ^^^

        return moveCost;
	}

	/// <summary>
	/// todo: comment IsValidCellForSearch; UNDONE: add rivers and water
	/// </summary>
	/// <param name="current"></param>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	private static bool IsValidCellForSearch(HexCell current, HexCell neighbor)
	{
		// invalid if neighbor is null or if the cell is already out of the queue
		if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase) return false;

		// invalid if cell is underwater
		//if (neighbor.IsUnderwater) return false;

		// if a Unit exists on this cell
		if (neighbor.Unit) return false; // TODO: check unit type

		// invalid if there is a river inbetween
		//if (current.GetEdgeType(neighbor) == river) return false;

		// invalid if edge between cells is a cliff
		if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) return false;

		// invalid if cell is unexplored
		if (!neighbor.IsExplored) return false;

		// neighbor is a valid cell
		return true;
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
        Debug.Log("PathFinding Display Path IEnumerator Checking In");
        yield return null;
    }

	#endregion

}
