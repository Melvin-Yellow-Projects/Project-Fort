/**
 * File Name: HexMetrics.cs
 * Description: Manual configuration file/script for hex metrics and calculations
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

/// <summary>
/// Static class to track/calculate hex metrics 
/// </summary>
public static class HexMetrics
{
    /********** MARK: Metric Variables **********/
    #region Metric Variables

    /// <summary>
    /// percent of a HexCell that is solid and unaltered by its neighbors
    /// </summary>
    public const float solidFactor = 0.75f;

    /// <summary>
    /// height of each successive elevation change; UNDONE: For an actual game I'd probably use
    ///         a smaller step size. 
    /// </summary>
    public const float elevationStep = 5f;

    // TODO Comment terrace var
    public const int terracesPerSlope = 2;

    // TODO Comment HexEdgeType cliff/slope var
    public const int cliffDelta = 1;

    #endregion

    /********** MARK: Metric Constants **********/
    #region Metric Constants

    /// <summary>
    /// conversion from a hex's outer radius to its inner radius
    /// </summary>
    public const float outerToInner = 0.866025404f;

    /// <summary>
    /// conversion from a hex's inner radius to its outer radius
    /// </summary>
    public const float innerToOuter = 1f / outerToInner;

    /// <summary>
    /// a hex's outer radius
    /// </summary>
    public const float outerRadius = 10f;

    /// <summary>
    /// a hex's inner radius
    /// </summary>
    public const float innerRadius = outerRadius * outerToInner;

    /// <summary>
    /// a hex's 6 corners; has a redundant first corner to handle out of bounds error
    /// </summary>
    static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    /// <summary>
    /// percent of a HexCell that is blended and altered by its neighbors; calculated directly from
    /// solidFactor
    /// </summary>
    public const float blendFactor = 1f - solidFactor;

    // TODO: Comment terrace constants
    public const int terraceSteps = terracesPerSlope * 2 + 1;

    public const float horizontalTerraceStepSize = 1f / terraceSteps;

    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Gets the first corner of a given direction (corner that is to the left from the center)
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the location of a Hex corner</returns>
    public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}

	/// <summary>
	/// Gets the second corner of a given direction (corner that is to the right from the center)
	/// </summary>
	/// <param name="direction">given HexDirection</param>
	/// <returns>the location of a Hex corner</returns>
	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}

	/// <summary>
	/// Gets the first solid, unaltered corner of a given direction (corner that is to the left from 
    /// the center and unaltered by neighboring cells)
	/// </summary>
	/// <param name="direction">given HexDirection</param>
	/// <returns>the location of a solid Hex corner</returns>
	public static Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return corners[(int)direction] * solidFactor;
	}

	/// <summary>
	/// Gets the second solid, unaltered corner of a given direction (corner that is to the right 
    /// from the center and unaltered by neighboring cells)
	/// </summary>
	/// <param name="direction">given HexDirection</param>
	/// <returns>the location of a solid Hex corner</returns>
	public static Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return corners[(int)direction + 1] * solidFactor;
	}

	/// <summary>
	/// Returns the midpoint between the two vertices of a HexDirection, which is multiplied such that
    /// it extends to the neighboring HexCell's solid Hex; a diagram can be viewed here between v1 and 
    /// v2: https://catlikecoding.com/unity/tutorials/hex-map/part-2/blend-regions/edge-bridge.png
	/// </summary>
	/// <param name="direction">given HexDirection</param>
	/// <returns>the bridge point</returns>
	public static Vector3 GetBridge(HexDirection direction)
	{
		// multiplying by blendFactor causes overlap (opposed to just averaging the vectors) this is
        //   done to reduce triangulation
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}

    // TODO: Comment Function TerraceLerp
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        // horizontal lerp
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        // vertical lerp 
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;

        return a;
    }

    // TODO: Comment Function TerraceLerp
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    // TODO: Comment Function GetEdgeType
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        // if neither slope or flat
        HexEdgeType edgeType = HexEdgeType.Cliff;

        // check if slope
        int delta = Mathf.Abs(elevation2 - elevation1);
        if (delta <= HexMetrics.cliffDelta) edgeType = HexEdgeType.Slope;

        // check if flat
        if (elevation1 == elevation2) edgeType = HexEdgeType.Flat;
        
        // return slope
        return edgeType;
    }

    #endregion
}
