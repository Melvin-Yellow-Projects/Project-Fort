/**
 * File Name: HexUnit.cs
 * Description: Script for managing a hex unit
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: remove private and protected headers from functions that do not need it
 *      TODO: remove private and protected headers from variables that do not need it
 **/

using UnityEngine;
using System.IO;

/// <summary>
/// a unit that is able to interact with a hex map 
/// </summary>
public class HexUnit : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    public static HexUnit unitPrefab;

    HexCell location; // HACK: i really don't like this name

    float orientation;

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
            location = value;
            value.Unit = this; // sets this hex cell's unit to this one
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

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
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
}


