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
        OtherPiece = other.GetComponent<PieceCollisionHandler>().MyPiece;
        if (!OtherPiece) Debug.LogError("Non-Piece Collision Detected!");

        // detect whether the piece is moving or not
        bool isMyPieceActive = MyPiece.Movement.EnRouteCell;
        bool isOtherPieceActive = OtherPiece.Movement.EnRouteCell;

        // TODO: does HadActionCanceled disable the collision potential of a unit? or swap/shove?
        //isMyPieceActive = MyPiece.Movement.EnRouteCell && !otherPiece.Movement.HadActionCanceled;

        if (isMyPieceActive && isOtherPieceActive) // Active Collision
        {
            if (MyPiece.Movement.EnRouteCell == OtherPiece.Movement.EnRouteCell)
            {
                SharedBorderCollision();
            }
            else
            {
                SharedCenterCollision();
            }
        }
        else // Idle Collision
        {
            if (isMyPieceActive)
            {
                IdleActiveCollision();
            }
            else
            {
                IdleInactiveCollision();
            }
        }
        OtherPiece = null;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void SharedBorderCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllySharedBorderSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemySharedBorderSkill.Invoke(MyPiece);
        }
    }

    private void SharedCenterCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllySharedCenterSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemySharedCenterSkill.Invoke(MyPiece);
        }
    }

    private void IdleActiveCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllyIdleActiveSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemyIdleActiveSkill.Invoke(MyPiece);
        }
    }

    private void IdleInactiveCollision()
    {
        if (MyPiece.MyTeam == OtherPiece.MyTeam)
        {
            MyPiece.Configuration.AllyIdleInactiveSkill.Invoke(MyPiece);
        }
        else
        {
            MyPiece.Configuration.EnemyIdleInactiveSkill.Invoke(MyPiece);
        }
    }

    #endregion
}
