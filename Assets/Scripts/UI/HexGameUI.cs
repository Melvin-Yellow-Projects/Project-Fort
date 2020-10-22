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

        // get new path
        if (cell != currentCell)
        {
            currentCell = cell;
            if (selectedUnit.IsValidDestination(currentCell))
            {
                HexPath path = HexPathfinding.FindPath(selectedUnit.MyCell, currentCell, selectedUnit);
                selectedUnit.Path = path;
                if (selectedUnit.HasPath) selectedUnit.Path.Show(selectedUnit.Speed);

                //if (selectedUnit.HasPath) selectedUnit.Path.LogPath();
            }
        }
        else if (cell == currentCell) // get end path direction
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 point = grid.GetRelativeBridgePoint(ray);
            point = cell.transform.InverseTransformPoint(point);
            HexMetrics.GetRelativeDirection(point);

            selectedDirection = HexMetrics.GetRelativeDirection(point);
        }
    }

    void DoMove()
    {
        if (selectedUnit.HasPath)
        {
            selectedUnit.Travel();

            Debug.Log(selectedDirection);
        }
        else
        {
            selectedUnit.LookAt(selectedDirection);
        }
    }

    #endregion
}