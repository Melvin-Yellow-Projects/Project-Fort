// TODO: comment listpool file

using System.Collections.Generic;

public static class ListPool<T>
{
	/********** MARK: Variables **********/
	#region Variables

	static Stack<List<T>> stack = new Stack<List<T>>();

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public static List<T> Get()
	{
		if (stack.Count > 0)
		{
			return stack.Pop();
		}
		return new List<T>();
	}

	public static void Add(List<T> list)
	{
		list.Clear();
		stack.Push(list);
	}

	#endregion
}