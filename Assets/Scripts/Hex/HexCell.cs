/**
 * File Name: HexCell.cs
 * Description: Class for a specific hex cell or tile
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

public class HexCell : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    // a cell's coordinates
    public HexCoordinates coordinates;

    // a cell's color
    public Color color;

    // some cell neighbors
    [SerializeField] HexCell[] neighbors;

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    // TODO: Comment Function 
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    // TODO: Comment Function
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    #endregion
}
