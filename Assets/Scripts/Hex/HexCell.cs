﻿/**
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
///     Class for a specific hex cell or tile
/// </summary>
public class HexCell : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Public Variables */
    [Tooltip("a cell's coordinates")]
    public HexCoordinates coordinates;

    [Tooltip("a cell's color")]
    public Color color;

    [Tooltip("a cell's neighbors")]
    [SerializeField] HexCell[] neighbors = null;

    /// <summary>
    ///     a cell's reference to the UI Coordinate Text RectTransform
    /// </summary>
    [HideInInspector] public RectTransform uiRectTransform;

    /* Private & Protected Variables */

    /// <summary>
    ///     a cell's elevation/height
    /// </summary>
    [ReadOnly] [SerializeField] private int elevation;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    ///     Elevation/height of a HexCell
    /// </summary>
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            // TODO: Comment this property

            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;

            // because the hex grid canvas is rotated, the labels have to be moved in the negative Z
            //      direction, instead of the positive Y direction
            Vector3 uiPosition = uiRectTransform.localPosition;
            uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiRectTransform.localPosition = uiPosition;
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    ///     Gets the HexCell neighbor given the direction, might return null
    /// </summary>
    /// <param name="direction">direction to get neighbor</param>
    /// <returns></returns>
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    /// <summary>
    ///     Sets both the given HexCell and the other cell as neighbors to eachother
    /// </summary>
    /// <param name="direction">direction to set neighbor</param>
    /// <param name="cell">reference to HexCell</param>
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    // TODO: comment Function GetEdgeType
    /// <summary>
    /// we already know that we're not dealing with a border edge so NullReferenceException cant
    /// happen
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    // TODO: once uiRect added, finish method EnableHighlight
    public void EnableHighlight(Color color)
    {
        //Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        //highlight.color = color;
        //highlight.enabled = true;
    }

    // TODO: once uiRect added, finish method DisableHighlight
    public void DisableHighlight()
    {
        //Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        //highlight.enabled = false;
    }

    #endregion
}
