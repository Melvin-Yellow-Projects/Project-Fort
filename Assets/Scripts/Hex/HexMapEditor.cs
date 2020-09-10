/**
 * File Name: DebugComments.cs
 * Description: This script is to serve as a placeholder for useful comments
 * 
 * Authors: XXXX [Youtube Channel], Will Lacey
 * Date Created: August 18, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found on XXXX YouTube channel under the video: 
 *      "yyyy"; updated it to better fit project
 * 
 **/

using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;

	void Awake()
	{
		SelectColor(0);
	}

	void Update()
	{
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			// draw line for 1 second
			Debug.DrawLine(inputRay.origin, hit.point, Color.white, 1f);

			// color the cell given the hit position
			hexGrid.ColorCell(hit.point, activeColor);
		}
	}

	public void SelectColor(int index)
	{
		activeColor = colors[index];
	}
}