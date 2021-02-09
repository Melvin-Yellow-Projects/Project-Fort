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

    [Header("Settings")]
    [Tooltip("max loading progress speed")]
    [SerializeField] float maxFillSpeed = 1f;

    Coroutine fillCoroutine = null;

    #endregion
    /************************************************************/
    #region Properties

    public static LoadingDisplay Singleton { get; private set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        Singleton = this;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static void SetFillProgress(float percent)
    {
        if (Singleton.fillCoroutine != null) Singleton.StopCoroutine(Singleton.fillCoroutine);
        Singleton.fillCoroutine = Singleton.StartCoroutine(Singleton.Fill(percent));
    }

    private IEnumerator Fill(float percent)
    {
        float prevPercent = Singleton.fillProgressImage.fillAmount;
        for (float interpolator = 0; interpolator < 1; interpolator += Time.deltaTime)
        {
            Singleton.fillProgressImage.fillAmount = Mathf.Lerp(prevPercent, percent, interpolator);
            yield return null;
        }
    }

    public static void Done()
    {
        SetFillProgress(1);
        Singleton.StartCoroutine(Singleton.FadeOutAndDestroyDisplay());
    }

    private IEnumerator FadeOutAndDestroyDisplay()
    {
        fader.FadeOut();
        while (fader.IsVisible) yield return null;
        Destroy(gameObject);
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
