﻿/**
 * File Name: DebugComments.cs
 * Description: This script is to serve as a placeholder for useful comments
 * 
 * Authors: XXXX [Youtube Channel], Will Lacey
 * Date Created: August 18, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found on XXXX YouTube channel under the video: 
 *      "yyyy"; updated it to better fit project
 * 
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugComments : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Header("Cached References Setup")]
    [Tooltip("this is my variable description")]
    [SerializeField] [Range(0f, 1f)] protected float val = 0f;

    [Header("Settings")]
    [Tooltip("this is my variable description")]
    [SerializeField] [Range(0f, 1f)] protected float val2 = 0f;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    ///     Description of MyVal
    /// </summary>
    public float MyVal
    {
        get
        {
            return val;
        }
        set
        {
            val = value;
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    ///     Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; Start() is called before the first frame update
    /// </summary>
    protected void Start()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; This function is called when the script is loaded or a value is changed in the Inspector
    ///         (Called in the editor only)
    /// </summary>
    protected void OnValidate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; Called every frame while the mouse is over the Collider
    /// </summary>
    protected void OnMouseOver()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; Called when the mouse is not any longer over the Collider
    /// </summary>
    protected void OnMouseExit()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; Update() is called once per frame
    /// </summary>
    private void Update()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Unity Method; OnTriggerEnter() is called in FixedUpdate() when a GameObject collides with another GameObject; The Colliders
    ///         involved are not always at the point of initial contact
    /// </summary>
    /// <param name="otherCollider">other GameObject that the collision has occured with</param>
    private void OnTriggerEnter(Collider otherCollider)
    {
        throw new NotImplementedException();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    #endregion

    }
