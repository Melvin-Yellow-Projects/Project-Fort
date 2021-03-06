/**
 * File Name: SkSwap.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: March 3, 2021
 * 
 * Additional Comments: 
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "Swap", menuName = "Skills/Non-Collision Skills/Swap")]
public class SkSwap : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        HexCell neighbor = myPiece.MyCell.GetNeighbor(myPiece.Movement.Direction);
        if (!neighbor) return;

        Piece otherPiece = neighbor.MyPiece;
        if (!otherPiece) return;

        if (myPiece.MyTeam != otherPiece.MyTeam || otherPiece.HasMove) return;

        neighbor.MyPiece.Movement.Server_ForceMove(myPiece.Movement.Direction.Opposite());

        //Debug.Log($"myPiece's direction {myPiece.Movement.Direction}");
        //Debug.Log($"myPiece's cell {myPiece.MyCell.name}");
        //Debug.Log($"myPiece's neighbor cell {neighbor.name}");
    }

    #endregion
}
