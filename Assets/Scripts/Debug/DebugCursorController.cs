/**
 * File Name: DebugCurserController.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: October 14, 2020
 * 
 * Additional Comments: 
 **/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class DebugCursorController : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    PieceCursor currentCurser = null;

    Vector3 point;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    private void Update()
    {
        point = RaycastToWorldPoint(Camera.main.ScreenPointToRay(Input.mousePosition));

        HandleInput();

        if (currentCurser) currentCurser.HeadPoint = point;
    }
    #endregion

    /********** MARK: Initialization & Input **********/
    #region Initialization & Input

    private void HandleInput()
    {
        // left click, new curser
        if (Input.GetMouseButtonDown(0))
        {
            if (currentCurser) currentCurser.IsSelected = false;
            currentCurser = PieceCursor.Initialize(new Vector3(), point);
        }

        // right click, add point
        if (Input.GetMouseButtonDown(1) && currentCurser) currentCurser.AddPoint(point);
    }

    #endregion

    /********** MARK: Utility Functions **********/
    #region Utility Functions

    public Vector3 RaycastToWorldPoint(Ray inputRay)
    {
        RaycastHit hit;

        // did we hit anything? 
        if (Physics.Raycast(inputRay, out hit))
        {
            // draw line for 1 second
            Debug.DrawLine(inputRay.origin, hit.point, Color.white, 1f);

            return hit.point;
        }

        Debug.LogWarning("No hit detected");

        // hit nothing
        return new Vector3();
    }

    #endregion

}
