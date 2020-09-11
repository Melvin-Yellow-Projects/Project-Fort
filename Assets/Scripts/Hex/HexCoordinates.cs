﻿/**
 * File Name: HexCoordinates.cs
 * Description: Struct to handle the coordinates of a hex cell/tile
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
    ///     Gets the x hex (cube) coordinate
    /// </summary>
	public int X
	{
		get
		{
			return x;
		}
	}

	/// <summary>
	///     Gets the y hex (cube) coordinate
	/// </summary>
	public int Y
	{
		get
		{
			return -(X + Z);
		}
	}

	/// <summary>
	///     Gets the z hex (cube) coordinate
	/// </summary>
	public int Z
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
	///     Default constructor for HexCoordinates; takes two values and returns a new HexCoordinate
    ///         struct
	/// </summary>
	/// <param name="x">x hex (cube) coordinate</param>
	/// <param name="z">z hex (cube) coordinate</param>
	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	/// <summary>
	///     Offset coordinate constructor for HexCoordinates; takes offset coordinates and converts
    ///         them to hex (cube)
	/// </summary>
	/// <param name="x">x (row) offset coordinate</param>
	/// <param name="z">z (col) offset coordinate</param>
	/// <returns>a new HexCoordinate struct</returns>
	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		int xCube = x;
		int zCube = z - x / 2; // undo verticle shift/offset

        return new HexCoordinates(xCube, zCube);
	}

    /// <summary>
    ///     Overriden ToString method; displays the HexCoordinate data
    /// </summary>
    /// <returns>string 3-tuple hex coordinates</returns>
	public override string ToString()
	{
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    /// <summary>
    ///     Displays the HexCoordinate data vertically
    /// </summary>
    /// <returns>HexCoordinate in a string data form</returns>
	public string ToStringOnSeparateLines()
	{
		return "x:" + X.ToString() + "\ny:" + Y.ToString() + "\nz:" + Z.ToString();
		//return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

    /// <summary>
    ///     Converts a world position into a HexCoordinate; Essentially divides position by hex
    ///         dimensions; Assumes no map offset
    /// </summary>
    /// <param name="position">world position</param>
    /// <returns>a hex coordinate position</returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        // get z coordinate by dividing by a stacked verticle hex
		float z = position.z / (2f * HexMetrics.innerRadius);
        float y = -z;

        // calculate verticle offset as the position moves horizontally 
        float offset = position.x / (3f * HexMetrics.outerRadius);
        z -= offset;
        y -= offset;

        // round values to suspected coordinates
        int iX = Mathf.RoundToInt(-(y + z));
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(z);

        // possible rounding error? then correct coordinates
        if (iX + iY + iZ != 0)
        {
			// get each rounding delta
			float deltaX = Mathf.Abs(-(y + z) - iX);
			float deltaY = Mathf.Abs(y - iY);
			float deltaZ = Mathf.Abs(z - iZ);

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

    #endregion
}