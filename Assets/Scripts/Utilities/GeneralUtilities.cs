using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public static class GeneralUtilities
{
    public static bool IsRunningOnHost()
    {
        if (NetworkServer.localConnection == null) return false;
        return NetworkServer.localConnection.connectionId == NetworkClient.connection.connectionId;
    }

    //public static bool IsMyConnection(NetworkConnection conn)
    //{
    //    return conn.connectionId == NetworkClient.connection.connectionId;
    //}

    public static float Normalization(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static void LogMonoBehaviour(GameObject gameObject)
    {
        Debug.LogWarning("Logging " + gameObject.name);
    }
}
