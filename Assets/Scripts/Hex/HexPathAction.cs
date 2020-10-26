/**
 * File Name: HexPathAction.cs
 * Description: TODO: write this
 * 
 * Authors: Will Lacey
 * Date Created: October 25, 2020
 * 
 * Additional Comments:
 **/
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: write HexPathAction class disc
/// </summary>
public class HexPathAction
{
    /********** MARK: Properties **********/ 
    #region Properties

    public HexCell StartCell { private set; get; }

    public HexCell EndCell { private set; get; }

    public HexDirection StartDirection { private set; get; }

    public HexDirection EndDirection { set; get; }

    public HexActionType ActionType { private set; get; }

    #endregion

    /********** MARK: Constructor **********/
    #region Constructor

    /// <summary>
    /// Move Constructor
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="d"></param>
    public HexPathAction(HexCell c1, HexCell c2, HexDirection d)
    {
        StartCell = c1;
        EndCell = c2;
        StartDirection = d;
        EndDirection = d;
        ActionType = HexActionType.Move;
    }

    /// <summary>
    /// Rotation Constructor
    /// </summary>
    /// <param name="c"></param>
    /// <param name="d1"></param>
    /// <param name="d2"></param>
    public HexPathAction(HexCell c, HexDirection d1, HexDirection d2)
    {
        StartCell = c;
        EndCell = c;
        StartDirection = d1;
        EndDirection = d2;
        ActionType = HexActionType.Rotation;
    }

    #endregion

    /********** MARK: Debug **********/
    #region Debug

    public void LogPathAction()
    {
        string str = "Action: " + ActionType;

        if (ActionType == HexActionType.Move)
        {
            str += StartCell.Index + " -> " + EndCell.Index;
        }
        else
        {
            str += StartDirection + " -> " + EndDirection;
        }

        Debug.LogWarning(str);
    }

    #endregion
}
