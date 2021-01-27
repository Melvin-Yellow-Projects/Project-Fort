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
    [Tooltip("whether or not to use a fake loading wheel")]
    [SerializeField] bool isFillProgressFake = false;
    [Tooltip("fake loading progress speed")]
    [SerializeField] float fakeFillSpeed = 1f;

    float interpolator = 0;

    #endregion
    /************************************************************/
    #region Properties

    public static LoadingDisplay Singleton { get; private set; }

    public static bool IsFillProgressFake
    {
        get
        {
            return Singleton.isFillProgressFake;
        }
        set
        {
            Singleton.isFillProgressFake = value;
            if (value) Singleton.enabled = true;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        Singleton = this;
        enabled = isFillProgressFake;
    }

    private void OnEnable()
    {
        interpolator = 0;
    }

    private void Update()
    {
        SetFillProgress(interpolator);

        interpolator += Time.deltaTime * fakeFillSpeed;

        if (interpolator > 1) enabled = false;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static void SetFillProgress(float percent)
    {
        Singleton.fillProgressImage.fillAmount = percent;
    }

    public static void Done()
    {
        Singleton.Done(dummy: false);

        // HACK: for some bizarre reason this doesn't work 
        //Singleton.StartCoroutine(FadeOutAndDestroyDisplay());
    }

    private void Done(bool dummy)
    {
        Singleton.StartCoroutine(FadeOutAndDestroyDisplay());
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
