/**
 * File Name: LoadingDisplay.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 27, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class LoadingDisplay : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [Tooltip("component to manage this display's fade in and fade out")]
    [SerializeField] Fader fader = null;
    [Tooltip("image that manages the fill progress for the loading display")]
    [SerializeField] Image fillProgressImage = null;

    float t = 0;
    float speed = 1;

    #endregion
    /************************************************************/
    #region Properties

    public static LoadingDisplay Singleton { get; private set; }

    private static Image FillProgressImage
    {
        get
        {
            return Singleton.fillProgressImage;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        SetFillProgress(t);

        t += Time.deltaTime * speed;

        if (t > 1)
        {
            enabled = false;
            Done();
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static void SetFillProgress(float percent)
    {
        FillProgressImage.fillAmount = percent;
    }

    private void Done()
    {
        fader.FadeOut();
    }

    #endregion
    /************************************************************/
    #region Debug

    //private void UpdateTimerDisplay()
    //{
    //    float newProgress = unitTimer / unitSpawnDuration;

    //    if (newProgress < unitProgressImage.fillAmount)
    //    {
    //        unitProgressImage.fillAmount = newProgress;
    //    }
    //    else
    //    {
    //        unitProgressImage.fillAmount = Mathf.SmoothDamp(
    //            unitProgressImage.fillAmount,
    //            newProgress,
    //            ref progressImageVelocity,
    //            0.1f
    //        );
    //    }
    //}

    #endregion
}
