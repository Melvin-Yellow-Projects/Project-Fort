/**
 * File Name: HexEdgeVertices.cs
 * Description: Struct that divides a hex triangle's edge into multiple points
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 24, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: specify the number of vertices per edge, this should probably be done in HexMetrics
 **/

using UnityEngine;

/// <summary>
/// Struct that creates and tracks additional vertices along a hex triangle edge
/// </summary>
public struct HexEdgeVertices
{
	/********** MARK: Variables **********/
	#region Variables

	public Vector3 v1, v2, v3, v4;

    #endregion

    /// <summary>
    /// Creates an EdgeVertices struct using the two outside corners of a hex triangle 
    /// </summary>
    /// <param name="corner1">left outer hex corner</param>
    /// <param name="corner2">right outer hex corner</param>
    public HexEdgeVertices(Vector3 corner1, Vector3 corner2)
	{
		v1 = corner1;
		v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
		v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
		v4 = corner2;
	}

    /// <summary>
    /// Creates and finds the intermediate vertices along a terrace given a interpolation step count
    /// </summary>
    /// <param name="a">first hex edge vertices</param>
    /// <param name="b">second hex edge vertices</param>
    /// <param name="step">terrace step to interpolate the vertices to</param>
    /// <returns>a new EdgeVertices struct</returns>
    public static HexEdgeVertices TerraceLerp(HexEdgeVertices a, HexEdgeVertices b, int step)
	{
		HexEdgeVertices result;
		result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
		result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
		result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
		result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
		return result;
	}
}