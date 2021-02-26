/**
 * File Name: SkGaleforce.cs
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
[CreateAssetMenu(fileName = "Galeforce", menuName = "Skills/Movement Skills/Galeforce")]
public class SkGaleforce : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        if (myPiece.HasCaptured) myPiece.Movement.CanMove = true;

        else if (myPiece.Movement.CurrentMovement < myPiece.Movement.MaxMovement)
            myPiece.Movement.CanMove = false;
    }

    #endregion
}
