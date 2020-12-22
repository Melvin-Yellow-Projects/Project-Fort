/**
 * File Name: Fort.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 
/// </summary>
public class EllipsisSetter : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Header("Settings")]
    [Tooltip("does the text move when adding ellipses (font characters need to have same size)")]
    [SerializeField] bool isLockedInPlace = true;

    [Tooltip("wait time to change the ellipsis")]
    [SerializeField, Range(0, 2f)] float waitTime = 1f;

    [Tooltip("wait time to reset the ellipsis")]
    [SerializeField, Range(0, 2f)] float resetTime = 1f;

    TMP_Text tmpText = null;
    string originalText;

    #endregion


    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        originalText = tmpText.text;
    }

    private void OnEnable()
    {
        StartCoroutine(AddEllipses());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    #endregion

    /********** MARK: Classa Functions **********/
    #region Class Functions

    private IEnumerator AddEllipses()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(waitTime);
                if (isLockedInPlace) tmpText.text = " " + tmpText.text;
                tmpText.text += "."; 
            }
            yield return new WaitForSeconds(resetTime);

            tmpText.text = originalText;
        }
    }
   
    #endregion
}
