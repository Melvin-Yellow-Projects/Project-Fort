/**
 * File Name: ICollisionSkill.cs
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
public interface ICollisionSkill
{
    /************************************************************/
    #region Interface Functions

    bool IsPieceCaptured();

    bool IsPieceBlocked();

    #endregion
}
