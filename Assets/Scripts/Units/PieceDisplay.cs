/**
 * File Name: PieceDisplay.cs
 * Description: Class in charge of setting color and display data for a piece
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as UnitDisplay.cs
 *  
 *      TODO: change authority to show ally info as well
 *      TODO: slowly fade display in and out
 *      TODO: resize display given screen zoom
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Sets color and displays for a piece
/// </summary>
public class PieceDisplay : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [Tooltip("GameObject for displaying a piece's movement")]
    [SerializeField] GameObject movementDisplay = null;
    [Tooltip("movement text within the movement display")]
    [SerializeField] TMP_Text currentMovementText = null;

    Piece piece = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    // HACK: we don't need all three of these functions
    public void ShowDisplay()
    {
        movementDisplay.SetActive(true);
    }

    public void HideDisplay()
    {
        movementDisplay.SetActive(false);
    }

    /// <summary>
    /// Refreshes the movement display text
    /// </summary>
    public void RefreshMovementDisplay(int movement)
    {
        currentMovementText.text = $"{movement}";
    }

    #endregion
}
