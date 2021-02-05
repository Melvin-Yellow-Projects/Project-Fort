/**
 * File Name: UnitData.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 24, 2020
 * 
 * Additional Comments: 
 * 
 *      HACK: Move the serializer code into it's own class?
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 
/// </summary>
public struct UnitData
{
    /************************************************************/
    #region Variables

    public NetworkIdentity netIdentity;

    public List<HexCell> pathCells;

    //public UnitAction action

    #endregion

    public Unit MyUnit
    {
        get
        {
            return netIdentity.GetComponent<Unit>();
        }
    }

    /************************************************************/
    #region Struct Functions

    public static UnitData Instantiate(Unit unit)
    {
        return new UnitData
        {
            netIdentity = unit.netIdentity,
            pathCells = unit.Movement.Path.Cells
        };
    }

    // HACK: can this be added to general utilities?
    public bool DoesConnectionHaveAuthority(NetworkConnection conn)
    {
        return MyUnit.connectionToClient.connectionId == conn.connectionId;
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public static class UnitDataSerializer
{
    /************************************************************/
    #region HexCellData

    public static void WriteUnitData(this NetworkWriter writer, UnitData data)
    {
        writer.WriteNetworkIdentity(data.netIdentity);
        HexCellSerializer.WriteHexCellIndices(writer, data.pathCells);
    }

    public static UnitData ReadUnitData(this NetworkReader reader)
    {
        UnitData data = new UnitData
        {
            netIdentity = reader.ReadNetworkIdentity(),
            pathCells = HexCellSerializer.ReadHexCellIndices(reader)
        };

        return data;
    }
    #endregion
}
