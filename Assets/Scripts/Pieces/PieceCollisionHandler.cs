/**
 * File Name: PieceCollisionHandler.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 15, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as UnitCombat.cs & UnitCollisionHandler.cs
 *      
 *      TODO: does HadActionCanceled disable the collision potential of a unit? 
 *      TODO: does a swap enable the collision potential of a unit?
 *      TODO: does a shove enable the collision potential of a unit?
 *      TODO: there is a constraint that for an inactive collision only the active piece decides
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCollisionHandler : MonoBehaviour
{
    /************************************************************/
    #region Properties

    public Piece MyPiece { get; private set; }

    public Piece OtherPiece { get; private set; }

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
        OtherPiece = other.GetComponent<PieceCollisionHandler>().MyPiece;
        if (!OtherPiece) Debug.LogError("Non-Piece Collision Detected!");

        // TODO: does HadActionCanceled disable the collision potential of a unit? or swap/shove?
        //isMyPieceActive = MyPiece.Movement.EnRouteCell && !otherPiece.Movement.HadActionCanceled;

        if (IsActiveCollision(MyPiece, OtherPiece)) // Active Collision
        {
            if (IsCenterCollision(MyPiece, OtherPiece))
            {
                Debug.LogWarning("ActiveCenterCollision");
                ActiveCenterCollision();
            }
            else
            {
                Debug.LogWarning("ActiveBorderCollision");
                ActiveBorderCollision();
            }
        }
        else // Inactive Collision
        {
            Debug.LogWarning("InactiveCollision");
            InactiveCollision();
        }
        OtherPiece = null;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static bool IsActiveCollision(Piece piece, Piece otherPiece)
    {
        return piece.IsActive && otherPiece.IsActive;
    }

    public static bool IsCenterCollision(Piece piece, Piece otherPiece)
    {
        return piece.Movement.EnRouteCell == otherPiece.Movement.EnRouteCell;
    }

    private void ActiveCenterCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllyActiveCenterCollisionSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemyActiveCenterCollisionSkill.Invoke(MyPiece);
        }
    }

    private void ActiveBorderCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllyActiveBorderCollisionSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemyActiveBorderCollisionSkill.Invoke(MyPiece);
        }
    }

    private void InactiveCollision()
    {
        if (!MyPiece.IsActive) return; // let the other piece decide 

        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllyInactiveCollision.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemyInactiveCollision.Invoke(MyPiece);
        }
    }
    #endregion
}
