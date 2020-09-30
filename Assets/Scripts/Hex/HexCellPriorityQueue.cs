/**
 * File Name: HexCellPriorityQueue.cs
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
 *      TODO: write all method descriptions for HexCellPriorityQueue.cs
 **/

using System.Collections.Generic;

/// <summary>
/// TODO: class description
/// </summary>
public class HexCellPriorityQueue
{
	/********** MARK: Variables **********/
	#region Variables

	List<HexCell> list = new List<HexCell>();

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
		while (priority >= list.Count) list.Add(null);

		// this creates a linked list of cells; the structure of filling the list with empty cells
		// and adding a linked list to existing indices looks like this:
		// http://bit.ly/HexPriorityQueue
		cell.NextWithSamePriority = list[priority];

        // potentially update the minimum
		if (priority < minimum) minimum = priority;

		list[priority] = cell;
	}

	public HexCell Dequeue()
	{
		count -= 1;

        // find the first cell that isn't null in the list and return
		while (minimum < list.Count)
		{
			HexCell cell = list[minimum];
			if (cell != null)
			{
				list[minimum] = cell.NextWithSamePriority; // decrement the link list at this index
				return cell;
			}
			minimum++; // increment the new minimum and find the new lowest priority cell
		}

		return null; // list is empty
	}

	public void Change(HexCell cell, int oldPriority)
	{
		HexCell current = list[oldPriority];
		HexCell next = current.NextWithSamePriority; // this could be null

        // fix list after removing cell logic
		if (current == cell)
		{
			list[oldPriority] = next; // decrement the link list at this index
		}
		else
		{
            // keep searching linked list until 'next' is the cell
			while (next != cell)
			{
				current = next;
				next = current.NextWithSamePriority;
			}

            // 'next' is the cell, so we can remove/pop 'next' and set 'current.Next...' to
            // 'next.Next...'
			current.NextWithSamePriority = cell.NextWithSamePriority; 
		}

        // we've updated the list after removing the cell, now let's readd the cell...
		Enqueue(cell);
		count -= 1; // ...while keeping the count the same
	}

	public void Clear()
	{
		list.Clear();
		count = 0;
		minimum = int.MaxValue;
	}

    #endregion
}