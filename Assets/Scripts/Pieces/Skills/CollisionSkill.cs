/**
 * File Name: CollisionSkill.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 24, 2021
 * 
 * Additional Comments: 
 * 
 *      Assumes that a collision is currently underway
 *      
 *      HACK: should this take into account ally collisions?
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class CollisionSkill : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        Piece otherPiece = myPiece.CollisionHandler.OtherPiece;

        if (PieceCollisionHandler.IsActiveCollision(myPiece, otherPiece))
        {
            ActiveCollision(myPiece, otherPiece);
        }
        else
        {
            InactiveCollision(myPiece, otherPiece);
        }
    }

    protected virtual void ActiveCollision(Piece piece, Piece otherPiece)
    {
        throw new NotImplementedException();
    }

    protected virtual void InactiveCollision(Piece piece, Piece otherPiece)
    {
        throw new NotImplementedException();
    }

    #endregion
}
