using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	public int width = 6;
	public int height = 6;

	public HexCell cellPrefab;

	HexCell[] cells;

	public Text cellLabelPrefab;

	Canvas gridCanvas;

	HexMesh hexMesh;

	public Color defaultColor = Color.white;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[height * width];

		for (int x = 0, i = 0; x < width; x++)
		{
			for (int z = 0; z < height; z++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	void Start()
	{
		hexMesh.Triangulate(cells);
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	void CreateCell(int x, int z, int i)
	{
        // initialize cell position
		Vector3 position;
		position.x = x;
		position.y = 0f;
		position.z = z;

		// calculate x position
		position.x *= (1.5f * HexMetrics.outerRadius);

		// calculate z position
		position.z += (x / 2f); // offset z by half of x (verticle shift)
		position.z -= (x / 2); // undo offset with integer math (this will effect odd rows)
		position.z *= (2f * HexMetrics.innerRadius); // multiply by correct z hex metric

		// instantiate cell
		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab, transform);
        cell.transform.localPosition = position;

        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

		//label.text = x.ToString() + "\n" + z.ToString(); // offset coordinates
        label.text = cell.coordinates.ToStringOnSeparateLines(); // cube coordinates

		cell.color = defaultColor;
	}

	public void ColorCell(Vector3 position, Color color)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        // get index from coordinates
		int index = coordinates.Z + (coordinates.X * height) + (coordinates.X / 2);

		// change cell color using cell index
		HexCell cell = cells[index];
		cell.color = color;

        // retriangulate
		hexMesh.Triangulate(cells);

	}

	#endregion
}
