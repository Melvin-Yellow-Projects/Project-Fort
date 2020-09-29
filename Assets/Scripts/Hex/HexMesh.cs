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

    [Tooltip("wether or not this HexMesh uses varying types of terrain")]
    public bool useTerrainTypes;

    /* Private & Protected Variables */

    /// <summary>
    /// class's mesh object
    /// </summary>
    protected Mesh hexMesh; 

	/// <summary>
	/// mesh's vertices; this variable is used as a placeholder for the static ListPool struct
	/// </summary>
	[NonSerialized] List<Vector3> vertices;

    // TODO: comment terrainTypes
    [NonSerialized] List<Vector3> terrainTypes;

    /// <summary>
    /// mesh's color at a given vertex; this variable is used as a placeholder for the static
    /// ListPool struct
    /// </summary>
    [NonSerialized] List<Color> colors;

	/// <summary>
	/// mesh's uvs; this variable is used as a placeholder for the static ListPool struct
	/// </summary>
	[NonSerialized] List<Vector2> uvs;

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

		if (useColors) colors = ListPool<Color>.Get();

		if (useUVCoordinates) uvs = ListPool<Vector2>.Get();

        if (useTerrainTypes) terrainTypes = ListPool<Vector3>.Get();

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

		// set optional colors
		if (useColors)
		{
			hexMesh.SetColors(colors);
			ListPool<Color>.Add(colors);
		}

        // set optional UV coordinates
		if (useUVCoordinates)
		{
			hexMesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}

        if (useTerrainTypes)
        {
            // "store the terrain types in the third UV set. That way, it won't clash with the other
            // two sets, if we were to ever use them together."
            // I think its 0: water, 1: shore, 2: terrain
            hexMesh.SetUVs(2, terrainTypes);
            ListPool<Vector3>.Add(terrainTypes);
        }

        // set triangles
        hexMesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);

        // apply data to hex mesh
		hexMesh.RecalculateNormals();

        // set optional collider
		if (useCollider) meshCollider.sharedMesh = hexMesh;
	}

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
	/// Adds a color to a triangle
	/// </summary>
	/// <param name="color">color to add</param>
	public void AddTriangleColor(Color color)
	{
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	/// <summary>
	/// Adds/blends three colors to a triangle
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
	/// Adds two colors to a triangulated quad
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
	/// Adds four colors to a triangulated quad
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

    // TODO: comment AddTriangleTerrainTypes
    public void AddTriangleTerrainTypes(Vector3 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }

    // TODO: comment AddQuadTerrainTypes
    public void AddQuadTerrainTypes(Vector3 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }

    #endregion
}
