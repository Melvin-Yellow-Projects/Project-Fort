/**
 * File Name: HexCellLabel.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 9, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class HexCellLabel : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] RectTransform outline;
    [SerializeField] RectTransform pathIndicator;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        // 10f comes from empirical measurements
        outline.sizeDelta = new Vector2(
            outline.sizeDelta.x / 10f * HexMetrics.Configuration.OuterRadius,
            outline.sizeDelta.y / 10f * HexMetrics.Configuration.OuterRadius
        );
        pathIndicator.sizeDelta = new Vector2(
            pathIndicator.sizeDelta.x / 10f * HexMetrics.Configuration.OuterRadius,
            pathIndicator.sizeDelta.y / 10f * HexMetrics.Configuration.OuterRadius
        );
    }

    #endregion
}
