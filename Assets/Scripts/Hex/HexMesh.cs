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

	protected Mesh hexMesh; // mesh object

	// mesh's vertices
	protected List<Vector3> vertices;

	// mesh's color at a given vertex
	protected List<Color> colors;

	// mesh's triangle draw order (how to draw the mesh from the vertices, i.e. there might be more
	//      triangles than verticesto save space)
	protected List<int> triangles; 

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
	///     Function to draw all the hex map's cells
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
	///    Builds a mesh for given cell and connects it to its neighbors
	/// </summary>
	/// <param name="cell">HexCell to draw</param>
	protected void Triangulate(HexCell cell)
	{
        // triangulates the mesh in each hex direction
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			Triangulate(d, cell);
		}
	}

	/// <summary>
	///     Builds the structure of a particular hex direction
	/// </summary>
	/// <param name="direction">direction to triangulate</param>
	/// <param name="cell">the cell to triangulate</param>
	protected void Triangulate(HexDirection direction, HexCell cell)
	{
        // gets the local position of the cell
		Vector3 center = cell.transform.localPosition;

        // gets the iterior hex cell's vertices
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        // builds interior
		AddTriangle(center, v1, v2);
		AddTriangleColor(cell.color);

		// builds connections for Southeast/Northwest, Northeast/Southwest, & North/South
		if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, v1, v2);

	}

    /// <summary>
    ///     Builds a connection bridge between two cells given a direction
    /// </summary>
    /// <param name="direction">direction to triangulate</param>
    /// <param name="cell">the cell to triangulate</param>
    /// <param name="v1">reused param from Triangulate; first solid hex corner</param>
    /// <param name="v2">reused param from Triangulate; second solid hex corner</param>
    protected void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
	{
        // get the neighbor for the given direction
		HexCell neighbor = cell.GetNeighbor(direction);

        // do not triangulate connection if there is no neighbor
		if (neighbor != null)
		{
            // gets birdge point
			Vector3 bridge = HexMetrics.GetBridge(direction);

            // gets bridge vertices
			Vector3 v3 = v1 + bridge;
			Vector3 v4 = v2 + bridge;

            // set elevation of bridge to be equal to the neighbor's
            v3.y = neighbor.Elevation * HexMetrics.elevationStep;
			v4.y = neighbor.Elevation * HexMetrics.elevationStep;

            // triangulates bridge quad
            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            }
            else
            {
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(cell.color, neighbor.color);
            }

            // get next neighbor to build bridge corner
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

            // builds the bridge corner if there is another neighbor; this only needs to be done
            // for Northeast & East because 3 cells share these intersections and is 
            // guaranteed to be triangulated by a cell from one of those 3 directions 
            if (direction <= HexDirection.E && nextNeighbor != null)
			{
				// builds corner from the other neighbor's bridge vertex... definitely confusing
				Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());

				// again, set elevation of point to be equal to the (next) neighbor's
				v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

                // triangulate connection triangle 
                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                    {
                        TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                    }
                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
		}
	}

    // TODO: Comment TriangulateEdgeTerraces
    protected void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, 
        Vector3 endLeft, Vector3 endRight, HexCell endCell
    )
    {
        for (int i = 0; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            Vector3 v2 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i + 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, i + 1);

            Color c1 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i + 1);

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }
    }

    // TODO: comment and refactor TriangulateCorner
    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }
        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerCliffTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
        }
        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }
    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);
    }

    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0) b = -b;

        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        TriangulateBoundaryTriangle(
            begin, beginCell, left, leftCell, boundary, boundaryColor
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor
            );
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0) b = -b;

        Vector3 boundary = Vector3.Lerp(begin, left, b);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

        TriangulateBoundaryTriangle(
            right, rightCell, begin, beginCell, boundary, boundaryColor
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor
            );
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor
    )
    {

        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }

    /// <summary>
    ///     Adds a triangle to the mesh
    /// </summary>
    /// <param name="v1">first triangle vertex</param>
    /// <param name="v2">second triangle vertex</param>
    /// <param name="v3">third triangle vertex</param>
    protected void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
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
	protected void AddTriangleColor(Color color)
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
	protected void AddTriangleColor(Color c1, Color c2, Color c3)
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
	protected void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		vertices.Add(v4);

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
	void AddQuadColor(Color c1, Color c2)
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
	protected void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
	{
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

	#endregion
}
