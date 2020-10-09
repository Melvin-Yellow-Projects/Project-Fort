/**
 * File Name: HexGameUI.cs
 * Description: TODO: comment script
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// TODO: comment class
/// </summary>
public class HexGameUI : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("instance reference to the HexGrid in the scene")]
    public HexGrid grid;

    HexCell currentCell;

    HexUnit selectedUnit;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    protected void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // verify pointer is not on top of GUI
        {
            if (Input.GetMouseButtonDown(0)) // HACK: hardcoded input / left click
            { 
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1)) //right click
                {
                    DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    bool UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (cell != currentCell)
        {
            currentCell = cell;
            return true;
        }
        return false;
    }

    public void SetEditMode(bool toggle)
    {
        Debug.Log("Game UI is toggled:" + toggle);
        enabled = !toggle;
        grid.ClearPath();
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();

        if (currentCell) selectedUnit = currentCell.Unit;
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell))
            {
                grid.FindPath(selectedUnit.Location, currentCell, 24);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    void DoMove()
    {
        if (grid.HasPath)
        {
            selectedUnit.Travel(grid.GetPath());
            grid.ClearPath();
        }
    }

    #endregion
}