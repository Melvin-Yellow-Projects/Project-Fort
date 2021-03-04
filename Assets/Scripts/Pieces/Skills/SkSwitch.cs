/**
 * File Name: SkSwitch.cs
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
[CreateAssetMenu(fileName = "Switch", menuName = "Skills/Collision Skills/Switch")]
public class SkSwitch : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        //HexCell temp = myPiece.Movement.myCell;
        //myPiece.Movement.myCell = otherPiece.Movement.myCell;
        //otherPiece.Movement.myCell = temp;
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        throw new NotImplementedException();
    }

    #endregion
}
