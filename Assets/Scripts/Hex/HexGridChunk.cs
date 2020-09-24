using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	HexCell[] cells;

	HexMesh hexMesh;
	Canvas gridCanvas;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
	}

	/// <summary>
	/// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
	/// Update functions have been called
	/// </summary>
	void LateUpdate()
	{
		hexMesh.Triangulate(cells);
		enabled = false;
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public void AddCell(int index, HexCell cell)
	{
		cells[index] = cell;
		cell.chunk = this;
		cell.transform.SetParent(transform, false);
		cell.uiRectTransform.SetParent(gridCanvas.transform, false);
	}

	public void Refresh()
	{
		enabled = true;
	}

	#endregion

}