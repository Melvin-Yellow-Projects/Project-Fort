// TODO: date???

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

    /// <summary>
    /// Gets the client's player object; the returned player is guarenteed to be either null or
    /// Human Player castable
    /// </summary>
    /// <returns>a client's player object</returns>
    public static Player GetPlayerFromClientConnection()
    {
        if (NetworkClient.connection == null || !NetworkClient.connection.identity) return null;
        return NetworkClient.connection.identity.GetComponent<Player>();
    }

    public static float Normalization(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static void LogMonoBehaviour(GameObject gameObject)
    {
        Debug.LogWarning("Logging " + gameObject.name);
    }
}
