/**
 * File Name: IHexCellOwnable.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/


using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Interface for an MonoBehavior object that can belong to a HexCell
/// </summary>
public interface IHexCellOwnable
{
    /********** MARK: Properties **********/
    #region Properties

    HexCell MyCell { get; set; }

    float Orientation { get; set; }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    void ValidateLocation();

    #endregion

    /********** MARK: Save/Load Functions **********/
    #region Save/Load Functions

    void Save(BinaryWriter writer);

    void Load(BinaryReader reader, int header);

    #endregion
}
