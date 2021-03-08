﻿/**
 * File Name: SkCapture.cs
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
/// Piece attempts to take the place of the other collision piece
/// </summary>
[CreateAssetMenu(fileName = "Capture", menuName = "Skills/Collision Skills/Capture")]
public class SkCapture : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToCapturePiece(myPiece)) return;
        if (otherPiece.TryToBlockPiece(myPiece)) return;

        // piece has not been captured nor blocked, it won
        myPiece.Configuration.OnStopTurnSkill.Invoke(myPiece); 
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToBlockPiece(myPiece)) return;
        myPiece.Configuration.OnStopTurnSkill.Invoke(myPiece); 

        // HACK: this should be guaranteed to succeed
        myPiece.TryToCapturePiece(otherPiece); 
    }

    #endregion
}