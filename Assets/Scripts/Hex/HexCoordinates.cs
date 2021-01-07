/**
 * File Name: HexCoordinates.cs
 * Description: A struct script to handle the coordinates of a hex cell/tile
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using System.IO;

/// <summary>
/// Hex (cube) coordinates for a HexCell
/// </summary>
[System.Serializable]
public struct HexCoordinates
{
	/********** MARK: Variables **********/
	#region Variables

	[SerializeField] private int x; 

	[SerializeField] private int z;

	#endregion

	/********** MARK: Properties **********/
	#region Properties

    /// <summary>
    /// Gets the x hex (cube) coordinate
    /// </summary>
	public int X
	{
		get
		{
			return x;
		}
	}

	/// <summary>
	/// Gets the y hex (cube) coordinate
	/// </summary>
	public int Y
	{
		get
		{
			return -(X + Z);
		}
	}

	/// <summary>
	/// Gets the z hex (cube) coordinate
	/// </summary>
	public int Z
	{
		get
		{
			return z;
		}
	}

	/// <summary>
	/// TODO: comment OffsetX
	/// </summary>
	public int OffsetX
    {
        get
        {
			return x + z / 2;
        }
    }

	/// <summary>
	/// TODO: comment OffsetZ
	/// </summary>
	public int OffsetZ
	{
		get
		{
			return z;
		}
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	/// <summary>
	/// Default constructor for HexCoordinates; takes two values and returns a new HexCoordinate
	/// struct
	/// </summary>
	/// <param name="x">x hex (cube) coordinate</param>
	/// <param name="z">z hex (cube) coordinate</param>
	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	/// <summary>
	/// Offset coordinate constructor for HexCoordinates; takes offset coordinates and converts them
	/// to hex (cube)
	/// </summary>
	/// <param name="x">x (row) offset coordinate</param>
	/// <param name="z">z (col) offset coordinate</param>
	/// <returns>a new HexCoordinate struct</returns>
	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		int xCube = x - z / 2; // undo horizontal shift/offset
        int zCube = z;

        return new HexCoordinates(xCube, zCube);
	}

    /// <summary>
    /// Overriden ToString method; displays the HexCoordinate data
    /// </summary>
    /// <returns>string 3-tuple hex coordinates</returns>
	public override string ToString()
	{
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

	/// <summary>
	/// Displays the HexCoordinate data vertically
	/// </summary>
	/// <param name="offset"></param>
	/// <param name="addHeaders"></param>
	/// <returns></returns>
	public string ToStringOnSeparateLines(bool offset = false, bool addHeaders = false)
	{
		string[] headers = {"", "", ""};
        if (addHeaders)
        {
			headers[0] = (offset) ? "c:" : "x:";
			headers[1] = "y:";
			headers[2] = (offset) ? "r:" : "z:";
		}

        if (offset)
        {
			return headers[2] + OffsetZ.ToString() + "\n" + headers[0] + OffsetX.ToString();
		}
        else
        {
			return headers[0] + X.ToString() + "\n" + headers[1] + Y.ToString() +
				"\n" + headers[2] + Z.ToString();
		}
	}

    /// <summary>
    /// Converts a world position into a HexCoordinate; Essentially divides position by hex
    /// dimensions; Assumes no map offset
    /// </summary>
    /// <param name="position">world position</param>
    /// <returns>a hex coordinate position</returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        // get x coordinate by dividing by a stacked horizontal hex
        float x = position.x / (2f * HexMetrics.innerRadius);
        float y = -x;

        // calculate horizontal offset as the position moves vertically 
        float offset = position.z / (3f * HexMetrics.outerRadius);
        x -= offset;
        y -= offset;

        // round values to suspected coordinates
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-(x + y));

        // possible rounding error? then correct coordinates
        if (iX + iY + iZ != 0)
        {
            // get each rounding delta
            float deltaX = Mathf.Abs(x - iX);
            float deltaY = Mathf.Abs(y - iY);
            float deltaZ = Mathf.Abs(-x - y - iZ);

            // find largest delta and reconstruct flawed coordinate 
            if (deltaX > deltaY && deltaX > deltaZ)
			{
				iX = -iY - iZ; // reconstruct X
			}
			else if (deltaZ > deltaY)
			{
				iZ = -iX - iY; // reconstruct Z
			}
		}

        return new HexCoordinates(iX, iZ); // auto reconstruct Y

    }

    /// <summary>
    /// TODO: comment DistanceTo
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
	public int DistanceTo(HexCoordinates other)
	{
		return (Mathf.Abs(x - other.x) + Mathf.Abs(Y - other.Y) + Mathf.Abs(z - other.z)) / 2;
	}

	/// <summary>
	/// TODO: comment HexCoordinates Load
	/// </summary>
	/// <param name="reader"></param>
	/// <returns></returns>
	public static HexCoordinates Load(BinaryReader reader)
	{
		HexCoordinates c;
		c.x = reader.ReadInt32();
		c.z = reader.ReadInt32();
		return c;
	}

	/// <summary>
	/// TODO: comment HexCoordinates Save
	/// Saves/writes the coordinates to the stream
	/// </summary>
	/// <param name="writer"></param>
	public void Save(BinaryWriter writer)
	{
		writer.Write(x);
		writer.Write(z);
	}

	#endregion
}