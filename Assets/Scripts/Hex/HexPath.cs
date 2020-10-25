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

    //List<HexCell> cells = ListPool<HexCell>.Get();
    List<HexCell> cells;

    HexCurser curser;

    HexUnit unit;

    List<HexPathAction> pathActions;

    #endregion

    /********** MARK: Public Properties **********/
    #region Public Properties

    public int NumberOfCells
    {
        get
        {
            return cells.Count;
        }
    }

    public List<HexCell> Cells
    {
        get
        {
            return cells;
        }
    }

    public int NumberOfPathActions
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

    #endregion

    /********** MARK: Constructor **********/
    #region Constructor

    // HACK: this can probably be optimized
    public HexPath(HexUnit unit, HexCell start, HexCell end, HexDirection endDirection)
    {
        cells = new List<HexCell>();
        for (HexCell c = end; c != start; c = c.PathFrom)
        {
            cells.Add(c);
        }

        cells.Add(start); // since the path is in reverse order...
        cells.Reverse(); // let's reverse it so it's easier to work with

        this.unit = unit;

        // initialization
        pathActions = new List<HexPathAction>();
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

        if (HexMetrics.IsFlank(outDir, endDirection))
        {
            pathActions.Add(new HexPathAction(outCell, outDir, endDirection));
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void RemovePathAction()
    {
        pathActions.RemoveAt(0);
    }

    /// <summary>
    /// TODO: comment ShowPath
    /// HACK: show path and clear path can be compressed into one function
    /// </summary>
    /// <param name="speed"></param>
    public void Show(int speed)
    {
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < cells.Count; i++)
        {
            int turn = (cells[i].Distance - 1) / speed;
            //cells[i].SetLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
            //cells[i].EnableHighlight(Color.white);

            points.Add(cells[i].Position);
        }
        cells[0].EnableHighlight(Color.blue);
        //endCell.EnableHighlight(Color.red);

        if (curser == null) curser = HexCurser.Initialize(points); 
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
