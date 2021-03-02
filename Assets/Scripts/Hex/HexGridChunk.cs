/**
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

    // terrain texture variables
    static Color weights1 = new Color(1f, 0f, 0f);
    static Color weights2 = new Color(0f, 1f, 0f);
    static Color weights3 = new Color(0f, 0f, 1f);

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
        cells = new HexCell[HexMetrics.Configuration.ChunkSizeX * HexMetrics.Configuration.ChunkSizeZ];
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

        // allows for the cell ui to be visible
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

        // gets the interior hex cell's vertices for a direction (builds a triangle in a direction)
        HexEdgeVertices e = new HexEdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        // builds interior of the triangle
        TriangulateEdgeFan(center, e, cell.Index);

        // builds connections for Southeast/Northwest, Northeast/Southwest, & North/South
        if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
    }

    /// <summary>
    /// Builds a connection bridge between two cells given a direction
    /// </summary>
    /// <param name="direction">direction to bridge</param>
    /// <param name="cell">starting cell</param>
    /// <param name="e1">edge vertice of starting cell</param>
    protected void TriangulateConnection(HexDirection direction, HexCell cell, HexEdgeVertices e1)
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

            HexEdgeVertices e2 = new HexEdgeVertices(
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
                TriangulateEdgeStrip(
                    e1, weights1, cell.Index,
                    e2, weights2, neighbor.Index
                );
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
        HexEdgeVertices begin, HexCell beginCell,
        HexEdgeVertices end, HexCell endCell
    )
    {
        for (int i = 0; i < HexMetrics.Configuration.TerraceSteps; i++)
        {
            HexEdgeVertices e1 = HexEdgeVertices.TerraceLerp(begin, end, i);
            HexEdgeVertices e2 = HexEdgeVertices.TerraceLerp(begin, end, i + 1);

            Color w1 = HexMetrics.TerraceLerp(weights1, weights2, i);
            Color w2 = HexMetrics.TerraceLerp(weights1, weights2, i + 1);

            TriangulateEdgeStrip(e1, w1, beginCell.Index, e2, w2, endCell.Index);
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

            Vector3 indices;
            indices.x = bottomCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;
            terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
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
        Vector3 right, HexCell rightCell)
    {

        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        for (int i = 0; i < HexMetrics.Configuration.TerraceSteps; i++)
        {
            Vector3 v1 = HexMetrics.TerraceLerp(begin, left, i);
            Vector3 v2 = HexMetrics.TerraceLerp(begin, right, i);
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, i + 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, i + 1);

            Color w1 = HexMetrics.TerraceLerp(weights1, weights2, i);
            Color w2 = HexMetrics.TerraceLerp(weights1, weights3, i);
            Color w3 = HexMetrics.TerraceLerp(weights1, weights2, i + 1);
            Color w4 = HexMetrics.TerraceLerp(weights1, weights3, i + 1);

            

            if (i == 0)
            {
                // v1 == v2 for the first itertion, so we only need to draw triangle
                terrain.AddTriangle(v1, v3, v4);
                terrain.AddTriangleCellData(indices, w1, w3, w4);
            }
            else
            {
                // v1 != v2, so we need to draw quads
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadCellData(indices, w1, w2, w3, w4);
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
        Color boundaryWeights = Color.Lerp(weights1, weights3, b);

        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(
            begin, weights1, left, weights2, boundary, boundaryWeights, indices
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, weights2, right, weights3, boundary, boundaryWeights, indices
            );
        }
        else
        {
            terrain.AddTriangle(
                HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary, perturb: false
            );

            terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
        }
    }

    void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0) b = -b;

        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryWeights = Color.Lerp(weights1, weights2, b);

        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(
            right, weights3, begin, weights1, boundary, boundaryWeights, indices
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, weights2, right, weights3, boundary, boundaryWeights, indices
            );
        }
        else
        {
            terrain.AddTriangle(
                HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary, perturb: false
            );
            terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, Color beginWeights,
        Vector3 left, Color leftWeights,
        Vector3 boundary, Color boundaryWeights, Vector3 indices
    )
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

        terrain.AddTriangle(HexMetrics.Perturb(begin), v2, boundary, perturb: false);
        terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

        for (int i = 2; i < HexMetrics.Configuration.TerraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color w1 = w2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
            terrain.AddTriangle(v1, v2, boundary, perturb: false);
            terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
        }

        terrain.AddTriangle(v2, HexMetrics.Perturb(left), boundary, perturb: false);
        terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
    }

    void TriangulateEdgeFan(Vector3 center, HexEdgeVertices edge, float index)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        //terrain.AddTriangle(center, edge.v4, edge.v5);

        Vector3 indices;
        indices.x = indices.y = indices.z = index; // HACK: this is also not my favorite
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        //terrain.AddTriangleCellData(indices, weights1);
    }

    void TriangulateEdgeStrip(
        HexEdgeVertices e1, Color w1, float index1,
        HexEdgeVertices e2, Color w2, float index2
    )
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        //terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

        Vector3 indices;
		indices.x = indices.z = index1; // HACK: not a huge fan of this line
		indices.y = index2;
		terrain.AddQuadCellData(indices, w1, w2);
		terrain.AddQuadCellData(indices, w1, w2);
		terrain.AddQuadCellData(indices, w1, w2);
		//terrain.AddQuadCellData(indices, w1, w2);
    }

    /// <summary>
    /// TODO: comment refresh
    /// </summary>
    public void Refresh()
	{
		enabled = true;
	}

	#endregion

}