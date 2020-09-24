/**
 * File Name: HexMesh.cs
 * Description: Class responsible for handling the triangulation and orientation of a hex mesh
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      UNDONE: Class description is subject to change
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A mesh specificly for a hex map; UNDONE: subject to change
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	/* Settings */
	[Header("Settings")]
	[Tooltip("wether or not this HexMesh uses a MeshCollider")]
	public bool useCollider;

	[Tooltip("wether or not this HexMesh uses different vertex colors")]
	public bool useColors;

	[Tooltip("wether or not this HexMesh uses its UV coordinates")]
	public bool useUVCoordinates;

	/* Private & Protected Variables */
	protected Mesh hexMesh; // mesh object

	// mesh's vertices TODO: comment these three ~static lists
	[NonSerialized] List<Vector3> vertices;

	// mesh's color at a given vertex
	[NonSerialized] List<Color> colors;

	[NonSerialized] List<Vector2> uvs;

	// mesh's triangle draw order (how to draw the mesh from the vertices, i.e. there might be more
	// triangles than verticesto save space)
	[NonSerialized] List<int> triangles;

	// collider for the mesh
	protected MeshCollider meshCollider;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	/// <summary>
	///     Unity Method; Awake() is called before Start() upon GameObject creation
	/// </summary>
	protected void Awake()
	{
        // initialize mesh
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();

		if (useCollider) meshCollider = gameObject.AddComponent<MeshCollider>();

		hexMesh.name = "Hex Mesh";
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public void Clear()
	{
		hexMesh.Clear();

		vertices = ListPool<Vector3>.Get();

		if (useColors) colors = ListPool<Color>.Get();

		if (useUVCoordinates) uvs = ListPool<Vector2>.Get();

		triangles = ListPool<int>.Get();
	}

	public void Apply()
	{
		hexMesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);

		if (useColors)
		{
			hexMesh.SetColors(colors);
			ListPool<Color>.Add(colors);
		}

		if (useUVCoordinates)
		{
			hexMesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}

		hexMesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);

		hexMesh.RecalculateNormals();

		if (useCollider) meshCollider.sharedMesh = hexMesh;
	}

	public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
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
	///     Adds a triangle to the mesh
	/// </summary>
	/// <param name="v1">first triangle vertex</param>
	/// <param name="v2">second triangle vertex</param>
	/// <param name="v3">third triangle vertex</param>
	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(v1));
        vertices.Add(HexMetrics.Perturb(v2));
        vertices.Add(HexMetrics.Perturb(v3));
        triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	/// <summary>
	///     Adds a color to a triangle
	/// </summary>
	/// <param name="color">color to add</param>
	public void AddTriangleColor(Color color)
	{
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	/// <summary>
	///     Adds/blends three colors to a triangle
	/// </summary>
	/// <param name="c1">color for the first vertex</param>
	/// <param name="c2">color for the second vertex</param>
	/// <param name="c3">color for the third vertex</param>
	public void AddTriangleColor(Color c1, Color c2, Color c3)
	{
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	/// <summary>
	///     Triangulates a quad given four vertices; structure adheres to (v1, v3, v2) & (v2, v3,
	///         v4)
	/// </summary>
	/// <param name="v1">first vertex</param>
	/// <param name="v2">second vertex</param>
	/// <param name="v3">third vertex</param>
	/// <param name="v4">fourth vertex</param>
	public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(v1));
        vertices.Add(HexMetrics.Perturb(v2));
        vertices.Add(HexMetrics.Perturb(v3));
        vertices.Add(HexMetrics.Perturb(v4));

        triangles.Add(vertexIndex);     // v1
		triangles.Add(vertexIndex + 2); // v3
		triangles.Add(vertexIndex + 1); // v2

		triangles.Add(vertexIndex + 1); // v2
		triangles.Add(vertexIndex + 2); // v3
		triangles.Add(vertexIndex + 3); // v4
	}

	/// <summary>
	///     Adds two colors to a triangulated quad
	/// </summary>
	/// <param name="c1">first color</param>
	/// <param name="c2">second color</param>
	public void AddQuadColor(Color c1, Color c2)
	{
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}

	/// <summary>
	///     Adds four colors to a triangulated quad
	/// </summary>
	/// <param name="c1">first color</param>
	/// <param name="c2">second color</param>
	/// <param name="c3">third color</param>
	/// <param name="c4">fourth color</param>
	public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
	{
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

	public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
	{
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}

	public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
	{
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

    // I dont get this method
	public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
	{
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}

	#endregion
}
