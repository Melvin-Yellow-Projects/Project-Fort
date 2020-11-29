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
using System.IO;

/// <summary>
/// Class for a specific hex cell or tile
/// </summary>
public class HexCell : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    /* Public Variables */
    [Tooltip("a cell's coordinates")]
    public HexCoordinates coordinates;

    [Tooltip("a cell's neighbors")]
    [SerializeField] HexCell[] neighbors = null;

    /// <summary>
    /// a reference to a cell's chunk
    /// </summary>
    public HexGridChunk chunk;

    /// <summary>
    /// a cell's reference to the UI Coordinate Text RectTransform
    /// </summary>
    [HideInInspector] public RectTransform uiRectTransform;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /// <summary>
    /// a cell's elevation/height
    /// </summary>
    [ReadOnly] [SerializeField] private int elevation = int.MinValue;

    /// <summary>
    /// a cell's terrain type; this variable also initializes the terrain type for the map
    /// </summary>
    private int terrainTypeIndex;

    int visibility;

    bool explored;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    /// cell's index count relative to the other cells in the map (assumes only 1 HexGrid)
    /// </summary>
    public int Index { get; set; }

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

            int originalViewElevation = ViewElevation;

            // updates new cell position
            elevation = value;

            // alert the cell's shader data that the view has changed
            if (ViewElevation != originalViewElevation) ShaderData.ViewElevationChanged();

            RefreshPosition();

            // UNDONE: validation logic should go here for rivers, terrain, etc.

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
                ShaderData.RefreshTerrain(this);
            }
        }
    }

    /// <summary>
    /// Index for a cell's label type, updates a cell's label; [0: offset coordinates, 1: cube
    /// coordinates, 2: navigation/distance label] HACK: is this realllllly needed
    /// </summary>
    public int LabelTypeIndex
    {
        set
        {
            switch (value)
            {
                case 0: // hide text (for navigation)
                    SetLabel(null);
                    break;

                case 1: // offset coordinates
                    string text = "i:" + Index.ToString() + "\n";
                    text += coordinates.ToStringOnSeparateLines(offset: true, addHeaders: true);
                    SetLabel(text);
                    break;

                case 2: // cube coordinates
                    SetLabel(coordinates.ToStringOnSeparateLines(addHeaders: true));
                    break;
            }
        }
    }

    /// <summary>
    /// Property used for tracking a cell's distance from a source cell
    /// </summary>
    public int Distance { get; set; }

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
            return Distance + SearchHeuristic;
        }
    }

    /// <summary>
    /// A reference to a cell's adjacent neighbor in the linked list data structure of the
    /// HexCellPriorityQueue object; if this property is null, then the cell has no neighbor in the
    /// queue
    /// </summary>
    public HexCell NextWithSamePriority { get; set; }

    /// <summary>
    /// Tracker of which phase of the search a cell is in; either not yet in the frontier [0],
    /// currently part of the frontier [1], or behind the frontier 2
    /// </summary>
    public int SearchPhase { get; set; }

    /// <summary>
    /// TODO: comment Unit
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// TODO: comment ShaderData
    /// </summary>
    public HexCellShaderData ShaderData { get; set; }

    /// <summary>
    /// TODO: comment IsVisible
    /// </summary>
    public bool IsVisible
    {
        get
        {
            return visibility > 0 && Explorable;
        }
    }

    public bool IsExplored
    {
        get
        {
            return explored && Explorable;
        }
        private set
        {
            explored = value;
        }
    }

    public int ViewElevation
    {
        get
        {
            return elevation;
            //return elevation >= waterLevel ? elevation : waterLevel;
        }
    }

    public bool Explorable { get; set; }

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
    /// Checks to see if a given cell is a neighbor of this cell
    /// </summary>
    /// <param name="cell">potential neighboring cell</param>
    /// <returns>whether or not given cell is a neighbor</returns>
    public bool IsNeighbor(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            if (cell == GetNeighbor(d)) return true;
        }
        return false;
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

            // refresh unit location
            if (Unit) Unit.ValidateLocation();
        }
    }

    //void RefreshSelfOnly()
    //{
    //    chunk.Refresh();
    //    if (Unit) Unit.ValidateLocation();
    //}

    /// <summary>
    /// TODO: comment RefreshPosition, is the name right since it's only for elevation?
    /// </summary>
    private void RefreshPosition()
    {
        // update old position to new height
        Vector3 position = transform.localPosition;
        position.y = elevation * HexMetrics.elevationStep;

        // perturb hex height
        position = HexMetrics.Perturb(position, perturbElevation: true);

        // set position to new height
        transform.localPosition = position;

        // update ui label position; because the hex grid canvas is rotated, the labels have to
        // be moved in the negative Z direction, instead of the positive Y direction
        Vector3 uiPosition = uiRectTransform.localPosition;
        uiPosition.z = -position.y;
        uiRectTransform.localPosition = uiPosition;
    }

    /// <summary>
    /// Updates the label that is connected to this cell
    /// </summary>
    /// <param name="text">new text to write on the label</param>
    /// <param name="fontStyle">the style of the font; default is Normal</param>
    /// <param name="fontSize">the size of the font; default is 3</param>
    public void SetLabel(string text, FontStyle fontStyle = FontStyle.Normal, int fontSize = 3)
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

    /// <summary>
    /// TODO: comment IncreaseVisibility
    /// </summary>
    public void IncreaseVisibility()
    {
        visibility += 1;
        if (visibility == 1)
        {
            IsExplored = true;
            ShaderData.RefreshVisibility(this);
        }
    }

    /// <summary>
    /// TODO: comment DecreaseVisibility
    /// </summary>
    public void DecreaseVisibility()
    {
        visibility -= 1;
        if (visibility == 0) ShaderData.RefreshVisibility(this);
    }

    public void ResetVisibility()
    {
        if (visibility > 0)
        {
            visibility = 0;
            ShaderData.RefreshVisibility(this);
        }
    }

    /// <summary>
    /// TODO: write Save; because our integers will certainly be within the range of 0 to 255, we
    /// can use bytes instead of integers; see Hex Map section 12 to see further ways to reduce
    /// file size
    /// </summary>
    /// <param name="writer"></param>
    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)elevation);
        writer.Write(IsExplored);
    }

    /// <summary>
    /// TODO: write Load func
    /// </summary>
    /// <param name="reader"></param>
    public void Load(BinaryReader reader, int header)
    {
        // HACK: this could be replaced with the terrain type index property
        terrainTypeIndex = reader.ReadByte();
        ShaderData.RefreshTerrain(this);

        elevation = reader.ReadByte();

        // HACK: hardcoded value
        IsExplored = header >= 3 ? reader.ReadBoolean() : false;
        ShaderData.RefreshVisibility(this);

        RefreshPosition();
    }

    #endregion
}
