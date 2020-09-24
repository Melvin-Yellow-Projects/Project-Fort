using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	HexCell[] cells;

	public HexMesh terrain; // todo maybe make this private and just find it in the prefab reference
	Canvas gridCanvas;

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		ShowUI(false);
	}

	/// <summary>
	/// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
	/// Update functions have been called
	/// </summary>
	void LateUpdate()
	{
        Triangulate();
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

    /// <summary>
	///     Function to draw all the hex map's cells
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
        Vector3 center = cell.Position;

        // gets the iterior hex cell's vertices
        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        // builds interior
        TriangulateEdgeFan(center, e, cell.Color);

        // builds connections for Southeast/Northwest, Northeast/Southwest, & North/South
        if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
    }

    /// <summary>
    /// Builds a connection bridge between two cells given a direction; TODO: rewrite method
    /// desc
    /// </summary>
    /// <param name="direction">direction to triangulate</param>
    /// <param name="cell">the cell to triangulate</param>
    /// <param name="v1">reused param from Triangulate; first solid hex corner</param>
    /// <param name="v2">reused param from Triangulate; second solid hex corner</param>
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

                // triangulate connection triangle 
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

    // TODO: Comment TriangulateEdgeTerraces
    protected void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell
    )
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        TriangulateEdgeStrip(e2, c2, end, endCell.Color);

        //for (int i = 0; i < HexMetrics.terraceSteps; i++)
        //{
        //    Vector3 v1 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
        //    Vector3 v2 = HexMetrics.TerraceLerp(beginRight, endRight, i);
        //    Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i + 1);
        //    Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, i + 1);

        //    Color c1 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
        //    Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i + 1);

        //    AddQuad(v1, v2, v3, v4);
        //    AddQuadColor(c1, c2);
        //}
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
            terrain.AddTriangle(bottom, left, right);
            terrain.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

        terrain.AddTriangle(begin, v3, v4);
        terrain.AddTriangleColor(beginCell.Color, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadColor(c1, c2, c3, c4);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
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
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
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
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
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

        terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
        terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            terrain.AddTriangleUnperturbed(v1, v2, boundary);
            terrain.AddTriangleColor(c1, c2, boundaryColor);
        }

        terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
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