/**
 * File Name: HexMesh.cs
 * Description: TODO: Write this
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	protected Mesh hexMesh; // mesh

	protected List<Vector3> vertices; // mesh's vertices
	protected List<Color> colors; // mesh's color at a given vertex
	protected List<int> triangles; // mesh's triangle draw order (how to draw the vertices)
    // HACK: idk what triangles actually does

	protected MeshCollider meshCollider;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	/// <summary>
	///     Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	void Awake()
	{
        // initialize mesh
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();

		hexMesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
		colors = new List<Color>();
		triangles = new List<int>();
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

    /// <summary>
    ///     Function to draw all cells belonging to HexMesh
    /// </summary>
    /// <param name="cells">cells to draw mesh for</param>
	public void Triangulate(HexCell[] cells)
	{
        // clear current mesh
		hexMesh.Clear();
		vertices.Clear();
		colors.Clear();
		triangles.Clear();

        // draw every cell
		for (int i = 0; i < cells.Length; i++)
		{
			Triangulate(cells[i]);
		}

        // assign mesh triangulation to the mesh
		hexMesh.vertices = vertices.ToArray();
		hexMesh.colors = colors.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.RecalculateNormals();

        // assign meshCollider to mesh
		meshCollider.sharedMesh = hexMesh;
	}

	/// <summary>
	///     Function to draw the mesh for a given hex cell
	/// </summary>
	/// <param name="cell">HexCell to draw</param>
	void Triangulate(HexCell cell)
	{
		Vector3 center = cell.transform.localPosition;
		for (int i = 0; i < 6; i++)
		{
			Vector3 v1 = center;
			Vector3 v2 = center + HexMetrics.corners[i];
			Vector3 v3 = center + HexMetrics.corners[i + 1];

			AddTriangle(v1, v2, v3);
			// AddTriangle(center, center + HexMetrics.corners[i], center + HexMetrics.corners[i + 1]);

			AddTriangleColor(cell.color);
		}
	}

	/// <summary>
	///     Adds a triangle to the mesh
	/// </summary>
	/// <param name="v1">first triangle vertex</param>
	/// <param name="v2">second triangle vertex</param>
	/// <param name="v3">third triangle vertex</param>
	void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

    /// <summary>
    ///     Adds a color to a triangle
    /// </summary>
    /// <param name="color">color to add</param>
	void AddTriangleColor(Color color)
	{
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

    #endregion
}
