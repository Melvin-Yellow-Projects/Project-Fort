using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Test2 : NetworkBehaviour
{
    // NOTE: this prefab must be registered by the NetworkManager before spawning
    public GameObject prefab;

    void Start()
    {
        // client asks server to spawn
        CmdSpawnObject();
    }

    [Command(ignoreAuthority = true)] 
    void CmdSpawnObject()
    {
        // TODO: Validation Logic goes here, can this client spawn this object?

        // create an instance of the object to spawn
        GameObject instance = Instantiate(prefab);

        // instance is now spawned on other machines
        NetworkServer.Spawn(instance);
    }
}
