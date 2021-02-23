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
    [Tooltip("ID for this piece")]
    [SerializeField] protected int maxMovement = 4;

    [Tooltip("ID for this piece")]
    [SerializeField] protected int visionRange = 1;

    [Tooltip("ID for this piece")]
    [SerializeField] protected int movesPerStep = 1;

    [Header("Aesthetic Settings")]
    [Tooltip("ID for this piece")]
    [SerializeField] protected float travelSpeed = 6f;

    [Tooltip("ID for this piece")]
    [SerializeField] protected float rotationSpeed = 360f;

    /** Collision Skill Settings **/
    [Header("Collision Skill Settings")]
    [SerializeField] Skill AllySharedBorderSkill;
    [SerializeField] Skill AllySharedCenterSkill;
    [SerializeField] Skill AllyIdleSkill;
    [SerializeField] Skill EnemySharedBorderSkill;
    [SerializeField] Skill EnemySharedCenterSkill;
    [SerializeField] Skill EnemyIdleSkill;

    #endregion
}
