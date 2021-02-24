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
/// 
/// </summary>
[CreateAssetMenu(fileName = "Bonk Skill", menuName = "Skills/Collision Skill/Bonk")]
public class Bonk : Skill
{
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        myPiece.Movement.CancelAction();
    }

    #endregion
}
