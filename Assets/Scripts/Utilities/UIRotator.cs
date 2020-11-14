﻿/**
 * File Name: UIRotator.cs
 * Description: This script simply rotates a UI element about the x and y axis; it is meant to give
 *              a little bit of life to some UI
 * 
 * Authors: Will Lacey
 * Date Created: November 13, 2020
 * 
 * Additional Comments: 
 *      Line length: 100 Characters
 **/

using UnityEngine;

/// <summary>
/// Intended to rotate a UI GameObject
/// </summary>
public class UIRotator : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("max X rotation")]
    [SerializeField] [Range(0, 20f)] float maxRotationX = 0f;

    [Tooltip("max Y rotation")]
    [SerializeField] [Range(0, 20f)] float maxRotationY = 0f;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    private void LateUpdate()
    {
        // converts mouse pos to -1 to 1
        float xMouse = 2 * (Mathf.Clamp01(Input.mousePosition.x / Screen.width) - 0.5f);
        float yMouse = 2 * (Mathf.Clamp01(Input.mousePosition.y / Screen.height) - 0.5f);

        Debug.Log($"mouse x: {xMouse} and mouse y: {yMouse}");

        transform.eulerAngles = new Vector3(xMouse * maxRotationX, yMouse * maxRotationY, 0);

        //transform.eulerAngles += new Vector3(0, Time.deltaTime * rotationSpeed, 0);
    }

    #endregion
}