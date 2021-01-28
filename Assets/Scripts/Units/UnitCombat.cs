/**
 * File Name: UnitCombat.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 15, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as UnitCollisionHandler.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class UnitCombat : MonoBehaviour
{
    /************************************************************/
    #region Properties

    public Unit MyUnit { get; private set; }

    //public bool HasCaptured { get; set; }

    #endregion
    /************************************************************/
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
    [ServerCallback]
    protected void OnTriggerEnter(Collider other)
    {
        Unit otherUnit = other.GetComponent<UnitCombat>().MyUnit;

        // HACK: this should be 100% guarenteed because other collisions are disabled
        if (!otherUnit) return;

        // Debug.Log($"{MyUnit.name} has collided with {otherUnit.name}");

        // is the unit on my team? // TODO: is this okay with the horse unit?
        if (otherUnit.MyTeam == MyUnit.MyTeam)
        {
            // Other Unit is my Ally
            AllyCollision(otherUnit);
        }

        // do i die to this unit?
        else if (otherUnit.Movement.EnRouteCell && !otherUnit.Movement.HadActionCanceled)
        {
            // Other Unit is in route; active combat
            ActiveCollision(otherUnit);
        }

        // the other unit is my enemy, it is idle, and now i have entered idle combat
        else
        {
            // Other Unit is idle; idle combat
            IdleCollision(otherUnit);
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    protected virtual void AllyCollision(Unit otherUnit)
    {
        MyUnit.Movement.CancelAction();
    }

    protected virtual void ActiveCollision(Unit otherUnit)
    {
        MyUnit.Die();
    }

    protected virtual void IdleCollision(Unit otherUnit)
    {
        MyUnit.Movement.CanMove = false;
    }

    #endregion
}
