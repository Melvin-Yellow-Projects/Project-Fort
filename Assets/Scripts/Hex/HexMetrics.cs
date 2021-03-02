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
 *
 *      TODO: convert this into a ScriptableObject
 *      TODO: remove private and protected headers from functions that do not need it
 *      TODO: remove private and protected headers from variables that do not need it
 *      TODO: comment shaders
 **/

using UnityEngine;

/// <summary>
/// Static class to track/calculate hex metrics 
/// </summary>
public static class HexMetrics
{
    /************************************************************/
    #region Metric Variables

    public static HexConfig Configuration { get; set; }
    
    #endregion
    /************************************************************/
    #region Class Functions

    /// <summary>
    /// Gets the first corner of a given direction (corner that is to the left from the center)
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the location of a Hex corner</returns>
    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return Configuration.Corners[(int)direction];
    }

    /// <summary>
    /// Gets the second corner of a given direction (corner that is to the right from the center)
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the location of a Hex corner</returns>
    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return Configuration.Corners[(int)direction + 1];
    }

    /// <summary>
    /// Gets the first solid, unaltered corner of a given direction (corner that is to the left from 
    /// the center and unaltered by neighboring cells)
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the location of a solid Hex corner</returns>
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return Configuration.Corners[(int)direction] * Configuration.SolidFactor;
    }

    /// <summary>
    /// Gets the second solid, unaltered corner of a given direction (corner that is to the right 
    /// from the center and unaltered by neighboring cells)
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the location of a solid Hex corner</returns>
    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return Configuration.Corners[(int)direction + 1] * Configuration.SolidFactor;
    }

    /// <summary>
    /// Gets the midpoint between the two vertices of a HexDirection, which is multiplied such
    /// that it extends to the neighboring HexCell's solid Hex; a diagram can be viewed here between
    /// v1 and v2:
    /// https://catlikecoding.com/unity/tutorials/hex-map/part-2/blend-regions/edge-bridge.png
    /// </summary>
    /// <param name="direction">given HexDirection</param>
    /// <returns>the bridge point</returns>
    public static Vector3 GetBridge(HexDirection direction)
    {
        // multiplying by blendFactor causes overlap (opposed to just averaging the vectors) this is
        // done to reduce triangulation
        return (Configuration.Corners[(int)direction] +
            Configuration.Corners[(int)direction + 1]) * Configuration.BlendFactor;
    }

    /// <summary>
    /// Linear interpolation between terrace point 'a' to terrace point 'b'
    /// </summary>
    /// <param name="a">start point for lerp</param>
    /// <param name="b">end point for lerp</param>
    /// <param name="step">which terrace interval to lerp parameter 'a' to</param>
    /// <returns>a vector point somewhere along the terrace</returns>
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        // horizontal lerp
        float h = step * Configuration.HorizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        // vertical lerp 
        float v = ((step + 1) / 2) * Configuration.VerticalTerraceStepSize;
        a.y += (b.y - a.y) * v;

        return a;
    }

    /// <summary>
    /// Linear interpolation between terrace color 'a' to terrace color 'b'
    /// </summary>
    /// <param name="a">start color for lerp</param>
    /// <param name="b">end color for lerp</param>
    /// <param name="step">which terrace interval to lerp parameter 'a' to</param>
    /// <returns>a color somewhere along the terrace</returns>
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * Configuration.HorizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    /// <summary>
    /// Gets the HexEdgeType (Flat, Slope, or Cliff) from the elevation delta (difference)
    /// </summary>
    /// <param name="elevation1">HexCell elevation 1</param>
    /// <param name="elevation2">HexCell elevation 2</param>
    /// <returns>a HexEdgeType</returns>
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        // if neither slope or flat
        HexEdgeType edgeType = HexEdgeType.Cliff;

        // check if slope
        int delta = Mathf.Abs(elevation2 - elevation1);
        if (delta < HexMetrics.Configuration.CliffDelta) edgeType = HexEdgeType.Slope;

        // check if flat
        if (elevation1 == elevation2) edgeType = HexEdgeType.Flat;

        // return slope
        return edgeType;
    }

    /// <summary>
    /// Perturbs a cell's vertex position; a bool flag can be set to perturb only the height as
    /// opposed to a cell's XZ
    /// </summary>
    /// <param name="position">vertex to perturb</param>
    /// <param name="perturbElevation">whether to perturb Y or XZ</param>
    /// <returns>a perturbed vertex</returns>
    public static Vector3 Perturb(Vector3 position, bool perturbElevation = false)
    {
        // Samples the Perlin noise source for randomness using a given world position, yields a
        // random value between 0 and 1
        Vector4 sample = Configuration.NoiseSource.GetPixelBilinear(
            position.x * Configuration.NoiseScale, position.z * Configuration.NoiseScale
        );

        // convert the sample to a value between -1 and 1, then multiply it by it corresponding
        // noise strength
        if (perturbElevation)
        {
            position.y += (sample.y * 2f - 1f) * Configuration.ElevationPerturbStrength;
        }
        else
        {
            position.x += (sample.x * 2f - 1f) * Configuration.CellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * Configuration.CellPerturbStrength;
        }

        // return position
        return position;
    }

    /// <summary>
    /// Gets the direction between two cells; assumes they are neighbors
    /// HACK can this be done better such that an error is null?
    /// </summary>
    /// <param name="startCell">starting cell</param>
    /// <param name="endCell">end cell</param>
    /// <returns>direction from the starting cell to the ending cell</returns>
    public static HexDirection GetDirection(HexCell startCell, HexCell endCell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell cell = startCell.GetNeighbor(d);
            if (cell && cell.Index == endCell.Index) return d;
        }
        if (startCell.Index == endCell.Index) Debug.Log("no direction, cells are the same");
        else Debug.LogError("Direction Not Found");
        return HexDirection.E;
    }

    #endregion
    /************************************************************/
    #region Archived Functions

    /**

    // HACK: idk man this is probably not what i want
    public static Vector3 GetBridge2(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dir1"></param>
    /// <param name="dir2"></param>
    /// <returns></returns>
    public static bool IsFlank(HexDirection dir1, HexDirection dir2)
    {
        return ((int)dir1 / 3) != ((int)dir2 / 3);
    }

    /// <summary>
    /// Converts an angle to a hex direction; HACK: this does not cover angles outside the range of
    /// 360 degrees
    /// </summary>
    /// <param name="angle">angle to convert</param>
    /// <returns>a hex direction</returns>
    public static HexDirection AngleToDirection(float angle)
    {
        return (HexDirection)(Mathf.RoundToInt((Mathf.Abs(angle) - 30f) / 60f));
    }

    /// <summary>
    /// Gets the hex direction given a point inside of the hex cell; essentially this returns which
    /// triangle the point is in; for a diagram see the link: http://bit.ly/HexRelativeDirection
    /// </summary>
    /// <param name="localPoint">a point inside of a hex cell</param>
    /// <returns>relative hex direction that is in the general vicinity of the point</returns>
    public static HexDirection GetRelativeDirection(Vector3 localPoint)
    {
        // check if point is east or west given the x coordinate
        if (localPoint.x > corners[0].x)
        {
            // checks if point is above or below the Northeast region and then East region
            if (Vector3.Cross(localPoint, corners[1]).y > 0)
            {
                return HexDirection.NE;
            }
            else
            {
                if (Vector3.Cross(localPoint, corners[2]).y > 0) return HexDirection.E;
                else return HexDirection.SE;
            }
        }
        else
        {
            // checks if point is above or below the Northwest region and then West region
            if (Vector3.Cross(localPoint, corners[5]).y < 0)
            {
                return HexDirection.NW;
            }
            else
            {
                if (Vector3.Cross(localPoint, corners[4]).y < 0) return HexDirection.W;
                else return HexDirection.SW;
            }
        }
    }

    **/

    #endregion
}
