/**
 * File Name: UnitDisplay.cs
 * Description: Class in charge of setting color and display data for a unit
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
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
/// Sets color and displays for a Unit
/// </summary>
public class UnitDisplay : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [Tooltip("GameObject for displaying a Unit's movement")]
    [SerializeField] GameObject movementDisplay = null;
    [Tooltip("movement text within the movement display")]
    [SerializeField] TMP_Text currentMovementText = null;

    Unit unit = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    // HACK: we don't need all three of these functions
    public void ShowDisplay()
    {
        if (!unit.hasAuthority) return;
        movementDisplay.SetActive(true);
    }

    public void HideDisplay()
    {
        if (!unit.hasAuthority) return;
        movementDisplay.SetActive(false);
    }

    /// <summary>
    /// Refreshes the movement display text
    /// </summary>
    public void RefreshMovementDisplay(int movement)
    {
        if (!unit.hasAuthority) return;
        currentMovementText.text = $"{movement}";
    }

    #endregion
}
