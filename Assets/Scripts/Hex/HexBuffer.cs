/**
 * File Name: HexBuffer.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 4, 2020
 * 
 * Additional Comments:
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class HexBuffer
{
    /********** Variables **********/
    # region Variables

    string stringBuffer;
    //List<BitArray> bitArrayBuffer;

    BinaryWriter writer;

    int index;

    #endregion

    /********** Constructor **********/
    #region Constructor

    public HexBuffer()
    {
        stringBuffer = "";
        //bitArrayBuffer = new List<BitArray>();
        index = 0;
    }

    public void WriteTo(FileStream input)
    {
        writer = new BinaryWriter(input);
    }

    public void ReadFrom(FileStream input)
    {
        while (input.Position < input.Length)
        {
            int b = input.ReadByte();
            Write((byte)b, useStream: false);
        }

        Log();
        input.Close();
    }

    #endregion

    /********** Class Functions **********/
    #region Class Functions

    public bool IsEmpty()
    {
        return (stringBuffer.Length == 0);
        //return (stringBuffer.Length == 0 || fileStream == null);
    }

    public void Clear()
    {
        stringBuffer = "";
        //bitArrayBuffer.Clear();
        index = 0;

        Close();
    }

    public void Close()
    {
        if (writer != null) writer.Close();
        writer = null;
    }

    private string ToHexString(byte val)
    {
        string hexString = Convert.ToString(val, 16);

        if (hexString.Length == 1)
        {
            hexString += 0;
            string reverse = "";
            for (int i = hexString.Length - 1; i >= 0; i--) reverse += hexString[i];
            return reverse;
        }
        else
        {
            return hexString;
        }
    }

    #endregion

    /********** Writing Functions **********/
    #region Writing Functions

    public void Write(byte val, bool useStream=true)
    {
        stringBuffer += ToHexString(val);

        if (useStream) writer.Write(val);
    }

    public void Write(bool val, bool useStream = true)
    {
        string hexString = "";
        if (val) hexString += "01";
        else hexString += "00";

        stringBuffer += hexString;

        if (useStream) writer.Write(val);
    }

    public void Write(int val, bool useStream = true)
    {
        byte[] arr = BitConverter.GetBytes(val);

        stringBuffer += ToHexString(arr[3]);
        stringBuffer += ToHexString(arr[2]);
        stringBuffer += ToHexString(arr[1]);
        stringBuffer += ToHexString(arr[0]);

        if (useStream)
        {
            writer.Write(val);
            //fileStream.WriteByte(arr[3]);
            //fileStream.WriteByte(arr[2]);
            //fileStream.WriteByte(arr[1]);
            //fileStream.WriteByte(arr[0]);
        }    
    }

    public void Write(float val, bool useStream = true)
    {
        byte[] arr = BitConverter.GetBytes(val);

        stringBuffer += ToHexString(arr[3]);
        stringBuffer += ToHexString(arr[2]);
        stringBuffer += ToHexString(arr[1]);
        stringBuffer += ToHexString(arr[0]);

        if (useStream)
        {
            writer.Write(val);
            //fileStream.WriteByte(arr[3]);
            //fileStream.WriteByte(arr[2]);
            //fileStream.WriteByte(arr[1]);
            //fileStream.WriteByte(arr[0]);
        }
    }

    #endregion

    /********** Reading Functions **********/
    #region Reading Functions

    public byte ReadByte()
    {
        string val = stringBuffer.Substring(index, 2);
        index += 2;
        return Convert.ToByte(val, 16);
    }

    public bool ReadBoolean()
    {
        string val = stringBuffer.Substring(index, 2);
        index += 2;
        if (val.Equals("01")) return true;
        return false;
    }

    public int ReadInt32()
    {
        string val = stringBuffer.Substring(index, 8);
        index += 8;
        return Convert.ToInt32(val, 16);
    }

    public float ReadSingle()
    {
        string val = stringBuffer.Substring(index, 8);
        index += 8;

        uint num = uint.Parse(val, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] floatVals = BitConverter.GetBytes(num);
        return BitConverter.ToSingle(floatVals, 0);
    }

    #endregion
    /********** Debug Functions **********/
    #region Debug Functions

    public void Log()
    {
        Debug.LogWarning("HexBuffer: 0x" + stringBuffer);
    }

    #endregion
}
