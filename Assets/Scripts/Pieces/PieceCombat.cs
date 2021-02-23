/**
 * File Name: PieceCombat.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 15, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as PieceCombat.cs & UnitCollisionHandler.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCombat : MonoBehaviour
{
    /************************************************************/
    #region Properties

    public Piece MyPiece { get; private set; }

    public bool HasCaptured { get; set; }

    public bool HasBonked { get; set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        MyPiece = GetComponentInParent<Piece>();
    }

    /// <summary>
    /// Unity Method; OnTriggerEnter() is called in FixedUpdate() when a GameObject collides with
    /// another GameObject; The Colliders involved are not always at the point of initial contact
    /// </summary>
    /// <param name="other">other Collider/GameObject that the collision has occured with</param>
    [ServerCallback]
    protected void OnTriggerEnter(Collider other)
    {
        Piece otherPiece = other.GetComponent<PieceCombat>().MyPiece;
        if (!otherPiece) Debug.LogError("Non-Piece Collision Detected!");

        if (otherPiece.Movement.EnRouteCell)
        {

        }
        else
        {

        }


        //// is the unit on my team? // TODO: is this okay with the horse piece?
        //if (otherPiece.MyTeam == MyPiece.MyTeam)
        //{
        //    // Other Unit is my Ally
        //    AllyCollision(otherPiece);
        //}

        //// do i die to this piece? if so other piece is in route; active combat
        //else if (otherPiece.Movement.EnRouteCell && !otherPiece.Movement.HadActionCanceled)
        //{
        //    // is this collision a center collision? (a collision roughly at cell's center?)
        //    if (MyPiece.Movement.EnRouteCell == otherPiece.Movement.EnRouteCell)
        //    {
        //        // this collision was at the cell center
        //        ActiveCenterCollision(otherPiece);
        //    }
        //    else
        //    {
        //        // this collision was at the cell border
        //        ActiveBorderCollision(otherPiece);
        //    }

        //}

        //// the other piece is my enemy, it is idle, and now i have entered idle combat
        //else
        //{
        //    // Other piece is idle; idle combat
        //    IdleCollision(otherPiece);
        //}
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void AllyCollision(Piece otherPiece)
    {
        //foreach (Skill skill in AllyCollisionSkills)

        //Bonk(otherUnit);
        MyPiece.Movement.CancelAction();
    }

    private void ActiveCenterCollision(Piece otherPiece)
    {
        MyPiece.Die();
    }

    private void ActiveBorderCollision(Piece otherPiece)
    {
        gameObject.SetActive(false);
        MyPiece.Die();
    }

    private void IdleCollision(Piece otherPiece)
    {
        MyPiece.Movement.CanMove = false;
    }

    #endregion
}
