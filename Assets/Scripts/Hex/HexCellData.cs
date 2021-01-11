/**
 * File Name: HexCellData.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 6, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 
/// </summary>
public struct HexCellData // HACK: this could be converted to a struct
{
    /************************************************************/
    #region Variables

    public int index;

    public int elevation;

    public int terrainTypeIndex;

    public bool isExplored;

    #endregion
    /************************************************************/
    #region Struct Functions

    public static HexCellData Instantiate(HexCell cell)
    {
        return new HexCellData
        {
            index = cell.Index,
            elevation = cell.Elevation,
            terrainTypeIndex = cell.TerrainTypeIndex,
            isExplored = cell.IsExplored
        };
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
public static class HexCellDataSerializer
{
    /************************************************************/
    #region Class Functions

    public static void WriteHexCellData(this NetworkWriter writer, HexCellData data)
    {
        writer.Write(data.index);
        writer.Write((byte)data.elevation);
        writer.Write((byte)data.terrainTypeIndex);
        writer.WriteBoolean(data.isExplored);
    }

    public static HexCellData ReadHexCellData(this NetworkReader reader)
    {
        HexCellData data = new HexCellData
        {
            index = reader.ReadInt32(),
            elevation = reader.ReadByte(),
            terrainTypeIndex = reader.ReadByte(),
            isExplored = reader.ReadBoolean()
        };

        return data;
    }
    #endregion
}
