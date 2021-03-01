/**
 * File Name: HexSnap.cs
 * Description: Snaps a Hex to a set coordinate
 * 
 * Authors: Will Lacey
 * Date Created: August 6, 2020
 * 
 * Additional Comments:
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexSnap : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    protected HexMap myHexMap = null;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public float HexBase
    {
        get
        {
            return myHexMap.GridSize;
        }
    }

    public float HexWidth
    {
        get
        {
            return 2 * myHexMap.GridSize / Mathf.Sqrt(3);
        }
    }

    public float HexHeight
    {
        get
        {
            return myHexMap.GridSize / 5; // TODO: is this right?
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    protected void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Updating Hex: " + name);
            myHexMap = GetComponentInParent<HexMap>();
            if (myHexMap != null) UpdateHex();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void UpdateHex()
    {
        AdjustRotation();
        AdjustPosition();
        AdjustScale();
    }

    protected void AdjustRotation()
    {
        Vector3 snapRotation = transform.localEulerAngles;

        // rotate hex by one side
        snapRotation.y = Mathf.RoundToInt(transform.localEulerAngles.y / 60) * 60;

        transform.localEulerAngles = snapRotation;
    }

    protected void AdjustPosition()
    {
        Vector3 snapPosition;

        float xUnit = 0.75f * HexWidth;
        float zUnit = 0.5f * HexBase;

        // snaps pos to x/z grid
        snapPosition.x = Mathf.RoundToInt(transform.localPosition.x / xUnit) * xUnit;
        snapPosition.z = Mathf.RoundToInt(transform.localPosition.z / zUnit) * zUnit;

        // add offset to x/z coord
        bool xIsOffset = (IsAlmostEqual(Mathf.Abs(snapPosition.x) % (2 * xUnit), xUnit));
        bool zIsOffset = (IsAlmostEqual(Mathf.Abs(snapPosition.z) % (2 * zUnit), zUnit));

        // add offset
        if (xIsOffset && !zIsOffset) snapPosition.x += xUnit;
        else if (!xIsOffset && zIsOffset) snapPosition.z += zUnit;

        snapPosition.y = Mathf.RoundToInt(transform.localPosition.y / HexHeight) * HexHeight;

        transform.localPosition = snapPosition;
    }

    protected void AdjustScale()
    {
        Vector3 localScale = transform.localScale;

        localScale.x = myHexMap.GridSize - myHexMap.BorderSize;
        localScale.y = myHexMap.GridSize;
        localScale.z = myHexMap.GridSize - myHexMap.BorderSize;

        transform.localScale = localScale;
    }

    protected static bool IsAlmostEqual(float float1, float float2, float eta = 0.001f)
    {
        return Mathf.Abs(float1 - float2) < eta;
    }

    #endregion
}