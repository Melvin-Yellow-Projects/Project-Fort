/**
 * File Name: PieceData.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 24, 2020
 * 
 * Additional Comments: 
 *      Previously known as UnitData.cs
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
public struct PieceData
{
    /************************************************************/
    #region Variables

    public NetworkIdentity netIdentity;

    public List<HexCell> pathCells;

    //public PieceAction action

    #endregion

    public Piece MyPiece
    {
        get
        {
            return netIdentity.GetComponent<Piece>();
        }
    }

    /************************************************************/
    #region Struct Functions

    public static PieceData Instantiate(Piece piece)
    {
        return new PieceData
        {
            netIdentity = piece.netIdentity,
            pathCells = piece.Movement.Path.Cells
        };
    }

    // HACK: can this be added to general utilities?
    public bool DoesConnectionHaveAuthority(NetworkConnection conn)
    {
        return MyPiece.connectionToClient.connectionId == conn.connectionId;
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public static class PieceDataSerializer
{
    /************************************************************/
    #region HexCellData

    public static void WritePieceData(this NetworkWriter writer, PieceData data)
    {
        writer.WriteNetworkIdentity(data.netIdentity);
        HexCellSerializer.WriteHexCellIndices(writer, data.pathCells);
    }

    public static PieceData ReadPieceData(this NetworkReader reader)
    {
        PieceData data = new PieceData
        {
            netIdentity = reader.ReadNetworkIdentity(),
            pathCells = HexCellSerializer.ReadHexCellIndices(reader)
        };

        return data;
    }
    #endregion
}
