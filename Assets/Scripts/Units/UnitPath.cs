/**
 * File Name: UnitPath.cs
 * Description: TODO: write this
 * 
 * Authors: Will Lacey
 * Date Created: October 18, 2020
 * 
 * Additional Comments:
 *      HACK: this class creates a lot of instances, and then deletes them very fast
 *      
 *      Previously known as HexPath.cs
 **/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class UnitPath
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    Unit unit;

    //List<HexCell> cells = ListPool<HexCell>.Get();
    List<HexCell> cells = new List<HexCell>();

    UnitCursor curser;

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

    public List<HexCell> Cells // HACK: this could be simplified with the variable cells
    {
        get
        {
            return cells;
        }
    }

    public HexCell StartCell
    {
        get
        {
            return unit.MyCell;
            //return cells[0]; // this is the same thing
        }
    }

    public HexCell EndCell
    {
        get
        {
            return cells[cells.Count - 1];
        }
    }

    public HexCell PenultimateCell
    {
        get
        {
            if (Length > 1) return cells[cells.Count - 2];
            return null;
        }
    }

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

    /********** MARK: Constructors **********/
    #region Constructor

    public UnitPath(Unit unit)
    {
        this.unit = unit;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="canBackTrack"></param>
    public void AddCellToPath(HexCell cell, bool canBackTrack)
    {
        // HACK: wow this code is really confusing
        // initialize if new path
        if (cells.Count == 0) cells.Add(unit.MyCell);

        if (!cells.Contains(cell) || canBackTrack)
        {
            if (UnitPathfinding.CanAddCellToPath(unit, cell))
            {
                cells.Add(cell);

                if (UnitPathfinding.GetMoveCostCalculation(cells) <= unit.MaxMovement)
                {
                    return; // ...otherwise reset the path if the path is too long
                }
            }
        }

        if (PenultimateCell == cell)
        {
            cells.Remove(EndCell);
        }
        else
        {
            cells.Clear();
            cells = UnitPathfinding.FindPath(unit, StartCell, cell);
        }
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

        if (curser) curser.Redraw(points); 
        else curser = UnitCursor.Initialize(points);

        curser.IsSelected = unit.IsSelected;
        curser.HasError = (UnitPathfinding.GetMoveCostCalculation(cells) > unit.MaxMovement);
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

        //moveCost = 0;

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
