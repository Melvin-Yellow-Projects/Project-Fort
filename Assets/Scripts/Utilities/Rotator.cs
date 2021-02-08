/**
 * File Name: Rotator.cs
 * Description: This script simply rotates a GameObject about the yaw axis; it is
 *              meant to display things
 * 
 * Authors: Will Lacey
 * Date Created: August 25, 2020
 * 
 * Additional Comments: 
 *      Line length: 100 Characters
 **/

using UnityEngine;

/// <summary>
/// Rotates a GameObject about the yaw axis
/// </summary>
public class Rotator : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("set rotation to clockwise or counter-clockwise")]
    [SerializeField] bool isRotatingClockwise = false;

    [Tooltip("speed of rotation")]
    [SerializeField] [Range(0, 1000f)] float speed = 0f;

    [Tooltip("set if the base rotation speed should be randomized")]
    [SerializeField] bool isRandomized = false;

    [Tooltip("how much to randomize the base rotation speed by")]
    [SerializeField] [Range(0, 1000f)] float randomness = 0f;

    float direction = 1f;
    float rotationSpeed = 0f;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    private bool IsRotatingClockwise
    {
        set
        {
            if (isRotatingClockwise)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
        }
    }

    private bool IsRandomized
    {
        set
        {
            if (isRandomized)
            {
                rotationSpeed = speed * direction + Random.Range(0, randomness);
            }
            else
            {
                rotationSpeed = speed * direction;
            }
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; This function is called when the script is loaded or a value is changed in the
    /// Inspector (Called in the editor only)
    /// </summary>
    private void OnValidate()
    {
        IsRotatingClockwise = isRotatingClockwise;

        IsRandomized = isRandomized;
    }

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    private void Update()
    {
        transform.eulerAngles += new Vector3(0, Time.deltaTime * rotationSpeed, 0);
    }

    #endregion
}
