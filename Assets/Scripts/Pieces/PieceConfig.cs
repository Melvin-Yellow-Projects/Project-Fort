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
    [SerializeField] PieceType type = PieceType.Axe;
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

    /** Capture Settings **/
    [Header("Capture Settings")]
    [Tooltip("a list of piece types that this piece can Capture")]
    [SerializeField] PieceType[] captureTypes;

    ///** Block Settings **/
    //[Header("Block Settings")]
    //[Tooltip("a list of piece types that this piece can Block")]
    //[SerializeField] PieceType[] blockTypes; 

    /** Collision Skill Settings **/
    [Header("Collision Skill Settings")]
    [Tooltip("skill that activates during an ally shared border collision")]
    [SerializeField] Skill allySharedBorderSkill;
    [Tooltip("skill that Skill during an ally shared center collision")]
    [SerializeField] Skill allySharedCenterSkill;
    [Tooltip("skill that activates during an ally idle collision when the piece is moving")]
    [SerializeField] Skill allyIdleActiveSkill;
    [Tooltip("skill that activates during an ally idle collision when the piece is not moving")]
    [SerializeField] Skill allyIdleInactiveSkill;
    [Tooltip("skill that activates during an enemy shared border collision")]
    [SerializeField] Skill enemySharedBorderSkill;
    [Tooltip("skill that activates during an enemy shared center collision")]
    [SerializeField] Skill enemySharedCenterSkill;
    [Tooltip("skill that activates during an enemy idle collision when the piece is moving")]
    [SerializeField] Skill enemyIdleActiveSkill;
    [Tooltip("skill that activates during an enemy idle collision when the piece is not moving")]
    [SerializeField] Skill enemyIdleInactiveSkill;

    /** Non-Collision Skill Settings **/
    [Header("Non-Collision Skill Settings")]
    [Tooltip("skill that activates when a turn starts")]
    [SerializeField] Skill onStartTurnSkill;
    [Tooltip("skill that activates when a turn stops")]
    [SerializeField] Skill onStopTurnSkill;
    [Tooltip("skill that activates when a turn step starts")]
    [SerializeField] Skill onStartTurnStepSkill;
    [Tooltip("skill that activates when a turn step stops")]
    [SerializeField] Skill onStopTurnStepSkill;
    [Tooltip("skill that activates when the piece dies")]
    [SerializeField] Skill onDeathSkill;

    #endregion

    /************************************************************/
    #region Properties

    /** General Settings **/
    public PieceType Type => type;
    public string PieceTitle => pieceTitle;
    public int Credits => credits;
    //public Sprite Icon => icon;

    /** Movement Settings **/
    public int MaxMovement => maxMovement;
    public int VisionRange => visionRange;
    public int MovesPerStep => movesPerStep;
    public float TravelSpeed => travelSpeed;
    public float RotationSpeed => rotationSpeed;

    /** Capture Settings **/
    public PieceType[] CaptureTypes => captureTypes;

    ///** Block Settings **/
    //public PieceType[] BlockTypes => blockTypes;

    /** Collision Skill Settings **/
    public Skill AllySharedBorderSkill => allySharedBorderSkill;
    public Skill AllySharedCenterSkill => allySharedCenterSkill;
    public Skill AllyIdleActiveSkill => allyIdleActiveSkill;
    public Skill AllyIdleInactiveSkill => allyIdleInactiveSkill;
    public Skill EnemySharedBorderSkill => enemySharedBorderSkill;
    public Skill EnemySharedCenterSkill => enemySharedCenterSkill;
    public Skill EnemyIdleActiveSkill => enemyIdleActiveSkill;
    public Skill EnemyIdleInactiveSkill => enemyIdleInactiveSkill;

    /** Non-Collision Skill Settings **/
    public Skill OnStartTurnSkill => onStartTurnSkill;
    public Skill OnStopTurnSkill => onStopTurnSkill;
    public Skill OnStartTurnStepSkill => onStartTurnStepSkill;
    public Skill OnStopTurnStepSkill => onStopTurnStepSkill;
    public Skill OnDeathSkill => onDeathSkill;

    #endregion
}
