/**
 * File Name: Galeforce.cs
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
[CreateAssetMenu(fileName = "Fatigue Skill", menuName = "Skills/Movement Skills/Fatigue")]
public class Fatigue : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        if (myPiece.Movement.CurrentMovement < myPiece.Movement.MaxMovement)
            myPiece.Movement.CanMove = false;
    }

    #endregion
}
