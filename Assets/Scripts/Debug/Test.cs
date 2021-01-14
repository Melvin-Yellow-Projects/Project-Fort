using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Test : MonoBehaviour
{
    // NOTE: this prefab must be registered by the NetworkManager before spawning
    public GameObject prefab; 

    void Start()
    {
        // create an instance of the object to spawn
        GameObject instance = Instantiate(prefab);

        // instance is now spawned on other machines
        NetworkServer.Spawn(instance);
    }
}
