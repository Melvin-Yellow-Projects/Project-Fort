/**
 * File Name: SkBonk.cs
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
[CreateAssetMenu(fileName = "Bonk", menuName = "Skills/Collision Skills/Bonk")]
public class SkBonk : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (myPiece.MyTeam == otherPiece.MyTeam)
        {
            myPiece.Movement.Server_Bonk();
        }
        else
        {
            if (otherPiece.TryToCapturePiece(myPiece)) return;
            myPiece.Movement.Server_Bonk();
        }
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        myPiece.Movement.Server_Bonk();
    }

    #endregion
}
