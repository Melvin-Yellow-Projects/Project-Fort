/**
 * File Name: HexPath.cs
 * Description: TODO: write this
 * 
 * Authors: Will Lacey
 * Date Created: October 18, 2020
 * 
 * Additional Comments:
 *      HACK: this class creates a lot of instances, and then deletes them very fast
 **/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class HexPath
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    Unit unit;

    //List<HexCell> cells = ListPool<HexCell>.Get();
    List<HexCell> cells = new List<HexCell>();

    HexCursor curser;

    int moveCost = 0;

    #endregion

    /********** MARK: Public Properties **********/
    #region Public Properties

    public int Length
    {
        get
        {
            return cells.Count;
        }
    }

    public bool HasPath
    {
        get
        {
            return (cells.Count > 1);
        }
    }

    public bool IsNextStepValid { get; set; } = false;

    public HexCell this[int i]
    {
        get
        {
            return cells[i];
        }
        set
        {
            cells[i] = value;
        }
    }

    #endregion

    /********** MARK: Private Properties **********/
    #region Private Properties

    private HexCell StartCell
    {
        get
        {
            return unit.MyCell;
        }
    }

    // HACK: this is a temp fix on behalf of pathfinding
    public HexCell EndCell
    {
        get
        {
            return cells[cells.Count - 1];
        }
    }

    #endregion

    /********** MARK: Constructors **********/
    #region Constructor

    public HexPath(Unit unit)
    {
        this.unit = unit;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void AddCellToPath(HexCell cell, bool canBackTrack)
    {
        // initialize if new path
        if (cells.Count == 0) cells.Add(unit.MyCell);

        bool manualPath = false;
        if (!cells.Contains(cell) || canBackTrack)
        {
            if(HexPathfinding.CanAddCellToPath(unit, cell))
            {
                cells.Add(cell);

                if (HexPathfinding.GetMoveCostCalculation(cells) <= unit.Speed)
                {
                    manualPath = true;
                    return;
                }
            }
        }

        cells.Clear();
        cells = HexPathfinding.FindPath(unit, StartCell, cell);

        if (curser) curser.HasError = (HexPathfinding.GetMoveCostCalculation(cells) > unit.Speed);

        //// HACK: this is mega confusing
        //if (!HexPathfinding.CanAddCellToPath(unit, cell) || (cells.Contains(cell) && !canBackTrack))
        //{
        //    Debug.Log("Using A* to determine path");
        //    cells.Clear();
        //    cells = HexPathfinding.FindPath(unit, StartCell, cell);
        //    // gray out path if it is too far
        //}
        //else
        //{
        //    cells.Add(cell);
        //}

        //if (curser) curser.HasError = (HexPathfinding.GetMoveCostCalculation(cells) > unit.Speed);
    }

    public void RemoveTailCells(int numberToRemove)
    {
        if (numberToRemove > cells.Count) Debug.LogError("Removing more cells than in Path!");

        for (int i = 0; i < numberToRemove; i++) cells.RemoveAt(0);

        if (unit.MyCell != cells[0]) Debug.LogWarning("Tail cell does not equal Unit's cell!");
    }

    /// <summary>
    /// TODO: comment ShowPath
    /// HACK: show path and clear path can be compressed into one function
    /// </summary>
    /// <param name="speed"></param>
    public void Show()
    {
        if (!HasPath)
        {
            if (curser != null) curser.DestroyCurser();
            return;
            
        }

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < cells.Count; i++)
        {
            //int turn = (cells[i].Distance - 1) / unit.Speed;
            //cells[i].SetLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
            //cells[i].EnableHighlight(Color.white);

            points.Add(cells[i].Position);
        }
        //StartCell.EnableHighlight(Color.blue);
        //endCell.EnableHighlight(Color.red);

        if (curser == null) curser = HexCursor.Initialize(points);
        else curser.Redraw(points);

        curser.IsSelected = unit.IsSelected;
    }

    ///// <summary>
    ///// TODO: comment ClearPath
    ///// </summary>
    //public void Hide()
    //{
    //    // TODO: this is just wrong boy
    //    for (int i = 0; i < cells.Count; i++)
    //    {
    //        //cells[i].SetLabel(null);
    //        cells[i].DisableHighlight();
    //    }
    //}

    public void Clear()
    {
        //Hide();
        if (curser != null) curser.DestroyCurser(); // TODO: i think there needs to be a hide function for the curser

        moveCost = 0;

        cells.Clear();
    }

    #endregion

    /********** MARK: Debug **********/
    #region Debug

    public void LogPath()
    {
        string str = "Path: ";

        for (int i = 0; i < cells.Count - 1; i++)
        {
            str += cells[i].Index + " -> ";
        }

        str += cells[cells.Count - 1].Index;

        Debug.LogWarning(str);
    }

    #endregion
}
