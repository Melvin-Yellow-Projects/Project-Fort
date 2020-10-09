/**
 * File Name: ListPool.cs
 * Description: This script is for a static stack of lists. The datatype can be reused across
 *                  multiple chunks for the Hex map
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 24, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: I honestly don't get this file
 **/

using System.Collections.Generic;

/// <summary>
/// Static reusable stack of lists
/// </summary>
/// <typeparam name="T"></typeparam>
public static class ListPool<T>
{
	/********** MARK: Variables **********/
	#region Variables

	static Stack<List<T>> stack = new Stack<List<T>>();

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Pops a list from the stack
    /// </summary>
    /// <returns></returns>
    public static List<T> Get()
	{
		if (stack.Count > 0)
		{
			return stack.Pop();
		}
		return new List<T>();
	}

    /// <summary>
    /// Adds a new list to the stack
    /// </summary>
    /// <param name="list"></param>
	public static void Add(List<T> list)
	{
		list.Clear();
		stack.Push(list);
	}

	#endregion
}