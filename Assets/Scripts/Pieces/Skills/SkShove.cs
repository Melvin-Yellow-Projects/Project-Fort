/**
 * File Name: Shove.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: March 1, 2021
 * 
 * Additional Comments: 
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "Shove", menuName = "Skills/Non-Collision Skills/Shove")]
public class SkShove : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        HexCell neighbor = myPiece.MyCell.GetNeighbor(myPiece.Movement.Direction);

        Debug.Log($"myPiece's direction {myPiece.Movement.Direction}");
        Debug.Log($"myPiece's cell {myPiece.MyCell.name}");
        Debug.Log($"myPiece's neighbor cell {neighbor.name}");
    }

    #endregion
}
