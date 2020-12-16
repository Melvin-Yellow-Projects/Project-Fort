/**
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
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(ColorSetter))]
[RequireComponent(typeof(UnitDisplay))]
public class Unit : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("saturation reduction for when a unit has moved")]
    [SerializeField, Range(0, 1f)] float cannotMoveSaturation = 0.3f;

    // HACK: this needs to be configurable
    const float travelSpeed = 6f; 
    const float rotationSpeed = 360f;
    int currentMovement = 8;
    const int maxMovement = 8;
    const int visionRange = 3;
    const int movesPerStep = 1;

    public static Unit prefab;

    HexCell myCell;
    HexCell enRouteCell;

    float orientation;

    bool isSelected = false;

    float interpolator;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a unit is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Player">adds unit to player's list of owned units</subscriber>
    /// <subscriber class="Grid">adds unit to list of units</subscriber>
    public static event Action<Unit> OnUnitSpawned;

    /// <summary>
    /// Event for when a unit is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Player">removes unit from player's list of owned units</subscriber>
    /// <subscriber class="Grid">removes unit from list of units</subscriber>
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
            UnitPathfinding.IncreaseVisibility(myCell, visionRange);
            ValidateLocation();
        }
    }

    //public HexCell ToCell { get; set; }

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

            Display.RefreshMovementDisplay();

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

    public bool IsEnRoute { get; set; }

    public bool HasAction
    {
        get
        {
            return Path.HasPath;
        }
    }

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

    public UnitDisplay Display
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
            return GetComponent<ColorSetter>();
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        name = $"Unit {UnityEngine.Random.Range(0, 10000)}";

        Path = new UnitPath(this);
        currentMovement = maxMovement;

        GameManager.OnStartRound += HandleOnStartRound;
        GameManager.OnStartTurn += HandleOnStartTurn;
        GameManager.OnStopMoveUnits += HandleOnStopMoveUnits;

        Display.RefreshMovementDisplay();
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

    #endregion

    /********** MARK: Pathing **********/
    #region Pathing

    public void DoAction()
    {
        if (!HasAction) return;

        CurrentMovement--;

        List<HexCell> cells = new List<HexCell>();
        cells.Add(Path[0]);
        cells.Add(Path[1]);

        StartCoroutine(Route(cells));
    }

    public void CompleteAction()
    {
        if (!HasAction) return;

        MyCell = enRouteCell; // FIXME: this line is causing alot of problems, did i solve this?

        if (currentMovement == 0) CurrentMovement = 0; // HACK: there should be a flag for if a unit has moved and canceled its action
        else Path.RemoveTailCells(numberToRemove: 1);

        Path.Show();
    }

    public void CancelAction()
    {
        if (!enRouteCell) return;

        StopAllCoroutines(); 

        StartCoroutine(RouteCanceled());

        currentMovement = 0; // HACK: see CompleteAction()
    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    private IEnumerator Route(List<HexCell> cells)
    {
        IsEnRoute = true;
        Vector3 a, b, c = cells[0].Position;

        // perform lookat
        //yield return LookAt(cells[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        UnitPathfinding.DecreaseVisibility(
            (enRouteCell) ? enRouteCell : cells[0],
            visionRange
        );

        interpolator = Time.deltaTime * travelSpeed;
        for (int i = 1; i < cells.Count; i++)
        {
            enRouteCell = cells[i]; // prevents teleportation

            a = c;
            b = cells[i - 1].Position;
            c = (b + enRouteCell.Position) * 0.5f;

            UnitPathfinding.IncreaseVisibility(enRouteCell, visionRange);

            for (; interpolator < 1f; interpolator += Time.deltaTime * travelSpeed)
            {
                //transform.localPosition = Vector3.Lerp(a, b, interpolator);
                transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
                //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator);
                //d.y = 0f;
                //transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            UnitPathfinding.DecreaseVisibility(enRouteCell, visionRange);

            interpolator -= 1f;
        }
        enRouteCell = cells[cells.Count - 1];

        // arriving at the center if the last cell
        a = c;
        b = enRouteCell.Position; // We can simply use the destination here.
        c = b;

        UnitPathfinding.IncreaseVisibility(enRouteCell, visionRange);

        for (; interpolator < 1f; interpolator += Time.deltaTime * travelSpeed)
        {
            //transform.localPosition = Vector3.Lerp(a, b, interpolator);
            transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
            //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator);
            //d.y = 0f;
            //transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        transform.localPosition = enRouteCell.Position;
        //orientation = transform.localRotation.eulerAngles.y;
        IsEnRoute = false;
    }

    private IEnumerator RouteCanceled()
    {   
        IsEnRoute = true;

        //yield return LookAt(myCell.Position);

        if (enRouteCell) UnitPathfinding.DecreaseVisibility(enRouteCell, visionRange);

        Vector3 a = myCell.Position;
        Vector3 b = enRouteCell.Position;
        Vector3 c = b;
        Vector3 d;
        enRouteCell = myCell;
        //for (; interpolator > 0; interpolator -= Time.deltaTime * travelSpeed / 10)
        for (float t = 0; t < 1f; t += Time.deltaTime * travelSpeed / 2)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, a, t);
            //transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
            //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator); // this will be backwards
            //d.y = 0f;
            //transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        UnitPathfinding.IncreaseVisibility(enRouteCell, visionRange);

        IsEnRoute = false;
    }

    public void LookAt(HexDirection direction)
    {
        // HACK: yea dis is weird, coroutine needs to probably be stopped first
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

    /********** MARK: Save/Load Functions **********/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((byte)MyTeam.TeamIndex);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Unit unit = Instantiate(prefab);
        if (header >= 4) unit.MyTeam.TeamIndex = reader.ReadByte();

        unit.MyCell = HexGrid.Singleton.GetCell(coordinates);
        unit.Orientation = orientation;

        HexGrid.Singleton.ParentTransformToGrid(unit.transform);
    }

    #endregion

    /********** MARK: Handle Functions **********/
    #region Handle Functions

    private void HandleOnStartRound()
    {
        Path.Clear();
        CurrentMovement = maxMovement; 
    }

    private void HandleOnStartTurn()
    {
        Path.Clear();
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


