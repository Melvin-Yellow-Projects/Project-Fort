/**
 * File Name: ICollision.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 18, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollision
{
    void Invoke(Unit unit, Unit otherUnit);
}
