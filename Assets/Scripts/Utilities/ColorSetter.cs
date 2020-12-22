/**
 * File Name: ColorSetter.cs
 * Description: Class in charge of setting the color for a GameObject
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSetter : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Header("Settings")]
    [Tooltip("speed in which to change the unit's color")]
    [SerializeField, Range(0, 10f)] float changeColorSpeed = 0.1f;

    [Tooltip("percentage to saturate the color when saturation is toggled")]
    [SerializeField, Range(0, 1f)] float saturation = 0.3f;

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Sets the color of all the materials under this GameObject
    /// </summary>
    /// <param name="color"></param>
    /// <param name="isSaturating">whether or not this color is getting saturated</param>
    public void SetColor(Color color, bool isSaturating = false)
    {
        StopAllCoroutines();

        if (isSaturating) color *= saturation;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            StartCoroutine(SetColor(renderer.material, color));
        }
    }

    /// <summary>
    /// Coroutine for setting a material's color
    /// </summary>
    /// <param name="material">material to change the color of</param>
    /// <param name="color">what color to change the material to</param>
    /// <returns>yields every frame</returns>
    private IEnumerator SetColor(Material material, Color color)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime * changeColorSpeed)
        {
            material.color = Color.Lerp(material.color, color, t);
            yield return null;
        }
    }

    #endregion
}
