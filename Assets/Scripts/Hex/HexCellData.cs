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
using System;

/// <summary>
/// 
/// </summary>
public struct HexCellData
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
public static class HexCellSerializer
{
    /************************************************************/
    #region HexCellData

    public static void WriteHexCellData(this NetworkWriter writer, HexCellData data)
    {
        writer.WriteInt32(data.index); // HACK not needed
        writer.WriteByte((byte)data.elevation);
        writer.WriteByte((byte)data.terrainTypeIndex);
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

    /************************************************************/
    #region HexCell Indices

    public static void WriteHexCellIndex(this NetworkWriter writer, HexCell cell)
    {
        writer.WriteInt32(cell.Index);
    }

    public static HexCell ReadHexCellIndex(this NetworkReader reader)
    {
        //reader.ReadInt32();
        HexCell cell = null;
        int index = reader.ReadInt32();
        try
        {
            cell = HexGrid.Singleton.GetCell(index);
        }
        catch (Exception e)
        {
            Debug.LogWarning("client has not loaded map yet and cannot deserialize HexCell");
            //System.Timers.Timer aTimer;
        }

        return cell;
    }

    public static void WriteHexCellIndices(this NetworkWriter writer, List<HexCell> cells)
    {
        writer.WriteInt32(cells.Count);
        for (int i = 0; i < cells.Count; i++) writer.WriteInt32(cells[i].Index);
    }

    public static List<HexCell> ReadHexCellIndices(this NetworkReader reader)
    {
        List<HexCell> cells = new List<HexCell>();

        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++) cells.Add(HexGrid.Singleton.GetCell(reader.ReadInt32()));

        return cells;
    }

    #endregion
}
