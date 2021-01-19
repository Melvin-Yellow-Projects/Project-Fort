using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHexBufferTest : MonoBehaviour
{
    private void Awake()
    {
        HexBuffer hexBuffer = new HexBuffer();

        Debug.Log(hexBuffer.IsEmpty());

        Debug.Log("15 as byte");
        hexBuffer.Clear();
        hexBuffer.Write((byte)15, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadByte()}");

        Debug.Log("1");
        hexBuffer.Clear();
        hexBuffer.Write(1, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("0");
        hexBuffer.Clear();
        hexBuffer.Write(0, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("-1");
        hexBuffer.Clear();
        hexBuffer.Write(-1, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("8");
        hexBuffer.Clear();
        hexBuffer.Write(8, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("16");
        hexBuffer.Clear();
        hexBuffer.Write(16, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("32");
        hexBuffer.Clear();
        hexBuffer.Write(32, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("64");
        hexBuffer.Clear();
        hexBuffer.Write(64, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("128");
        hexBuffer.Clear();
        hexBuffer.Write(128, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("256");
        hexBuffer.Clear();
        hexBuffer.Write(256, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("true and false");
        hexBuffer.Clear();
        hexBuffer.Write(true, useStream: false);
        hexBuffer.Write(false, useStream: false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadBoolean()} and {hexBuffer.ReadBoolean()}");

        Debug.Log("float");
        hexBuffer.Clear();
        hexBuffer.Write(4.123456f, useStream: false);
        hexBuffer.Log();
        float val = hexBuffer.ReadSingle();
        Debug.Log($"reading buffer {val}");
        hexBuffer.Write(val, useStream: false);
        Debug.Log($"reading buffer {hexBuffer.ReadSingle()}");
    }
}
