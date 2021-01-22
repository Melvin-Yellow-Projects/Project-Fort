/**
 * File Name: PikeMovement.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 21, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class PikeMovement : UnitMovement
{
    /************************************************************/
    #region Variables

    //const int maxMovement = 8;
    //const int visionRange = 100;
    
    #endregion
    /************************************************************/
    #region Class Functions

    public override bool IsValidEdgeForPath(HexCell current, HexCell neighbor)
    {
        return base.IsValidEdgeForPath(current, neighbor);
    }

    public override bool IsValidCellForPath(HexCell current, HexCell neighbor)
    {
        return base.IsValidCellForPath(current, neighbor);
    }
    #endregion
}
