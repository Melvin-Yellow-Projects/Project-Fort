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

/// <summary>
/// 
/// </summary>
public class HexCellShaderData : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    Texture2D cellTexture;
    Color32[] cellTextureData;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    protected void LateUpdate()
    {
        // applies the data to the texture and pushes it to the GPU
        cellTexture.SetPixels32(cellTextureData);
        cellTexture.Apply();
        enabled = false;

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

        // schedule update
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
        // convert 0-1 to bytes
        cellTextureData[cell.Index].r = cell.IsVisible ? (byte)255 : (byte)0;
        enabled = true;
    }

    #endregion

}