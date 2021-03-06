/**
 * File Name: SkConserve.cs
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
[CreateAssetMenu(fileName = "Conserve", menuName = "Skills/Movement Skills/Conserve")]
public class SkConserve : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        // HACK: is the clear action really needed?
        if (myPiece.Movement.CurrentMovement == 0) myPiece.Movement.Server_ClearMove();
    }

    #endregion
}
