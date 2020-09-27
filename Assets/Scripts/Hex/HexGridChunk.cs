﻿/**
 * File Name: HexGridChunk.cs
 * Description: Script to handle the complex triangulation of the hex map and chunk mesh data
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 24, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: the functions within this file should be renamed
 *      TODO: the last four functions need some touch up
 **/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A hex map mesh chunk; handles a local collection of cells
/// </summary>
public class HexGridChunk : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	HexMesh terrain;

	Canvas gridCanvas;

    HexCell[] cells;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
	{
        terrain = transform.Find("Terrain").GetComponent<HexMesh>();

        gridCanvas = GetComponentInChildren<Canvas>();

        // initialize cells
		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];

        // hide UI
		ShowUI(false);
	}

	/// <summary>
	/// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
	/// Update functions have been called
	/// </summary>
	protected void LateUpdate()
	{
        // whenever this script is enabled (Refresh() is called), the chunk will triangulate
        Triangulate();
        enabled = false;
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

    /// <summary>
    /// Adds a cell to the chunk using the cell's local index
    /// </summary>
    /// <param name="index">the cell's local index</param>
    /// <param name="cell">the cell to be added</param>
	public void AddCell(int index, HexCell cell)
	{
		cells[index] = cell;
		cell.chunk = this;
		cell.transform.SetParent(transform, false);
		cell.uiRectTransform.SetParent(gridCanvas.transform, false);
	}

    /// <summary>
	/// Builds the mesh data for this chunk
	/// </summary>
	public void Triangulate()
    {
        // clear previous mesh data
        terrain.Clear();

        // draw every cell
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        // assign mesh triangulations
        terrain.Apply();
    }

    /// <summary>
    /// Builds a mesh for given cell and its connection to its neighbors
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
    /// Builds the structure of a particular hex direction
    /// </summary>
    /// <param name="direction">direction to triangulate</param>
    /// <param name="cell">the cell to triangulate</param>
    protected void Triangulate(HexDirection direction, HexCell cell)
    {
        // gets the local position of the cell
        Vector3 center = cell.Position;

        // gets the iterior hex cell's vertices for a direction (builds a triangle in a direction)
        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        // builds interior of the triangle
        TriangulateEdgeFan(center, e, cell.Color);

        // builds connections for Southeast/Northwest, Northeast/Southwest, & North/South
        if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
    }

    /// <summary>
    /// Builds a connection bridge between two cells given a direction
    /// </summary>
    /// <param name="direction">direction to bridge</param>
    /// <param name="cell">starting cell</param>
    /// <param name="e1">edge vertice of starting cell</param>
    protected void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        // get the neighbor for the given direction
        HexCell neighbor = cell.GetNeighbor(direction);

        // do not triangulate connection if there is no neighbor
        if (neighbor != null)
        {
            // gets bridge point
            Vector3 bridge = HexMetrics.GetBridge(direction);

            // set elevation of bridge 
            bridge.y = neighbor.Position.y - cell.Position.y;

            EdgeVertices e2 = new EdgeVertices(
                e1.v1 + bridge,
                e1.v4 + bridge
            );

            // triangulates bridge quad
            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(e1, cell, e2, neighbor);
            }
            else
            {
                TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);
            }

            // get next neighbor to build bridge corner
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

            // builds the bridge corner if there is another neighbor; this only needs to be done
            // for Northeast & East because 3 cells share these intersections and is 
            // guaranteed to be triangulated by a cell from one of those 3 directions 
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                // builds corner from the other neighbor's bridge vertex... definitely confusing
                Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());

                // again, set elevation of point to be equal to the (next) neighbor's
                v5.y = nextNeighbor.Position.y;

                // triangulate connection triangle; HACK: can this be improved?
                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                    {
                        TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                    }
                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, e1.v4, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Triangulates the connection edge terraces between two cell's
    /// </summary>
    /// <param name="begin">starting cell's edge vertices</param>
    /// <param name="beginCell">starting cell</param>
    /// <param name="end">ending cell's edge vertices</param>
    /// <param name="endCell">ending cell</param>
    protected void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell
    )
    {
        for (int i = 0; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = EdgeVertices.TerraceLerp(begin, end, i);
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, i + 1);

            Color c1 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i + 1);

            TriangulateEdgeStrip(e1, c1, e2, c2);
        }
    }

    /// <summary>
    /// Triangulates the intersection/corner between three cells given their variation in
    /// elevation; an example of the cell orientations and elevation combinations can be seen here
    /// in these links: http://bit.ly/HexOrientation and http://bit.ly/HexCorners
    /// HACK: can this be improved?
    /// </summary>
    /// <param name="bottom">bottom vertex</param>
    /// <param name="bottomCell">bottom cell</param>
    /// <param name="left">left vertex</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="right">right vertex</param>
    /// <param name="rightCell">right cell</param>
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
            terrain.AddTriangle(bottom, left, right);
            terrain.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    /// <summary>
    /// Triangulates a corner terrace between three cells
    /// </summary>
    /// <param name="begin">begin vertex</param>
    /// <param name="beginCell">begin cell</param>
    /// <param name="left">left vertex</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="right">right vertex</param>
    /// <param name="rightCell">right cell</param>
    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        for (int i = 0; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = HexMetrics.TerraceLerp(begin, left, i);
            Vector3 v2 = HexMetrics.TerraceLerp(begin, right, i);
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, i + 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, i + 1);

            Color c1 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
            Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i + 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i + 1);
            
            if (i == 0)
            {
                // v1 == v2 for the first itertion, so we only need to draw triangle
                terrain.AddTriangle(v1, v3, v4);
                terrain.AddTriangleColor(c1, c3, c4);
            }
            else
            {
                // v1 != v2, so we need to draw quads
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadColor(c1, c2, c3, c4);
            }
        }
    }

    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0) b = -b;

        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

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
            terrain.AddTriangle(
                HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary, perturb: false
            );
            terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
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

        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

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
            terrain.AddTriangle(
                HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary, perturb: false
            );
            terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor
    )
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        terrain.AddTriangle(HexMetrics.Perturb(begin), v2, boundary, perturb: false);
        terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            terrain.AddTriangle(v1, v2, boundary, perturb: false);
            terrain.AddTriangleColor(c1, c2, boundaryColor);
        }

        terrain.AddTriangle(v2, HexMetrics.Perturb(left), boundary, perturb: false);
        terrain.AddTriangleColor(c2, leftCell.Color, boundaryColor);

    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        terrain.AddTriangleColor(color);
    }

    void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        terrain.AddQuadColor(c1, c2);
    }

    public void ShowUI(bool visible)
	{
		gridCanvas.gameObject.SetActive(visible);
	}

	public void Refresh()
	{
		enabled = true;
	}

	#endregion

}