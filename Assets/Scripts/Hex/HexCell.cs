/**
 * File Name: HexCell.cs
 * Description: Script for a hex cell or hex tile
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for a specific hex cell or tile
/// </summary>
public class HexCell : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Public Variables */
    [Tooltip("a cell's coordinates")]
    public HexCoordinates coordinates;

    [Tooltip("a cell's neighbors")]
    [SerializeField] HexCell[] neighbors = null;

    /// <summary>
    /// a cell's reference to the UI Coordinate Text RectTransform
    /// </summary>
    [HideInInspector] public RectTransform uiRectTransform;

    /// <summary>
    /// cell's index count relative to the other cells in the map (assumes only 1 HexGrid)
    /// </summary>
    public int globalIndex;

    /* Private & Protected Variables */

    /// <summary>
    /// a cell's elevation/height
    /// </summary>
    [ReadOnly] [SerializeField] private int elevation = int.MinValue;

    /// <summary>
    /// a reference to a cell's chunk
    /// </summary>
    public HexGridChunk chunk;

    /// <summary>
    /// a cell's terrain type; this variable also initializes the terrain type for the map
    /// </summary>
    private int terrainTypeIndex;

    /// <summary>
    /// a reference to how far away a cell is from the source cell
    /// </summary>
    private int distance;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    /// Gets the cell's local position relative to its parent grid
    /// </summary>
    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    /// <summary>
    /// Elevation/height of a HexCell, retriangulates when setting a new elevation
    /// </summary>
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value) return;

            // update old position to new height
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;

            // perturb hex height
            position = HexMetrics.Perturb(position, perturbElevation: true);

            // set elevation to new height
            elevation = value;
            transform.localPosition = position;

            // update ui label position; because the hex grid canvas is rotated, the labels have to
            // be moved in the negative Z direction, instead of the positive Y direction
            Vector3 uiPosition = uiRectTransform.localPosition;
            uiPosition.z = -position.y;
            uiRectTransform.localPosition = uiPosition;

            // refresh the cell's chunk
            Refresh();
        }
    }

    /// <summary>
    /// Index for a cell's terrain type, retriangulates when setting a new index; the acceptable
    /// index values are determined by a cell's material's texture array
    /// </summary>
    public int TerrainTypeIndex
    {
        get
        {
            return terrainTypeIndex;
        }
        set
        {
            if (terrainTypeIndex != value)
            {
                terrainTypeIndex = value;
                Refresh();
            }
        }
    }

    /// <summary>
    /// Index for a cell's label type, updates a cell's label; [0: offset coordinates, 1: cube
    /// coordinates, 2: navigation/distance label]
    /// </summary>
    public int LabelTypeIndex
    {
        set
        {
            switch (value)
            {
                case 0: // offset coordinates
                    string text = "i:" + globalIndex.ToString() + "\n";
                    text += coordinates.ToStringOnSeparateLines(offset: true, addHeaders: true);
                    UpdateLabel(text);
                    break;

                case 1: // cube coordinates
                    UpdateLabel(coordinates.ToStringOnSeparateLines(addHeaders: true));
                    break;

                case 2: // navigation
                    UpdateLabel(
                        "x",
                        fontStyle: FontStyle.Bold,
                        fontSize: 8
                    );
                    break;
            }
        }
    }

    /// <summary>
    /// Property used for tracking a cell's distance from a source cell; when set, this cell's label
    /// will be updated to its new distance value
    /// </summary>
    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;

            // update distance label
            string text = (distance == int.MaxValue) ? "" : distance.ToString();
            UpdateLabel(text, fontStyle: FontStyle.Bold, fontSize: 8);
        }
    }

    /// <summary>
    /// A reference tracker to a cell's previous neighbor that updated this cell's distance from a 
    /// source cell; this value can be recursively used to trace the path from a cell to the
    /// starting source cell
    /// </summary>
    public HexCell PathFrom { get; set; }

    /// <summary>
    /// A reference to a cell's optimal/potential distance from a source cell; this value can be
    /// used to gauge the possible distance this cell is from the source cell and will return the 
    /// lowest potential distance cost
    /// </summary>
    public int SearchHeuristic { get; set; }

    /// <summary>
    /// A reference to a cell's distance priority for when it should be evaluated in the search
    /// relative to other cells; this value is determined by a cell's current distance from the
    /// source cell and the search heuristic
    /// </summary>
    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }

    /// <summary>
    /// TODO: comment NextWithSamePriority prop
    /// </summary>
    public HexCell NextWithSamePriority { get; set; }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Gets the HexCell neighbor given the direction, might return null
    /// </summary>
    /// <param name="direction">direction to get neighbor</param>
    /// <returns>a HexCell neighbor</returns>
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    /// <summary>
    /// Sets both the given HexCell and the other cell as neighbors to eachother
    /// </summary>
    /// <param name="direction">direction to set neighbor</param>
    /// <param name="cell">reference to HexCell</param>
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    /// <summary>
    /// Gets the HexEdgeType in the given direction, assumes the HexCell has a neighbor; UNDONE:
    /// handle null pointer exception
    /// </summary>
    /// <param name="direction">direction to check the HexEdgeType for</param>
    /// <returns>a HexEdgeType</returns>
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    /// <summary>
    /// Gets the HexEdgeType between this and the given HexCell
    /// </summary>
    /// <param name="otherCell"></param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }

    /// <summary>
    /// Queries a hex cell's chunk (and possibly neighboring chunk) to retriangulate
    /// </summary>
    void Refresh()
    {
        if (chunk != null)
        {
            chunk.Refresh();

            // retriangulate neighbors' chunks if updating cell is from a different chunks
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    private void UpdateLabel(string text, FontStyle fontStyle = FontStyle.Normal, int fontSize = 3)
    {
        Text label = uiRectTransform.gameObject.GetComponent<Text>();

        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
    }

    /// <summary>
    /// Enables the HexCellOutline Sprite
    /// </summary>
    /// <param name="color">Color to set the Sprite</param>
    public void EnableHighlight(Color color)
    {
        Image highlight = uiRectTransform.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    /// <summary>
    /// Disables the HexCellOutline Sprite
    /// </summary>
    public void DisableHighlight()
    {
        Image highlight = uiRectTransform.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    #endregion
}
