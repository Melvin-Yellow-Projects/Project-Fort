/**
 * File Name: PopupMenu.cs
 * Description: TODO: comment script
 * 
 * Authors: Will Lacey
 * Date Created: November 14, 2020
 * 
 * Additional Comments: 
 *      Line length: 100 Characters
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 
/// </summary>
public class PopupMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] TMP_Text title = null;
    [SerializeField] TMP_Text description = null;
    [SerializeField] Button confirmationButton = null;

    #endregion
    /************************************************************/
    #region Variables

    public static PopupMenu Prefab { get; set; }

    public static PopupMenu Singleton { get; set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void OnDestroy()
    {
        Singleton = null;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static void Open(string title, string description,
        bool isConfirmationPopup = false, UnityEngine.Events.UnityAction func = null)
    {
        Singleton = Instantiate(Prefab);

        Singleton.title.text = title;

        Singleton.description.text = description;

        Singleton.confirmationButton.gameObject.SetActive(isConfirmationPopup);

        Singleton.confirmationButton.onClick.AddListener(func);

        MapCamera.Locked = true;
    }

    public static void Close()
    {
        MapCamera.Locked = false;

        Singleton.confirmationButton.onClick.RemoveAllListeners();

        Destroy(Singleton.gameObject);
    }

    #endregion
}
