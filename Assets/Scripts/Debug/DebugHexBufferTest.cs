using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHexBufferTest : MonoBehaviour
{
    private void Awake()
    {
        HexBuffer hexBuffer = new HexBuffer();

        Debug.Log("15 as byte");
        hexBuffer.Clear();
        hexBuffer.Write((byte)15);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadByte()}");

        Debug.Log("1");
        hexBuffer.Clear();
        hexBuffer.Write(1);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("0");
        hexBuffer.Clear();
        hexBuffer.Write(0);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("-1");
        hexBuffer.Clear();
        hexBuffer.Write(-1);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("8");
        hexBuffer.Clear();
        hexBuffer.Write(8);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("16");
        hexBuffer.Clear();
        hexBuffer.Write(16);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("32");
        hexBuffer.Clear();
        hexBuffer.Write(32);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("64");
        hexBuffer.Clear();
        hexBuffer.Write(64);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        //Debug.Log("128");
        //hexBuffer.Clear();
        //hexBuffer.Write(128);
        //hexBuffer.Log();
        //Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("256");
        hexBuffer.Clear();
        hexBuffer.Write(256);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadInt32()}");

        Debug.Log("true and false");
        hexBuffer.Clear();
        hexBuffer.Write(true);
        hexBuffer.Write(false);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadBoolean()} and {hexBuffer.ReadBoolean()}");

        Debug.Log("float");
        hexBuffer.Clear();
        hexBuffer.Write(4.1234567890f);
        hexBuffer.Log();
        Debug.Log($"reading buffer {hexBuffer.ReadSingle()}");
    }
}
