/**
 * File Name: NewMapMenu.cs
 * Description: TODO: comment script
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 1, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;

/// <summary>
/// 
/// </summary>
public class NewMapMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    private HexGrid hexGrid;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        hexGrid = FindObjectOfType<HexGrid>(); // assumes one hex grid in scene
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void Open()
    {
        gameObject.SetActive(true);
        HexMapCamera.Locked = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        HexMapCamera.Locked = false;
    }

    void CreateMap(int x, int z)
    {
        hexGrid.CreateMap(x, z);
        HexMapCamera.ValidatePosition();
        Close();
    }

    public void CreateSmallMap()
    {
        CreateMap(20, 15);
    }

    public void CreateMediumMap()
    {
        CreateMap(40, 30);
    }

    public void CreateLargeMap()
    {
        CreateMap(80, 60);
    }

    #endregion
}