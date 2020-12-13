﻿/**
 * File Name: Unit.cs
 * Description: Script for managing a hex unit
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// a unit that is able to interact with a hex map 
/// </summary>
public class Unit : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("saturation reduction for when a unit has moved")]
    [SerializeField, Range(0, 1f)] float cannotMoveSaturation = 0.3f;

    // HACK: this needs to be configurable
    const float travelSpeed = 8f; 
    const float rotationSpeed = 360f;
    int currentMovement = 8;
    const int maxMovement = 8;
    const int visionRange = 3;
    const int movesPerStep = 1;

    public static Unit prefab;

    HexCell myCell; 
    HexCell currentTravelCell; // HACK: i really don't like this name

    float orientation;

    bool isSelected = false;

    int team = 0;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a unit is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Player">adds unit to player's list of owned units</subscriber>
    public static event Action<Unit> OnUnitSpawned;

    /// <summary>
    /// Event for when a unit is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Player">removes unit from player's list of owned units</subscriber>
    public static event Action<Unit> OnUnitDepawned;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public HexCell MyCell
    {
        get
        {
            return myCell;
        }
        set
        {
            // if there is a previous cell...
            if (myCell) 
            {
                UnitPathfinding.DecreaseVisibility(myCell, visionRange);
                myCell.MyUnit = null;
            }

            // update for new location
            myCell = value;
            value.MyUnit = this; // sets this hex cell's unit to this one
            UnitPathfinding.IncreaseVisibility(value, visionRange);
            transform.localPosition = value.Position;
        }
    }
    
    /// <summary>
    /// A unit's rotation around the Y axis, in degrees
    /// </summary>
    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public HexDirection Direction
    {
        get
        {
            return HexMetrics.AngleToDirection(orientation);
        }
    }

    public int CurrentMovement
    {
        get
        {
            return currentMovement;
        }
        set // HACK: this should be private but cell needs to set it on combat, it's also just weird
        {
            currentMovement = Mathf.Clamp(value, 0, maxMovement);
            if (!CanMove) Path.Clear();

            MyUnitDisplay.RefreshMovementDisplay();

            float saturation = (CanMove) ? 1 : cannotMoveSaturation;
            MyColorSetter.SetColor(MyTeam.MyColor * saturation);
        }
    }

    public int MaxMovement
    {
        get
        {
            return maxMovement;
        }
    }

    public int VisionRange
    {
        get
        {
            return visionRange * 100;
        }
    }

    public UnitPath Path { get; private set; }

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            if (value != isSelected)
            {
                isSelected = value;
                Path.Show(); // updates path HACK: kinda sloppy
            }
        }
    }

    public bool HasAction { get; set; }

    public bool CanMove
    {
        get
        {
            return (currentMovement > 0);
        }
    }

    public Team MyTeam
    {
        get
        {
            return GetComponent<Team>();
        }
    }

    public UnitDisplay MyUnitDisplay
    {
        get
        {
            return GetComponent<UnitDisplay>();
        }
    }

    public ColorSetter MyColorSetter
    {
        get
        {
            return GetComponent<ColorSetter>(); ;
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    private void OnEnable()
    {
        if (myCell) // prevents failure during recompile
        {
            transform.localPosition = myCell.Position;
            if (currentTravelCell)
            {
                UnitPathfinding.IncreaseVisibility(myCell, visionRange);
                UnitPathfinding.DecreaseVisibility(currentTravelCell, visionRange);
                currentTravelCell = null;
            }
        }
    }

    private void Awake()
    {
        Path = new UnitPath(this);
        currentMovement = maxMovement;

        GameManager.OnStartRound += HandleOnStartRound;
        GameManager.OnStartTurn += HandleOnStartTurn;
        GameManager.OnStopMoveUnits += HandleOnStopMoveUnits;

        MyUnitDisplay.RefreshMovementDisplay();
    }

    private void Start()
    {
        OnUnitSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnUnitDepawned?.Invoke(this);

        GameManager.OnStartRound -= HandleOnStartRound;
        GameManager.OnStartTurn -= HandleOnStartTurn;
        GameManager.OnStopMoveUnits -= HandleOnStopMoveUnits;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions
        
    public void ValidateLocation()
    {
        transform.localPosition = myCell.Position;
    }
    
    public void Die(bool isPlayingAnimation = false)
    {
        if (myCell) UnitPathfinding.DecreaseVisibility(myCell, visionRange);

        myCell.MyUnit = null;

        Path.Clear();

        if (isPlayingAnimation) GetComponent<Death>().Die();
        else Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((byte)team);
    }

    public static void Load(BinaryReader reader, int header, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Unit unit = Instantiate(prefab);
        if (header >= 4) unit.MyTeam.TeamIndex = reader.ReadByte();

        grid.LoadUnitOntoGrid(unit, grid.GetCell(coordinates), orientation);
    }

    #endregion

    /********** MARK: Pathing **********/
    #region Pathing

    public void PrepareNextMove()
    {
        if (!Path.HasPath || !HasAction) return; // TODO: maybe continue to change dir

        Path.IsNextStepValid = false;

        // add unit to the next cell's unit queue
        Path[1].AddUnitToCell(this);
    }

    public void ExecuteNextMove()
    {
        if (!Path.HasPath || !HasAction) return; // TODO: maybe continue to change dir

        StopAllCoroutines();

        if (Path.IsNextStepValid)
        {
            List<HexCell> cells = new List<HexCell>();

            cells.Add(Path[0]);
            cells.Add(Path[1]);

            if (cells[0] != myCell) Debug.LogError("This line should never execute"); // HACK: remove line
            
            StartCoroutine(RouteSuccess(cells));

            // remove path cells
            myCell.MyUnit = null;
            myCell = Path[1];
            Path.RemoveTailCells(numberToRemove: (cells.Count - 1));

            CurrentMovement--;

            // redraw path curser
            Path.Show();
        }
        else
        {
            StartCoroutine(RouteFailure());
            Path.Clear();
            CurrentMovement = 0;
        }
    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    private IEnumerator RouteSuccess(List<HexCell> cells)
    {
        Vector3 a, b, c = cells[0].Position;

        // perform lookat
        yield return LookAt(cells[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        UnitPathfinding.DecreaseVisibility(
            currentTravelCell ? currentTravelCell : cells[0],
            visionRange
        );

        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < cells.Count; i++)
        {
            currentTravelCell = cells[i]; // prevents teleportation

            a = c;
            b = cells[i - 1].Position;
            c = (b + currentTravelCell.Position) * 0.5f;

            UnitPathfinding.IncreaseVisibility(cells[i], visionRange);

            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            UnitPathfinding.DecreaseVisibility(cells[i], visionRange);

            t -= 1f;
        }
        currentTravelCell = null;

        // arriving at the center if the last cell
        a = c;
        b = myCell.Position; // We can simply use the destination here.
        c = b;

        UnitPathfinding.IncreaseVisibility(myCell, visionRange);

        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        transform.localPosition = myCell.Position;
        orientation = transform.localRotation.eulerAngles.y;
    }

    private IEnumerator RouteFailure()
    {
        Vector3 startPosition = Path[0].Position;
        Vector3 endPosition = Path[1].Position;
        endPosition = Vector3.Lerp(startPosition, endPosition, 0.3f);
        
        yield return LookAt(endPosition);

        float t = Time.deltaTime * travelSpeed;

        for (; t < 1f; t += Time.deltaTime * 3 * travelSpeed)
        {
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        for (; t > 0f; t -= Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        transform.localPosition = startPosition;
    }

    public void LookAt(HexDirection direction)
    {
        // HACK: yea dis is hacky, coroutine needs to probably be stopped first
        Vector3 localPoint = HexMetrics.GetBridge(direction);
        StartCoroutine(LookAt(myCell.Position + localPoint));
    }

    IEnumerator LookAt(Vector3 point)
    {
        // locks the y dimension
        point.y = transform.localPosition.y;

        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);

        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f)
        {
            // normalizes the speed so that it's always the same regardless of angle; "To ensure a
            // uniform angular speed, we have to slow down our interpolation as the rotation angle
            // increases."
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;

    }

    public bool IsValidEdgeForPath(HexCell current, HexCell neighbor)
    {
        // invalid if there is a river inbetween
        //if (current.GetEdgeType(neighbor) == river) return false;

        // invalid if edge between cells is a cliff
        if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) return false;

        // neighbor is a valid cell
        return true;
    }

    public bool IsValidCellForPath(HexCell current, HexCell neighbor)
    {
        // if a Unit exists on this cell
        //if (neighbor.MyUnit && neighbor.MyUnit.Team == Team) return false; // TODO: check unit type

        // invalid if cell is unexplored
        if (!neighbor.IsExplored) return false;

        //isValid &= !cell.IsUnderwater; // cell is not underwater

        // neighbor is a valid cell
        return true;
    }

    #endregion

    /********** MARK: Handle Functions **********/
    #region Handle Functions

    private void HandleOnStartRound()
    {
        HasAction = false;
        CurrentMovement = maxMovement;
        //Path.Clear(); 
    }

    private void HandleOnStartTurn()
    {
        HasAction = false;
        //Path.Clear(); 
    }

    private void HandleOnStopMoveUnits()
    {
        if (currentMovement < maxMovement)
        {
            CurrentMovement = 0;
        }
    }

    #endregion

    /********** MARK: Debug **********/
    #region Debug

    ///// <summary>
    ///// Unity Method; Gizmos are drawn only when the object is selected; Gizmos are not pickable;
    ///// This is used to ease setup
    ///// </summary>
    //void OnDrawGizmos()
    //{
    //    if (pathToTravel == null || pathToTravel.Count == 0)
    //    {
    //        return;
    //    }

    //    Vector3 a, b, c = pathToTravel[0].Position;

    //    for (int i = 1; i < pathToTravel.Count; i++)
    //    {
    //        a = c;
    //        b = pathToTravel[i - 1].Position;
    //        c = (b + pathToTravel[i].Position) * 0.5f;
    //        for (float t = 0f; t < 1f; t += Time.deltaTime * travelSpeed)
    //        {
    //            Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //        }
    //    }

    //    a = c;
    //    b = pathToTravel[pathToTravel.Count - 1].Position;
    //    c = b;
    //    for (float t = 0f; t < 1f; t += 0.1f)
    //    {
    //        Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //    }
    //}

    #endregion

}


