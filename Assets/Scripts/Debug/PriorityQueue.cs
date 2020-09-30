/**
 * File Name: PriorityQueue.cs
 * Description: TODO: script description
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 29, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: write all method descriptions for PriorityQueue.cs
 **/

using System.Collections.Generic;

/// <summary>
/// TODO: class description
/// </summary>
public class PriorityQueue
{
	/********** MARK: Variables **********/
	#region Variables

    /// <summary>
    /// priority queue data structure
    /// </summary>
	List<List<HexCell>> ds = new List<List<HexCell>>();

	/// <summary>
	/// current number of cells in the data structure
	/// </summary>
	int count = 0;

    /// <summary>
    /// value to keep track of the minimum cell priority
    /// </summary>
	int minimum = int.MaxValue;

	#endregion

	/********** MARK: Properties **********/
	#region Properties

	public int Count
	{
		get
		{
			return count;
		}
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public void Enqueue(HexCell cell)
	{
		count += 1;
		int priority = cell.SearchPriority;

        // add null elements into the list until the count matches the cell's priority
		while (priority >= ds.Count) ds.Add(null); // BUG: this might need to be a new List<HexCell>

		// this creates a linked list of cells; the structure of filling the list with empty cells
		// and adding a linked list to existing indices looks like this:
		// http://bit.ly/HexPriorityQueue

		if (ds[priority] == null) ds[priority] = new List<HexCell>();

		ds[priority].Add(cell); // adds cell to the back of the linked list

        // potentially update the minimum
		if (priority < minimum) minimum = priority;
	}

	public HexCell Dequeue()
	{
		count -= 1;

        // find the first cell that isn't null in the list and return
		while (minimum < ds.Count)
		{
            if (ds[minimum] != null) // a list is present, a cell must exist
            {
				int index = ds[minimum].Count - 1; // index for the last cell in the list
				HexCell cell = ds[minimum][index]; // gets the last cell in the list

				// decrement the list at this index
				ItemRemoval(minimum, index);

				return cell;
			}
			minimum++; // increment the new minimum and find the new lowest priority cell
		}

		return null; // list is empty
	}

	public void Change(HexCell cell, int oldPriority)
	{
		int index = ds[oldPriority].Count; // get last element in list
		HexCell current = ds[oldPriority][--index]; // this should never be null

		while (current != cell) current = ds[oldPriority][--index];

		ItemRemoval(oldPriority, index);

        // we've updated the list after removing the cell, now let's readd the cell...
		Enqueue(cell);
		count -= 1; // ...while keeping the count the same
	}

	public void Clear()
	{
		ds.Clear();
		count = 0;
		minimum = int.MaxValue;
	}

    private void ItemRemoval(int listIndex, int itemIndex)
    {
		ds[listIndex].RemoveAt(itemIndex); // removes the last cell
		if (ds[listIndex].Count == 0) ds[listIndex] = null; // if list is empty, set to null
	}

    #endregion
}