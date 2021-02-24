/**
 * File Name: Bonk.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 23, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Piece simply bounces off other piece or cancels its movement
/// </summary>
[CreateAssetMenu(fileName = "Bonk Skill", menuName = "Skills/Collision Skills/Bonk")]
public class Bonk : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToCapturePiece(myPiece)) return;
        myPiece.Movement.CancelAction();
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        myPiece.Movement.CancelAction();
    }

    #endregion
}
