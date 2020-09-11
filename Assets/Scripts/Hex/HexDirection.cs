﻿/**
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

/********** MARK: HexDirection Enum **********/
#region HexDirection Enum

public enum HexDirection
{
	N, NE, SE, S, SW, NW
}

#endregion

/********** MARK: HexDirection Extensions **********/
#region HexDirection Extensions

// TODO: write Extensions functions

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.N ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.N : (direction + 1);
    }

    //public static HexDirection Previous2(this HexDirection direction)
    //{
    //    direction -= 2;
    //    return direction >= HexDirection.NE ? direction : (direction + 6);
    //}

    //public static HexDirection Next2(this HexDirection direction)
    //{
    //    direction += 2;
    //    return direction <= HexDirection.NW ? direction : (direction - 6);
    //}
}

#endregion