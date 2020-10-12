/**
 * File Name: HexCellShaderData.cs
 * Description: Script to manage the texture that contains the cell data
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: touch up comments in HexCellShaderData
 **/

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class HexCellShaderData : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    const float transitionSpeed = 255f;

    Texture2D cellTexture;
    Color32[] cellTextureData;

    List<HexCell> transitioningCells = new List<HexCell>();

    bool needsVisibilityReset;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public bool ImmediateMode { get; set; }

    public HexGrid Grid { get; set; }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    protected void LateUpdate()
    {
        if (needsVisibilityReset)
        {
            needsVisibilityReset = false;
            Grid.ResetVisibility();
        }

        int delta = (int)(Time.deltaTime * transitionSpeed);
        if (delta == 0) delta = 1;

        for (int i = transitioningCells.Count; i > 0; i--)
        {
            if (!UpdateCellData(transitioningCells[i - 1], delta)) transitioningCells.RemoveAt(i - 1);
        }

        // applies the data to the texture and pushes it to the GPU
        cellTexture.SetPixels32(cellTextureData);
        cellTexture.Apply();

        enabled = (transitioningCells.Count > 0);

        // "To actually apply the data to the texture and push it to the GPU, we have to invoke
        // Texture2D.SetPixels32 followed by Texture2D.Apply. Like we do with chunks, we're going
        // to delay this to LateUpdate so we do it at most once per frame, no matter how many cells
        // were changed."
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Creates a cell texture; "We'll use an RGBA texture, without mipmaps, and in linear color
    /// space. We don't want to blend cell data, so use point filtering. Also, the data shouldn't
    /// wrap. Each pixel of the texture will hold the data of one cell."
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void Initialize(int x, int z)
    {
        // does not create a texture if already initialized; instead, it simply resizes to fit map
        if (cellTexture)
        {
            cellTexture.Resize(x, z);
        }
        else
        {
            cellTexture = new Texture2D(x, z, TextureFormat.RGBA32, false, true);
            cellTexture.filterMode = FilterMode.Point;
            cellTexture.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture("_HexCellData", cellTexture);
        }

        Shader.SetGlobalVector("_HexCellData_TexelSize", new Vector4(1f / x, 1f / z, x, z));

        // "Instead of applying cell data one pixel at a time, we'll use a color buffer and apply
        // all cell data in one go."
        if (cellTextureData == null || cellTextureData.Length != x * z)
        {
            // create a new array instance when needed
            cellTextureData = new Color32[x * z];
        }
        else
        {
            // reset array contents if we already one of the correct size
            for (int i = 0; i < cellTextureData.Length; i++)
            {
                cellTextureData[i] = new Color32(0, 0, 0, 0); // color32 directly uses byte data
            }
        }

        transitioningCells.Clear();

        // schedule update
        enabled = true;
    }

    bool UpdateCellData(HexCell cell, int delta)
    {
        int index = cell.Index;
        Color32 data = cellTextureData[index];
        bool stillUpdating = false;

        // explored data stored in g channel
        if (cell.IsExplored && data.g < 255)
        {
            stillUpdating = true;

            // Update cell data; Arithmatic operations don't work on bytes, they are always 
            // converted to integers first. So the sum is an integer, which has to be cast to a byte
            int t = data.g + delta;
            data.g = (t >= 255) ? (byte)255 : (byte)t; // make sure there is no overflow
        }

        // visible data stored in r channel
        if (cell.IsVisible)
        {
            if (data.r < 255)
            {
                stillUpdating = true;
                int t = data.r + delta;
                data.r = t >= 255 ? (byte)255 : (byte)t;
            }
        }
        else if (data.r > 0)
        {
            stillUpdating = true;
            int t = data.r - delta;
            data.r = t < 0 ? (byte)0 : (byte)t;
        }

        if (!stillUpdating) data.b = 0;

        cellTextureData[index] = data;
        return stillUpdating;
    }

    public void ViewElevationChanged()
    {
        needsVisibilityReset = true;
        enabled = true;
    }

    public void RefreshTerrain(HexCell cell)
    {
        // this allows us to support 256 terrain types
        cellTextureData[cell.Index].a = (byte)cell.TerrainTypeIndex;

        // schedule update
        enabled = true; 
    }

    public void RefreshVisibility(HexCell cell)
    {
        int index = cell.Index;


        if (ImmediateMode)
        {
            // convert 0-1 to bytes
            cellTextureData[index].r = cell.IsVisible ? (byte)255 : (byte)0; // stored in r channel
            cellTextureData[index].g = cell.IsExplored ? (byte)255 : (byte)0; // stored in g channel
        }
        else if (cellTextureData[index].b != 255) // blue channel flag if a cell is transitioning
        {
            cellTextureData[index].b = 255;
            transitioningCells.Add(cell);
        }

        enabled = true;
    }

    #endregion

}