/**
 * File Name: HexCell.cs
 * Description: Script for a hex cell or tile
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

    /* Private & Protected Variables */

    /// <summary>
    /// a cell's elevation/height
    /// </summary>
    [ReadOnly] [SerializeField] private int elevation = int.MinValue;

    [ReadOnly] public HexGridChunk chunk;

    /// <summary>
    /// a cell's color; connects to the mesh's UV colors's i think TODO: figure this out
    /// </summary>
    private Color color;

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
    /// Elevation/height of a HexCell
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

            // add elevation noise
            float noiseSample = HexMetrics.SampleNoise(position).y;
            position.y += (noiseSample * 2f - 1f) * HexMetrics.elevationPerturbStrength;

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

    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Gets the HexCell neighbor given the direction, might return null
    /// </summary>
    /// <param name="direction">direction to get neighbor</param>
    /// <returns></returns>
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
    /// Gets the HexEdgeType in the given direction, assumes the HexCell has a neighbor; TODO:
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

    // TODO: write method
    void Refresh()
    {
        if (chunk != null)
        {
            chunk.Refresh();

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
