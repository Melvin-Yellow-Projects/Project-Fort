using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexManager : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	public const float outerToInner = 0.866025404f;

	public const float innerToOuter = 1f / outerToInner;

	public const float outerRadius = 10f;

	public const float innerRadius = outerRadius * outerToInner;

	public static Vector3[] corners = {
		new Vector3(-outerRadius, 0f, 0f),
		new Vector3(-0.5f * outerRadius, 0f, innerRadius),
		new Vector3(0.5f * outerRadius, 0f, innerRadius),
		new Vector3(outerRadius, 0f, 0f),
		new Vector3(0.5f * outerRadius, 0f, -innerRadius),
		new Vector3(-0.5f * outerRadius, 0f, -innerRadius),
		new Vector3(-outerRadius, 0f, 0f)
	};

	#endregion
}
