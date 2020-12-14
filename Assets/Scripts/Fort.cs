/**
 * File Name: Fort.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Team), typeof(ColorSetter))]
public class Fort : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    HexCell myCell = null;

    public static Fort prefab;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public Team MyTeam
    {
        get
        {
            return GetComponent<Team>();
        }
    }

    public HexCell MyCell
    {
        get
        {
            return myCell;
        }
        set
        {
            myCell = value;
            ValidateLocation();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void ValidateLocation()
    {
        transform.localPosition = myCell.Position;
    }

    #endregion
}
