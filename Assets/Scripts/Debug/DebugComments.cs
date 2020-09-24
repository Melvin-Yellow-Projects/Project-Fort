/**
 * File Name: DebugComments.cs
 * Description: This script is to serve as a placeholder for useful comments
 * 
 * Authors: XXXX [Youtube Channel], Will Lacey
 * Date Created: August 18, 2020
 * Last Updated: September 21, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found on XXXX YouTube channel under the video: 
 *      "yyyy"; updated it to better fit project
 *
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      Comment length: 100 Characters
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template class description goes here
/// </summary>
public class DebugComments : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("this is my variable description")]
    [SerializeField] [Range(0f, 1f)] protected float val = 0f;

    /* General Settings */
    [Header("General Settings")]
    [Tooltip("this is my variable description")]
    [SerializeField] [Range(0f, 1f)] protected float val2 = 0f;

    /* Private & Protected Variables */
    /// <summary>
    /// This is a description for this confusing variable
    /// </summary>
    //protected float val3 = 0f;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    /// <summary>
    /// Description of MyVal
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
    /// Unity Method; This function is called when the script is loaded or a value is changed in the
    /// Inspector (Called in the editor only)
    /// </summary>
    protected void OnValidate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    protected void OnEnable()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Start() is called before the first frame update
    /// </summary>
    protected void Start()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Called every frame while the mouse is over the Collider
    /// </summary>
    protected void OnMouseOver()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Called when the mouse is not any longer over the Collider
    /// </summary>
    protected void OnMouseExit()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Frame-rate independent method for physics calculations
    /// </summary>
    protected void FixedUpdate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    protected void Update()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    protected void LateUpdate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unity Method; OnTriggerEnter() is called in FixedUpdate() when a GameObject collides with
    /// another GameObject; The Colliders involved are not always at the point of initial contact
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