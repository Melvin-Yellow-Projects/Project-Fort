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

        // HACK: this should be 100% guarenteed because other collisions are disabled
        if (!otherUnit) return;

        Debug.Log($"{MyUnit.name} has collided with {otherUnit.name}");

        // is the unit on my team?
        if (otherUnit.MyTeam == MyUnit.MyTeam)
        {
            // Other Unit is my Ally
            MyUnit.Movement.UndoAction();
        }

        // do i die to this unit?
        else if (otherUnit.Movement.EnRouteCell && !otherUnit.Movement.HadActionCanceled)
        {
            // Other Unit is in Active Combat
            MyUnit.Die(); // TODO: verify if we want the hadactioncanceled part
        }

        // the unit is my enemy, and i have killed him, and now i have to stop moving
        else
        {
            // Other Unit is in Inactive Combat
            MyUnit.Movement.StopActions();
        }
    }

    #endregion
}
