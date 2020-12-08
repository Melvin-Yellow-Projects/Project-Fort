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

/// <summary>
/// 
/// </summary>
public class PopupMenu : MonoBehaviour
{
    [SerializeField] Text descriptionText = null;

    public void Open()
    {
        gameObject.SetActive(true);
        MapCamera.Locked = true;
    }

    public void Open(string description)
    {
        gameObject.SetActive(true);
        MapCamera.Locked = true;

        descriptionText.text = description;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        MapCamera.Locked = false;
    }
}
