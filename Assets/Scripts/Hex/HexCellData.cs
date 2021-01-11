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

public class HexCellData
{
    int elevation;

    int terrainTypeIndex;

    bool explored;
}

public static class HexCellDataSerializer
{
    public static void WriteBinaryReader(this NetworkWriter writer, HexCellData data)
    {
        //writer.Write(mapFileVersion);
        //HexGrid.Singleton.Save(writer);
    }

    public static HexCellData ReadBinaryReader(this NetworkReader reader)
    {

        //reader.

        //int header = BinaryReaderBuffer.ReadInt32();
        //HexGrid.Singleton.Load(BinaryReaderBuffer, header);

        //return new MyData(reader.ReadInt32(), reader.ReadSingle());
        return new HexCellData();
    }
}
