/**
 * File Name: Fader.cs
 * Description: This class serves as a way to handle images fading in and out
 * 
 * Authors: Will Lacey
 * Date Created: July 22, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as FaderComponent.cs within the Death's Army Project
 *      
 *      HACK: Code repetition
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class Fader : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Settings")]
    [Tooltip("speed in which to fade in or out")]
    [SerializeField, Range(0, 5f)] float fadeSpeed = 1f;

    Image[] images = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private Image[] Images
    {
        get
        {
            if (images == null)
            {
                images = GetComponentsInChildren<Image>();
            }
            return images;
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void FadeIn()
    {
        StopAllCoroutines();

        foreach (Image image in Images)
            StartCoroutine(FadeInCoroutine(image));
    }

    public IEnumerator FadeInCoroutine(Image image)
    {
        // set color alpha to 0
        Color color = image.color;
        image.color = new Color(color.r, color.g, color.b, 0);
        color = image.color;

        // begin fade in process
        while (color.a < 1)
        {
            // yield every frame
            yield return null;

            color.a += Time.deltaTime * fadeSpeed;
            image.color = new Color(color.r, color.g, color.b, color.a);
            color = image.color;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines();

        foreach (Image image in Images)
            StartCoroutine(FadeOutCoroutine(image));
    }

    public IEnumerator FadeOutCoroutine(Image image)
    {
        // set color alpha to 1
        Color color = image.color;
        image.color = new Color(color.r, color.g, color.b, 1);
        color = image.color;

        // begin fade in process
        while (color.a > 0)
        {
            // yield every frame
            yield return null;

            color.a -= Time.deltaTime * fadeSpeed;
            image.color = new Color(color.r, color.g, color.b, color.a);
            color = image.color;
        }
    }

    #endregion
}
