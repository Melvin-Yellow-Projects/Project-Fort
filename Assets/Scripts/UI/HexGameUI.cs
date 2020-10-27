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

    HexUnit selectedUnit;

    HexCell currentCell;
    HexDirection currentDirection;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    protected void Update() // TODO: this could probably be OnDrag() or some variant of it
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

    void DoSelection()
    {
        // update current cell
        HexCell cell = grid.GetCell();
        if (cell != currentCell) currentCell = cell;

        if (currentCell) selectedUnit = currentCell.Unit;
    }

    //HexPath path;

    void DoPathfinding()
    {
        HexCell cell = grid.GetCell();
        if (!cell) return;

        HexDirection direction = GetDirection(); // HACK: assumes valid cell

        if (Input.GetKey("left shift"))
        {
            if (cell != currentCell || direction != currentDirection)
            {
                if (selectedUnit.IsValidDestination(cell))
                {
                    currentCell = cell;
                    currentDirection = direction;
                    
                    HexPath path = HexPathfinding.BuildPath(selectedUnit, cell, direction);
                    if (path != null)
                    {
                        //selectedUnit.Path = path; // BUG: does not work for creating paths for unit
                        selectedUnit.Path.Show();
                    }
                }
            }
        }

        // get new path
        else if (cell != currentCell)
        {
            if (selectedUnit.IsValidDestination(cell))
            {
                currentCell = cell;

                HexPath path = HexPathfinding.FindPath(selectedUnit, selectedUnit.MyCell, cell, HexDirection.E);
                if (path != null)
                {
                    selectedUnit.Path = path;
                    selectedUnit.Path.Show();
                }

                //if (selectedUnit.HasPath) selectedUnit.Path.LogPath();
            }
        }

        //if (cell == currentCell) // get end path direction
        //{
        //    SetHexDirection(cell);
        //}
    }

    //void DoMove()
    //{
    //    if (selectedUnit.HasPath)
    //    {
    //        selectedUnit.Travel();

    //        Debug.Log(selectedDirection);
    //    }
    //    else
    //    {
    //        selectedUnit.LookAt(selectedDirection);
    //    }
    //}

    // HACK: the ray could probably be a var inside of Grid
    HexDirection GetDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        Vector3 point = grid.GetRelativeBridgePoint(ray);
        point = grid.GetCell().transform.InverseTransformPoint(point); // assumes cell is valid
        HexMetrics.GetRelativeDirection(point);

        return HexMetrics.GetRelativeDirection(point);
    }

    #endregion
}