/**
 * File Name: HexMesh.cs
 * Description: Class responsible for handling the triangulation and orientation of a generic hex
 *                  mesh
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

/// <summary>
/// A general hex mesh object; consists of a Mesh Filter, a Mesh Renderer, and an optional Mesh
/// Collider
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	/********** MARK: Public Variables **********/
	#region Public Variables

	/* Settings */
	[Header("Settings")]
	[Tooltip("wether or not this HexMesh uses a MeshCollider")]
	public bool useCollider;

	[Tooltip("wether or not this HexMesh uses cell data")]
	public bool useCellData;

	[Tooltip("wether or not this HexMesh uses its UV coordinates")]
	public bool useUVCoordinates;

	//[Tooltip("wether or not this HexMesh uses its UV coordinates")]
	//public bool useUV2Coordinates;

	#endregion

	/********** MARK: Private Variables **********/
	#region Private Variables

	/// <summary>
	/// class's mesh object
	/// </summary>
	protected Mesh hexMesh; 

	/// <summary>
	/// mesh's vertices; this variable is used as a placeholder for the static ListPool struct
	/// </summary>
	[NonSerialized] List<Vector3> vertices;

    // TODO: comment terrainTypes
    [NonSerialized] List<Vector3> cellIndices;

    /// <summary>
    /// mesh's color at a given vertex; this variable is used as a placeholder for the static
    /// ListPool struct
    /// </summary>
    [NonSerialized] List<Color> cellWeights;

	/// <summary>
	/// mesh's uvs; this variable is used as a placeholder for the static ListPool struct
	/// </summary>
	[NonSerialized] List<Vector2> uvs;

	//[NonSerialized] List<Vector2> uv2s;

	/// <summary>
	/// mesh's triangle draw order (how to draw the mesh from the vertices, i.e. there might be more
	/// triangles than vertices to save space); this variable is used as a placeholder for the 
	/// static ListPool struct
	/// </summary>
	[NonSerialized] List<int> triangles;

	/// <summary>
    /// mesh's optional collider 
    /// </summary>
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
		hexMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = hexMesh;
		hexMesh.name = "Hex Mesh";

		// set a mesh collider if it is enabled
		if (useCollider) meshCollider = gameObject.AddComponent<MeshCollider>();
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

    /// <summary>
    /// Clears the hex mesh by clearing the ListPool structs
    /// </summary>
	public void Clear()
	{
		hexMesh.Clear();

		vertices = ListPool<Vector3>.Get();

		if (useCellData)
		{
			cellWeights = ListPool<Color>.Get();
			cellIndices = ListPool<Vector3>.Get();
		}
        
		if (useUVCoordinates) uvs = ListPool<Vector2>.Get();

		//if (useUV2Coordinates) uv2s = ListPool<Vector2>.Get();

		triangles = ListPool<int>.Get();
	}

    /// <summary>
    /// Sets the hex mesh's data through setting the ListPool structs
    /// </summary>
	public void Apply()
	{
        // set vertices
		hexMesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);

		if (useCellData)
		{
			hexMesh.SetColors(cellWeights);
			ListPool<Color>.Add(cellWeights);
			hexMesh.SetUVs(2, cellIndices);
			ListPool<Vector3>.Add(cellIndices);
		}

        // set optional UV coordinates
		if (useUVCoordinates)
		{
			hexMesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}

		//// set optional UV2 coordinates
		//if (useUV2Coordinates)
		//{
		//	hexMesh.SetUVs(1, uv2s);
		//	ListPool<Vector2>.Add(uv2s);
		//}

        // set triangles
        hexMesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);

        // apply data to hex mesh
		hexMesh.RecalculateNormals();

        // set optional collider
		if (useCollider) meshCollider.sharedMesh = hexMesh;
	}

	/********** MARK: Triangle Functions **********/
	#region Triangle Functions

	/// <summary>
	/// Adds a triangle to the mesh; can also perturb this triangle's vertices
	/// </summary>
	/// <param name="v1">first triangle vertex</param>
	/// <param name="v2">second triangle vertex</param>
	/// <param name="v3">third triangle vertex</param>
	/// <param name="perturb">optional perturb flag, default is set to true</param>
	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, bool perturb = true)
	{
		int vertexIndex = vertices.Count;

        if (perturb)
        {
			vertices.Add(HexMetrics.Perturb(v1));
			vertices.Add(HexMetrics.Perturb(v2));
			vertices.Add(HexMetrics.Perturb(v3));
		}
        else
        {
			vertices.Add(v1);
			vertices.Add(v2);
			vertices.Add(v3);
		}
        
        triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	/// <summary>
	/// Adds three UV coordinates to a triangle
	/// </summary>
	/// <param name="uv1">first UV coordinate</param>
	/// <param name="uv2">second UV coordinate</param>
	/// <param name="uv3">third UV coordinate</param>
	public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
	{
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}

	/// <summary>
	/// TODO: comment AddTriangleCellData
	/// </summary>
	/// <param name="indices"></param>
	/// <param name="weights1"></param>
	/// <param name="weights2"></param>
	/// <param name="weights3"></param>
	public void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3)
	{
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellWeights.Add(weights1);
		cellWeights.Add(weights2);
		cellWeights.Add(weights3);
	}

	/// <summary>
	/// TODO: comment AddTriangleCellData
	/// </summary>
	/// <param name="indices"></param>
	/// <param name="weights"></param>
	public void AddTriangleCellData(Vector3 indices, Color weights)
	{
		AddTriangleCellData(indices, weights, weights, weights);
	}

    #endregion

    /********** MARK: Quad Functions **********/

    /// <summary>
    /// Triangulates a quad given four vertices; structure adheres to (v1, v3, v2) & (v2, v3, v4)
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
	/// Adds four UV coordinates to a triangulated quad
	/// </summary>
	/// <param name="uv1">first UV coordinate</param>
	/// <param name="uv2">second UV coordinate</param>
	/// <param name="uv3">third UV coordinate</param>
	/// <param name="uv4">fourth UV coordinate</param>
	public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
	{
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

	/// <summary>
	/// Adds four UV coordinates to a triangulated quad using 4 uv values; TODO: TBH I don't get 
	/// this function 
	/// </summary>
	/// <param name="uMin"></param>
	/// <param name="uMax"></param>
	/// <param name="vMin"></param>
	/// <param name="vMax"></param>
	public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
	{
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}

	// TODO Comment AddQuadCellData
	public void AddQuadCellData(
        Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4)
	{
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellWeights.Add(weights1);
		cellWeights.Add(weights2);
		cellWeights.Add(weights3);
		cellWeights.Add(weights4);
	}

	// TODO Comment AddQuadCellData
	public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2)
	{
		AddQuadCellData(indices, weights1, weights1, weights2, weights2);
	}

	// TODO Comment AddQuadCellData
	public void AddQuadCellData(Vector3 indices, Color weights)
	{
		AddQuadCellData(indices, weights, weights, weights, weights);
	}

	#endregion
}
