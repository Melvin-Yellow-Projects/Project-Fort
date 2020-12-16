/**
 * File Name: UnitDisplay.cs
 * Description: Class in charge of setting color and display data for a unit
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
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
    /********** MARK: Variables **********/
    #region Variables

    [Header("Cached References")]
    [Tooltip("GameObject for displaying a Unit's movement")]
    [SerializeField] GameObject movementDisplay = null;
    [Tooltip("movement text within the movement display")]
    [SerializeField] TMP_Text currentMovementText = null;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    /// Referenece to this display's unit
    /// </summary>
    public Unit MyUnit
    {
        get
        {
            return GetComponent<Unit>();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Toggles the unit's movement display on or off
    /// </summary>
    public void ToggleMovementDisplay()
    {
        movementDisplay.SetActive(!movementDisplay.activeSelf);
    }

    public void HideDisplay()
    {
        movementDisplay.SetActive(false);
    }

    /// <summary>
    /// Refreshes the movement display text
    /// </summary>
    public void RefreshMovementDisplay()
    {
        currentMovementText.text = $"{MyUnit.CurrentMovement}";
    }

    #endregion
}
