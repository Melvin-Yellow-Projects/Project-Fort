/**
 * File Name: Skill.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 23, 2021
 * 
 * Additional Comments: 
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class Skill : ScriptableObject
{
    /************************************************************/
    #region Class Functions

    public virtual void Invoke(Piece myPiece)
    {
        throw new NotImplementedException();
    }

    #endregion
}
