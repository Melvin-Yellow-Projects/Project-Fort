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

    HexDirection selectedDirection;

    bool hasCurrentCellUpdated = false;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    protected void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // verify pointer is not on top of GUI
        {
            hasCurrentCellUpdated = UpdateCurrentCell();

            if (Input.GetMouseButtonDown(0)) // HACK: hardcoded input / left click
            {
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1)) //right click
                {
                    //DoMove();
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
        HexCell cell = grid.GetCell();
        if (cell != currentCell)
        {
            if (currentCell) currentCell.DisableHighlight();
            if (cell) cell.EnableHighlight(new Color(1f, 0f, 0f, 0.6f));

            currentCell = cell;
            return true; // whether or not current cell has updated
        }
        return false;
    }

    void DoSelection()
    {
        if (currentCell)
        {
            if (selectedUnit) selectedUnit.IsSelected = false;

            selectedUnit = currentCell.Unit;

            if (selectedUnit)
            {
                selectedUnit.Path.Clear();
                selectedUnit.IsSelected = true;
            }
        }
    }

    void DoPathfinding()
    {
        if (!hasCurrentCellUpdated && currentCell != null) return;

        if (Input.GetKey("left shift"))
        {
            // can backtrack
            selectedUnit.Path.AddCellToPath(currentCell, canBackTrack: true);
        }
        else
        {
            // can't backtrack
            selectedUnit.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedUnit.Path.Show();
    }

    #endregion

    #region Debug Functions

    //void DoMove()
    //{
    //    if (selectedUnit.Path.HasPath)
    //    {
    //        selectedUnit.Move();

    //        Debug.Log(selectedDirection);
    //    }
    //    else
    //    {
    //        selectedUnit.LookAt(selectedDirection);
    //    }
    //}

    //void DoPathfinding()
    //{
    //    HexCell cell = grid.GetCell();
    //    if (!cell) return;

    //    // get new path
    //    if (cell != currentCell)
    //    {
    //        currentCell = cell;
    //        if (selectedUnit.IsValidDestination(currentCell))
    //        {
    //            HexPath path = HexPathfinding.FindPath(selectedUnit.MyCell, currentCell, selectedUnit);
    //            selectedUnit.Path = path;
    //            if (selectedUnit.Path.HasPath) selectedUnit.Path.Show(selectedUnit.Speed);

    //            //if (selectedUnit.HasPath) selectedUnit.Path.LogPath();
    //        }
    //    }
    //}

    #endregion
}