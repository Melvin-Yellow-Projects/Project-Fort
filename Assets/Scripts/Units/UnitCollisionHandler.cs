/**
 * File Name: UnitCollisionHandler.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 15, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCollisionHandler : MonoBehaviour
{
    /********** MARK: Properties **********/
    #region Properties

    public Unit MyUnit { get; private set; }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        MyUnit = GetComponentInParent<Unit>();
    }

    /// <summary>
    /// Unity Method; OnTriggerEnter() is called in FixedUpdate() when a GameObject collides with
    /// another GameObject; The Colliders involved are not always at the point of initial contact
    /// </summary>
    /// <param name="other">other Collider/GameObject that the collision has occured with</param>
    protected void OnTriggerEnter(Collider other)
    {
        Unit otherUnit = other.GetComponent<UnitCollisionHandler>().MyUnit;

        if (!otherUnit) return;

        Debug.Log(otherUnit.name);

        MyUnit.CancelAction();
    }

    #endregion
}
