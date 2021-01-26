/**
 * File Name: DebugGameExecutor.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 23, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as DebugTempExecute.cs
 * 
 *      TODO: Delete this file after game UI is done
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugGameExecutor : MonoBehaviour
{
    private void Start()
    {
        TryDestroyGameObject1();
    }

    private void TryDestroyGameObject1()
    {
        bool isDestroyingGameObject = true;
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            isDestroyingGameObject &= !item.Value.identity.isServer;
        }

        if (isDestroyingGameObject) Destroy(gameObject);
    }

    private void TryDestroyGameObject2()
    {
        bool noServerFound = false;

        foreach (Player player in GameManager.Players)
        {
            HumanPlayer p = (HumanPlayer)player;
            if (p) noServerFound |= p.isServer;
        }

        if (!noServerFound) Destroy(gameObject);
    }
}
