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

    /** Ally Collision Skill Settings **/
    [Header("Ally Collision Skill Settings")]
    [Tooltip("skill that activates during an ally active center collision")]
    [SerializeField] CollisionSkill allyActiveCenterCollisionSkill;
    [Tooltip("skill that activates during an ally active border collision")]
    [SerializeField] CollisionSkill allyActiveBorderCollisionSkill;
    [Tooltip("skill that activates during an ally inactive collision when this piece is active")]
    [SerializeField] CollisionSkill allyInactiveCollision;

    /** Enemy Collision Skill Settings **/
    [Header("Enemy Collision Skill Settings")]
    [Tooltip("skill that activates during an enemy active center collision")]
    [SerializeField] CollisionSkill enemyActiveCenterCollisionSkill;
    [Tooltip("skill that activates during an enemy active border collision")]
    [SerializeField] CollisionSkill enemyActiveBorderCollisionSkill;
    [Tooltip("skill that activates during an enemy inactive collision when this piece is active")]
    [SerializeField] CollisionSkill enemyInactiveCollision;

    /** Movement Skill Settings **/
    [Header("Movement Skill Settings")]
    //[Tooltip("skill that activates when a unit wants to ")]
    //[SerializeField] PathfindingSkill validEdgeSkill;
    //[Tooltip("skill that activates when a turn starts")]
    //[SerializeField] PathfindingSkill validCellSkill;
    [Tooltip("skill that activates when a turn ends; this is used to determine a piece's movement")]
    [SerializeField] Skill onStopTurnSkill;

    /** Other Skill Settings **/
    [Header("Other Skill Settings")]
    [Tooltip("skill that activates when a turn starts")]
    [SerializeField] Skill onStartTurnSkill;
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

    /** Collision Skill Settings **/
    public Skill AllyActiveBorderCollisionSkill => allyActiveBorderCollisionSkill;
    public Skill AllyActiveCenterCollisionSkill => allyActiveCenterCollisionSkill;
    public Skill AllyInactiveCollision => allyInactiveCollision;
    public Skill EnemyActiveBorderCollisionSkill => enemyActiveBorderCollisionSkill;
    public Skill EnemyActiveCenterCollisionSkill => enemyActiveCenterCollisionSkill;
    public Skill EnemyInactiveCollision => enemyInactiveCollision;

    /** Movement Skill Settings **/
    public Skill OnStopTurnSkill => onStopTurnSkill;

    /** Other Skill Settings **/
    public Skill OnStartTurnSkill => onStartTurnSkill;
    public Skill OnStartTurnStepSkill => onStartTurnStepSkill;
    public Skill OnStopTurnStepSkill => onStopTurnStepSkill;
    public Skill OnDeathSkill => onDeathSkill;

    #endregion
}
