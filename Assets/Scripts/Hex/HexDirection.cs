/**
 * File Name: HexDirection.cs
 * Description: Hex Directional Enum and Struct data structures for tracking cell neighbors
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 10, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

/************************************************************/
#region HexDirection Enum

/// <summary>
/// Enum for tracking each of the six directions for a HexCell; (Northeast, East, Southeast,
/// Southwest, West, & Northwest)
/// </summary>
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

#endregion
/************************************************************/
#region HexDirection Extensions

public static class HexDirectionExtensions
{
    /// <summary>
    /// Gets the opposite direction of the given enum direction
    /// </summary>
    /// <param name="direction">direction to calculate the opposite of</param>
    /// <returns>the opposite direction</returns>
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    /// <summary>
    /// Gets the previous direction of the given enum direction
    /// </summary>
    /// <param name="direction">direction to calculate the previous of</param>
    /// <returns>the previous enum direction</returns>
    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    /// <summary>
    /// Gets the previous direction of the given enum direction
    /// </summary>
    /// <param name="direction">direction to calculate the previous of</param>
    /// <returns>the next enum direction</returns>
    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}

#endregion