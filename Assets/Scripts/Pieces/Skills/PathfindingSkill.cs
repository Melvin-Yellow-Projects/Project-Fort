/**
 * File Name: PathfindingSkill.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 24, 2021
 * 
 * Additional Comments: 
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class PathfindingSkill : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        throw new NotImplementedException();
    }

    //public override bool IsValidEdgeForPath(HexCell current, HexCell neighbor)
    //{
    //    return base.IsValidEdgeForPath(current, neighbor);
    //}

    //public override bool IsValidCellForPath(HexCell current, HexCell neighbor)
    //{
    //    return base.IsValidCellForPath(current, neighbor);
    //}

    #endregion
}
