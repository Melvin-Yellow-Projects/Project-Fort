using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;
public class DebugObserver : NetworkVisibility
{
    /************************************************************/
    #region Variables

    /// <summary>
    /// The maximim range that objects will be visible at.
    /// </summary>
    [Tooltip("The maximum range that objects will be visible at.")]
    public int visRange = 10;

    /// <summary>
    /// How often (in seconds) that this object should update the list of observers that can see it.
    /// </summary>
    [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
    [SerializeField] float visUpdateInterval = 0.1f;

    #endregion
    /************************************************************/
    #region Class Functions

    private IEnumerator RebuildObservers()
    {
        while (true)
        {
            netIdentity.RebuildObservers(false);
            yield return new WaitForSeconds(visUpdateInterval);
        }
    }

    /// <summary>
    /// Callback used by the visibility system to determine if an observer (player) can see this object.
    /// <para>If this function returns true, the network connection will be added as an observer.</para>
    /// </summary>
    /// <param name="conn">Network connection of a player.</param>
    /// <returns>True if the player can see this object.</returns>
    public override bool OnCheckObserver(NetworkConnection conn)
    {
        return DoesConnectionHaveVision(conn);
    }
    
    /// <summary>
    /// Callback used by the visibility system to (re)construct the set of observers that can see this object.
    /// <para>Implementations of this callback should add network connections of players that can see this object to the observers set.</para>
    /// </summary>
    /// <param name="observers">The new set of observers for this object.</param>
    /// <param name="initialize">True if the set of observers is being built for the first time.</param>
    public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
    {
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.identity != null)
            {
                if (DoesConnectionHaveVision(conn)) observers.Add(conn);
            }
        }
    }

    /// <summary>
    /// Callback used by the visibility system for objects on a host.
    /// <para>Objects on a host (with a local client) cannot be disabled or destroyed when they are
    /// not visible to the local client. So this function is called to allow custom code to hide
    /// these objects. A typical implementation will disable renderer components on the object.
    /// This is only called on local clients on a host.</para>
    /// </summary>
    /// <param name="visible">New visibility state.</param>
    public override void OnSetHostVisibility(bool visible)
    {
        if (!isClient) return;

        base.OnSetHostVisibility(visible);

        transform.Find("Player Name").gameObject.SetActive(visible);
    }

    private bool DoesConnectionHaveVision(NetworkConnection conn)
    {
        if (conn == null || conn.identity == null) return false;

        DebugPlayer player = conn.identity.GetComponent<DebugPlayer>();

        return Vector3.Distance(player.MyUnit.transform.position, transform.position) < visRange;
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnStartServer()
    {
        StartCoroutine(RebuildObservers());
        //InvokeRepeating(nameof(RebuildObservers), 0, visUpdateInterval);
    }

    [Server]
    public override void OnStopServer()
    {
        //CancelInvoke(nameof(RebuildObservers));

        StopAllCoroutines();
    }

    #endregion
}
