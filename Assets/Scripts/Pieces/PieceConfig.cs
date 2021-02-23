/**
 * File Name: PieceConfig.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 22, 2021
 * 
 * Additional Comments:
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
    [SerializeField] int maxMovement = 4;
    [Tooltip("how far this piece can see")]
    [SerializeField] int visionRange = 1;
    [Tooltip("how many actions this unit is able to perform per turn step")]
    [SerializeField] int movesPerStep = 1;
    [Tooltip("how fast the piece physically moves during turn step")]
    [SerializeField] float travelSpeed = 6f;
    [Tooltip("how fast the piece physically rotates during turn step")]
    [SerializeField] float rotationSpeed = 360f;

    /** Block Settings **/
    [Header("Block Settings")]
    [Tooltip("a list of piece IDs that this piece blocks")]
    [SerializeField] int[] pieceIds; // HACK: this one isn't right

    /** Collision Skill Settings **/
    [Header("Collision Skill Settings")]
    [Tooltip("skill that activates during an ally shared border collision")]
    [SerializeField] Skill allySharedBorderSkill;
    [Tooltip("skill that activates during an ally shared center collision")]
    [SerializeField] Skill allySharedCenterSkill;
    [Tooltip("skill that activates during an ally idle collision")]
    [SerializeField] Skill allyIdleSkill;
    [Tooltip("skill that activates during an enemy shared border collision")]
    [SerializeField] Skill enemySharedBorderSkill;
    [Tooltip("skill that activates during an enemy shared center collision")]
    [SerializeField] Skill enemySharedCenterSkill;
    [Tooltip("skill that activates during an enemy idle collision")]
    [SerializeField] Skill enemyIdleSkill;

    /** Non-Collision Skill Settings **/
    [Header("Non-Collision Skill Settings")]
    [Tooltip("skill that activates when a turn starts")]
    [SerializeField] Skill startTurnSkill;
    [Tooltip("skill that activates when a turn stops")]
    [SerializeField] Skill stopTurnSkill;
    [Tooltip("skill that activates when a turn step starts")]
    [SerializeField] Skill startTurnStepSkill;
    [Tooltip("skill that activates when a turn step stops")]
    [SerializeField] Skill stopTurnStepSkill;

    #endregion

    /************************************************************/
    #region Properties

    /** General Settings **/
    public int Id => id;
    public string ClassTitle => classTitle;
    public string PieceTitle => pieceTitle;
    public int Credits => credits;
    //public Sprite Icon => icon;

    /** Movement Settings **/
    public int MaxMovement => maxMovement;
    public int VisionRange => visionRange;
    public int MovesPerStep => movesPerStep;
    public float TravelSpeed => travelSpeed;
    public float RotationSpeed => rotationSpeed;

    /** Block Settings **/
    public int[] PieceIds => pieceIds;

    /** Collision Skill Settings **/
    public Skill AllySharedBorderSkill => allySharedBorderSkill;
    public Skill AllySharedCenterSkill => allySharedCenterSkill;
    public Skill AllyIdleSkill => allyIdleSkill;
    public Skill EnemySharedBorderSkill => enemySharedBorderSkill;
    public Skill EnemySharedCenterSkill => enemySharedCenterSkill;
    public Skill EnemyIdleSkill => enemyIdleSkill;

    /** Non-Collision Skill Settings **/
    public Skill StartTurnSkill => startTurnSkill;
    public Skill StopTurnSkill => stopTurnSkill;
    public Skill StartTurnStepSkill => startTurnStepSkill;
    public Skill StopTurnStepSkill => stopTurnStepSkill;

    #endregion
}
