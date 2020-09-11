/**
 * File Name: HexMetrics.cs
 * Description: Class to serve as a configuration file for the hex map calculations
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

public static class HexMetrics
{
	/********** MARK: Variables **********/
	#region Variables

    // conversion from a hex's outer radius to its inner radius
	public const float outerToInner = 0.866025404f;

	// conversion from a hex's inner radius to its outer radius
	public const float innerToOuter = 1f / outerToInner;

    // a hex's outer radius
	public const float outerRadius = 10f;

    // a hex's inner radius
	public const float innerRadius = outerRadius * outerToInner;

    // a hex's 6 corners; has a redundant first corner to handle out of bounds error
	public static Vector3[] corners = {
		new Vector3(-0.5f * outerRadius, 0f, innerRadius),
		new Vector3(0.5f * outerRadius, 0f, innerRadius),
		new Vector3(outerRadius, 0f, 0f),
		new Vector3(0.5f * outerRadius, 0f, -innerRadius),
		new Vector3(-0.5f * outerRadius, 0f, -innerRadius),
		new Vector3(-outerRadius, 0f, 0f),
		new Vector3(-0.5f * outerRadius, 0f, innerRadius)
	};

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

    // TODO: write class functions

	public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}

    #endregion
}
