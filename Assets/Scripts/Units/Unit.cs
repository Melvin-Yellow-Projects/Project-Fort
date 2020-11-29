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
public class Unit : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    // HACK: this needs to be configurable
    const float travelSpeed = 4f; 
    const float rotationSpeed = 180f;
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
    /// <subscriber name="HandleOnUnitSpawned">Player Class</subscriber>
    public static event Action<Unit> OnUnitSpawned;

    /// <summary>
    /// Event for when a unit is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber name="HandleOnUnitDepawned">Player Class</subscriber>
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
                HexPathfinding.DecreaseVisibility(myCell, visionRange);
                myCell.MyUnit = null;
            }

            // update for new location
            myCell = value;
            value.MyUnit = this; // sets this hex cell's unit to this one
            HexPathfinding.IncreaseVisibility(value, visionRange);
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

    public int Speed
    {
        get
        {
            return (int)travelSpeed;
        }
    }

    public int VisionRange
    {
        get
        {
            return visionRange;
        }
    }

    public HexPath Path { get; private set; }

    //public bool HasRealPath { get; set; }

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

    public bool IsAttacking { get; private set; }

    public int Team
    {
        get
        {
            return team;
        }

        set
        {
            team = value;
            if (team == 0) GetComponentInChildren<Renderer>().material.color = Color.blue;
            else GetComponentInChildren<Renderer>().material.color = Color.red;
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
                HexPathfinding.IncreaseVisibility(myCell, visionRange);
                HexPathfinding.DecreaseVisibility(currentTravelCell, visionRange);
                currentTravelCell = null;
            }
        }
    }

    private void Awake()
    {
        Path = new HexPath(this);
    }

    private void Start()
    {
        OnUnitSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnUnitDepawned?.Invoke(this);
    }

    #endregion

    /********** MARK: Pathing Functions **********/
    #region Pathing Functions

    //public bool IsValidDestination(HexCell cell)
    //{
    //    bool isValid = true;

    //    isValid &= !cell.Unit; // cell does not already have a unit

    //    //isValid &= !cell.IsUnderwater; // cell is not underwater

    //    return isValid;
    //}

    public void Move(int numberOfSteps)
    {
        if (!Path.HasPath) return; // TODO: maybe continue to change dir

        List<HexCell> cells = new List<HexCell>();

        cells.Add(Path[0]);
        for (int i = 1; i < Path.Length && i <= numberOfSteps; i++)
        {
            cells.Add(Path[i]);
        }

        Route(cells);
    }

    private void Route(List<HexCell> cells)
    {
        if (cells[0] != myCell) Debug.LogError("This line should never execute"); // HACK: remove line

        // does new cell have a unit?
        Unit unit = cells[cells.Count - 1].MyUnit;
        if (unit != null)
        {
            if (unit.team == team) return; // cannot move onto friendly cell
            unit.GetComponent<Death>().Die();
        }

        myCell.MyUnit = null;
        myCell = cells[cells.Count - 1]; 
        myCell.MyUnit = this;
        StopAllCoroutines();
        StartCoroutine(Route(cells, false));

        // remove path cells
        Path.RemoveTailCells(numberToRemove: (cells.Count - 1));

        // redraw path curser
        Path.Show();

        // TODO: clear path after travel
    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    private IEnumerator Route(List<HexCell> cells, bool dummy)
    {
        Vector3 a, b, c = cells[0].Position;

        List<int> l = new List<int>();

        // perform lookat
        yield return LookAt(cells[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        HexPathfinding.DecreaseVisibility(
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

            HexPathfinding.IncreaseVisibility(cells[i], visionRange);

            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            HexPathfinding.DecreaseVisibility(cells[i], visionRange);

            t -= 1f;
        }
        currentTravelCell = null;

        // arriving at the center if the last cell
        a = c;
        b = myCell.Position; // We can simply use the destination here.
        c = b;

        HexPathfinding.IncreaseVisibility(myCell, visionRange);

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

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions
        
    public void ValidateLocation()
    {
        transform.localPosition = myCell.Position;
    }
    
    public void Die()
    {
        if (myCell) HexPathfinding.DecreaseVisibility(myCell, visionRange);

        myCell.MyUnit = null;
        Destroy(gameObject);
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
        if (header >= 4) unit.Team = reader.ReadByte();

        grid.AddUnit(unit, grid.GetCell(coordinates), orientation);
    }

    #endregion

    /********** MARK: Pathing **********/
    #region Pathing

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
        if (neighbor.MyUnit && neighbor.MyUnit.Team == Team) return false; // TODO: check unit type

        // invalid if cell is unexplored
        if (!neighbor.IsExplored) return false;

        // neighbor is a valid cell
        return true;
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


