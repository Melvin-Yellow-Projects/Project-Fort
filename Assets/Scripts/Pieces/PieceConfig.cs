/**
 * File Name: PieceConfig.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 22, 2020
 * 
 * Additional Comments: 
 *      Previously known as UnitCursor & HexCursor.cs
 *      
 *      FIXME: will flash red if it is initialized with error on frame 1
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(menuName = "Piece Configuration")]
public class PieceConfig : ScriptableObject
{
    /************************************************************/
    #region Variables

    /** General Settings **/
    [Header("General Settings")]
    [Tooltip("ID for this piece")]
    [SerializeField] int id = 0;

    [Tooltip("class title name for this piece")]
    [SerializeField] string classTitle = null;

    [Tooltip("piece title name for this piece")]
    [SerializeField] string pieceTitle = null;

    [Tooltip("how much this piece costs")]
    [SerializeField] int credits = 0;

    //[Tooltip("sprite asset for the piece")]
    //[SerializeField] Sprite icon = null;

    /** Movement Settings **/
    [Header("Movement Settings")]
    [Tooltip("max movement this piece is capable of")]
    [SerializeField] protected int maxMovement = 4;

    [Tooltip("how far this piece can see")]
    [SerializeField] protected int visionRange = 1;

    [Tooltip("how many actions this unit is able to perform per turn step")]
    [SerializeField] protected int movesPerStep = 1;

    [Tooltip("how fast the piece physically moves during turn step")]
    [SerializeField] protected float travelSpeed = 6f;

    [Tooltip("how fast the piece physically rotates during turn step")]
    [SerializeField] protected float rotationSpeed = 360f;

    /** Block Settings **/
    [Header("Block Settings")]
    [Tooltip("")]
    [SerializeField] int[] unitId;

    /** Collision Skill Settings **/
    [Header("Collision Skill Settings")]
    [Tooltip("")]
    [SerializeField] Skill AllySharedBorderSkill;

    [Tooltip("")]
    [SerializeField] Skill AllySharedCenterSkill;

    [Tooltip("")]
    [SerializeField] Skill AllyIdleSkill;

    [Tooltip("")]
    [SerializeField] Skill EnemySharedBorderSkill;

    [Tooltip("")]
    [SerializeField] Skill EnemySharedCenterSkill;

    [Tooltip("")]
    [SerializeField] Skill EnemyIdleSkill;

    #endregion
}
