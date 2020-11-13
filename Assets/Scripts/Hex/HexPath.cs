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

    HexUnit unit;

    //List<HexCell> cells = ListPool<HexCell>.Get();
    List<HexCell> cells = new List<HexCell>();

    HexCurser curser;

    List<HexPathAction> pathActions = new List<HexPathAction>();

    int moveCost = 0;

    #endregion

    /********** MARK: Public Properties **********/
    #region Public Properties

    public HexCell StartCell
    {
        get
        {
            return unit.MyCell;
        }
    }

    public HexCell EndCell
    {
        get
        {
            return cells[cells.Count - 1];
        }
    }

    public int Length
    {
        get
        {
            return pathActions.Count;
        }
    }

    public List<HexPathAction> PathActions
    {
        get
        {
            return pathActions;
        }
    }

    public HexPathAction LastAction
    {
        get
        {
            return pathActions[pathActions.Count - 1];
        }
    }

    public bool HasPath { get; private set; } = false;

    //public HexPathAction this[int i]
    //{
    //    get
    //    {
    //        return pathActions[i];
    //    }
    //}

    #endregion
        
    /********** MARK: Constructor **********/
    #region Constructor

    public HexPath(HexUnit unit)
    {
        this.unit = unit;
    }

    // HACK: this can probably be optimized
    public HexPath(HexUnit unit, HexCell start, HexCell end, HexDirection endDirection)
    {
        for (HexCell c = end; c != start; c = c.PathFrom)
        {
            cells.Add(c);
        }

        cells.Add(start); // since the path is in reverse order...
        cells.Reverse(); // let's reverse it so it's easier to work with

        this.unit = unit;

        // initialization
        HexCell inCell, outCell = cells[0];
        HexDirection inDir, outDir = unit.Direction; 

        // PathAction calculations
        for (int i = 1; i < cells.Count; i++)
        {
            inCell = outCell;
            outCell = cells[i];
            inDir = outDir;
            outDir = HexMetrics.GetDirection(inCell, outCell);

            if (HexMetrics.IsFlank(inDir, outDir))
            {
                pathActions.Add(new HexPathAction(inCell, inDir, outDir));
            }
            pathActions.Add(new HexPathAction(inCell, outCell, outDir));
        }

        //if (outDir != endDirection)
        //{
        //    pathActions.Add(new HexPathAction(outCell, outDir, endDirection));
        //}
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void AddCellToPath(HexCell cell, bool canBacktrack)
    {
        // initialize new path
        if (cells.Count == 0) cells.Add(unit.MyCell);
        
        if (EndCell.IsNeighbor(cell) && (!cells.Contains(cell) || canBacktrack))
        {
            //moveCost += HexPathfinding.GetMoveCostCalculation(EndCell, cell, unit);
            cells.Add(cell);
            //Debug.Log($"adding cell to path, {moveCost}");

            HexPathfinding.ExpandPath(unit, cell);
        }
        else
        {
            Debug.Log("Using A* to determine path");

            // use A*
            // gray out path if it is too far
        }
    }
    
    public void AddPathAction(HexPathAction action)
    {
        pathActions.Add(action);
    }

    public void RemovePathAction()
    {
        pathActions.RemoveAt(0);
    }

    /// <summary>
    /// TODO: comment ShowPath
    /// HACK: show path and clear path can be compressed into one function
    /// </summary>
    /// <param name="speed"></param>
    public void Show()
    {
        List<Vector3> points = new List<Vector3>();

        //for (int i = 0; i < cells.Count; i++)
        //{
        //    int turn = (int) (cells[i].Distance - 1) / speed;
        //    //cells[i].SetLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
        //    //cells[i].EnableHighlight(Color.white);

        //    points.Add(cells[i].Position);
        //}
        //cells[0].EnableHighlight(Color.blue);
        ////endCell.EnableHighlight(Color.red);

        //if (curser == null) curser = HexCurser.Initialize(points); 

        HexPathAction action = pathActions[0];
        points.Add(action.StartCell.Position);

        for (int i = 0; i < Length; i++)
        {
            action = pathActions[i];
            points.Add(action.EndCell.Position);
        }

        if (curser == null) curser = HexCurser.Initialize(points);
        else curser.Redraw(points);
    }

    /// <summary>
    /// TODO: comment ClearPath
    /// </summary>
    public void Hide()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            //cells[i].SetLabel(null);
            cells[i].DisableHighlight();
        }
    }

    public void Clear()
    {
        Hide();
        if (curser != null) curser.DestroyCurser();

        moveCost = 0;

        cells.Clear();

        pathActions.Clear();

        HasPath = false;
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
