/**
 * File Name: HexCellPriorityQueue.cs
 * Description: The script contains a Priority Queue data structure that is specifically tailored to
 *              the HexCell object; useful for calculating distances and paths among HexCells
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 29, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      UNDONE: this can be templated if a priority queue item class is made
 **/

using System.Collections.Generic;

/// <summary>
/// A Priority Queue data structure that is specific to Hex Cells
/// </summary>
public class HexCellPriorityQueue
{
	/********** MARK: Variables **********/
	#region Variables

    /// <summary>
    /// core data structure variable
    /// </summary>
	List<HexCell> priorityQueue = new List<HexCell>();

    /// <summary>
    /// current number of cell elements in the priority queue
    /// </summary>
	private int count = 0;

	/// <summary>
	/// value to keep track of the minimum cell priority
	/// </summary>
	private int minimum = int.MaxValue;

	#endregion

	/********** MARK: Properties **********/
	#region Properties

    /// <summary>
    /// Gets the current number of HexCells within the Priority Queue
    /// </summary>
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

    /// <summary>
    /// Adds a cell to the priority queue
    /// </summary>
    /// <param name="cell">cell to add</param>
	public void Enqueue(HexCell cell)
	{
		count += 1;
		int priority = cell.SearchPriority;

		// add null elements into the list until the count matches the cell's priority
		while (priority >= priorityQueue.Count) priorityQueue.Add(null);

		// this creates a linked list of cells; the structure of filling the list with empty cells
		// and adding a linked list to existing indices looks like this:
		// http://bit.ly/HexPriorityQueue
		cell.NextWithSamePriority = priorityQueue[priority];

		// potentially update the minimum
		if (priority < minimum) minimum = priority;

        // sets the cell to the front of the priority queue
		priorityQueue[priority] = cell;
	}

    /// <summary>
    /// Removes the next cell in the priority queue
    /// </summary>
    /// <returns>the removed cell</returns>
	public HexCell Dequeue()
	{
		count -= 1;

		// find the first cell that isn't null in the list and return
		while (minimum < priorityQueue.Count)
		{
			HexCell cell = priorityQueue[minimum];
			if (cell != null)
			{
				// decrement the list at this index by setting the next cell to the front of list
				priorityQueue[minimum] = cell.NextWithSamePriority; 
				return cell;
			}
			minimum++; // increment the new minimum and find the new lowest priority cell
		}

		return null; // list is empty
	}

    /// <summary>
    /// Updates an existing cell in the queue to its new value given its old value
    /// </summary>
    /// <param name="cell">the cell to update</param>
    /// <param name="oldPriority">the cell's old priority value</param>
	public void Change(HexCell cell, int oldPriority)
	{
        HexCell current = priorityQueue[oldPriority];
        HexCell next = current.NextWithSamePriority; // this could be null 

        // fix list after removing cell logic
        if (current == cell)
        {
            priorityQueue[oldPriority] = next; // decrement the link list at this index
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

    /// <summary>
    /// Completely clears the priority queue and resets its values
    /// </summary>
	public void Clear()
	{
		priorityQueue.Clear();
		count = 0;
		minimum = int.MaxValue;
	}

	#endregion
}
