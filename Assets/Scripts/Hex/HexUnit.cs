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

    public static HexUnit unitPrefab;

    HexCell location; // HACK: i really don't like this name
    HexCell currentTravelLocation;

    float orientation;

    List<HexCell> pathToTravel;


    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            // if there is a previous location...
            if (location) 
            {
                HexPathfinding.DecreaseVisibility(location, visionRange);
                location.Unit = null;
            }

            // update for new location
            location = value;
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
            return 4;
        }
    }

    public int VisionRange
    {
        get
        {
            return 3;
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    void OnEnable()
    {
        if (location) // prevents failure during recompile
        {
            transform.localPosition = location.Position;
            if (currentTravelLocation)
            {
                HexPathfinding.IncreaseVisibility(location, visionRange);
                HexPathfinding.DecreaseVisibility(currentTravelLocation, visionRange);
                currentTravelLocation = null;
            }
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

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

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Travel(List<HexCell> path)
    {
        location.Unit = null;
        location = path[path.Count - 1];
        location.Unit = this;
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    IEnumerator TravelPath()
    {
        Vector3 a, b, c = pathToTravel[0].Position;

        // perform lookat
        yield return LookAt(pathToTravel[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        HexPathfinding.DecreaseVisibility(
            currentTravelLocation ? currentTravelLocation : pathToTravel[0],
            visionRange
        );

        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            currentTravelLocation = pathToTravel[i]; // prevents teleportation

            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + currentTravelLocation.Position) * 0.5f;

            HexPathfinding.IncreaseVisibility(pathToTravel[i], visionRange);

            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            HexPathfinding.DecreaseVisibility(pathToTravel[i], visionRange);

            t -= 1f;
        }
        currentTravelLocation = null;

        // arriving at the center if the last cell
        a = c;
        b = location.Position; // We can simply use the destination here.
        c = b;

        HexPathfinding.IncreaseVisibility(location, visionRange);

        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        transform.localPosition = location.Position;
        orientation = transform.localRotation.eulerAngles.y;

        // we're done with the list so it can be dispatched
        ListPool<HexCell>.Add(pathToTravel);
        pathToTravel = null;
    }

    public void LookAt(HexDirection direction)
    {
        // HACK: yea dis is hacky, coroutine needs to probably be stopped first
        Vector3 localPoint = HexMetrics.GetBridge(direction);
        StartCoroutine(LookAt(location.Position + localPoint));
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

    public void Die()
    {
        if (location) HexPathfinding.DecreaseVisibility(location, visionRange);

        location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
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


