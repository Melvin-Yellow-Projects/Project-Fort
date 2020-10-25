/**
 * File Name: HexUnit.cs
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

/// <summary>
/// a unit that is able to interact with a hex map 
/// </summary>
public class HexUnit : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    // HACK: this needs to be configurable
    const float travelSpeed = 4f; 
    const float rotationSpeed = 180f;
    const int visionRange = 3;
    const int movesPerStep = 1;

    public static HexUnit prefab;

    HexCell myCell; 
    HexCell currentTravelCell; // HACK: i really don't like this name

    float orientation;

    HexPath path;

    float t = 0; // interpolator

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
                myCell.Unit = null;
            }

            // update for new location
            myCell = value;
            value.Unit = this; // sets this hex cell's unit to this one
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
    
    public bool HasPath
    {
        get
        {
            return (path != null);
        }
    }

    public HexPath Path
    {
        get
        {
            return path;
        }
        set
        {
            if (HasPath) path.Clear();
            path = value;
        }
    }

    #endregion

    /********** MARK: Pathing Functions **********/
    #region Pathing Functions

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool IsValidDestination(HexCell cell)
    {
        bool isValid = true;

        isValid &= !cell.Unit; // cell does not already have a unit

        //isValid &= !cell.IsUnderwater; // cell is not underwater

        return isValid;
    }
    
    public void Travel()
    {
        if (!HasPath) return; // TODO: maybe continue to change dir

        myCell.Unit = null;
        myCell = path.Cells[path.Cells.Count - 1]; // HACK: this line will not work in the future
        myCell.Unit = this;
        StopAllCoroutines();
        StartCoroutine(TravelPath(path));
        // TODO: clear path after travel
    }

    public void TravelStep(int numberOfSteps)
    {
        if (!HasPath) return;

        for (int i = 0; i < path.NumberOfPathActions && i < numberOfSteps; i++)
        {
            path.PathActions[i].LogPathAction();

            HexPathAction action = path.PathActions[i];

            if (action.ActionType == HexActionType.Move)
            {
                Vector3 a = action.StartCell.Position;
                Vector3 b = action.EndCell.Position;
                Vector3 c = b;

                StartCoroutine(Route(a, b, c));
            }
            else
            {
                LookAt(action.EndDirection);
            }

            path.RemovePathAction();
        }

        // for i < step

        // perform 1 action step from path
        // build route list

        // using route list, move unit through the route

        // update my myCell, orientation, position

        // did we complete the path?
        // if so clear HasPath

    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    IEnumerator TravelPath(HexPath travelPath)
    {
        Vector3 a, b, c = travelPath.Cells[0].Position;

        // perform lookat
        yield return LookAt(travelPath.Cells[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        HexPathfinding.DecreaseVisibility(travelPath.Cells[0], visionRange);

        // initialize the interpolator
        t = Time.deltaTime * travelSpeed; 

        for (int i = 1; i < travelPath.NumberOfCells; i++)
        {
            currentTravelCell = travelPath.Cells[i]; // prevents teleportation

            a = c;
            b = travelPath.Cells[i - 1].Position;
            c = (b + currentTravelCell.Position) * 0.5f;

            HexPathfinding.IncreaseVisibility(travelPath.Cells[i], visionRange);

            yield return Route(a, b, c);

            HexPathfinding.DecreaseVisibility(travelPath.Cells[i], visionRange);
        }
        currentTravelCell = null;

        // arriving at the center if the last cell
        a = c;
        b = myCell.Position; // We can simply use the destination here.
        c = b;

        HexPathfinding.IncreaseVisibility(myCell, visionRange);

        yield return Route(a, b, c);

        transform.localPosition = myCell.Position;
        orientation = transform.localRotation.eulerAngles.y;

        // we're done with the path so it can be dispatched
        Path = null;
    }

    private IEnumerator Route(Vector3 a, Vector3 b, Vector3 c)
    {
        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }
        t -= 1f;
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

        myCell.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        grid.AddUnit(Instantiate(prefab), grid.GetCell(coordinates), orientation);
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


