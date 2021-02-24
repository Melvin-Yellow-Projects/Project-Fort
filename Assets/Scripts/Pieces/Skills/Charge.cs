﻿/**
 * File Name: Charge.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 24, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "Charge Skill", menuName = "Skills/Collision Skills/Charge")]
public class Charge : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToCapturePiece(myPiece)) return;
        if (otherPiece.TryToBlockPiece(myPiece)) return;
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToBlockPiece(myPiece)) return;
        otherPiece.Die();
    }

    #endregion
}