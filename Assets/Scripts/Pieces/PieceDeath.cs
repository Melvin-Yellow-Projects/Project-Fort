/**
 * File Name: PieceDeath.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: ???
 * 
 * Additional Comments: 
 *      Previously known as UnitDeath.cs
 * 
 *      TODO: find date created
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// 
/// </summary>
public class PieceDeath : MonoBehaviour
{
    /************************************************************/
    #region Public Variables

    [SerializeField] [Range(0, 1f)] float maxJitter = 0.2f;

    //[SerializeField] [Range(0, 5f)] float fallSpeed = 0.5f;
    [SerializeField] [Range(0, 20f)] float fallSpeed = 5f;

    [SerializeField] [Range(0, 5f)] float timeToSurvive = 3f;

    Vector3 originalPosition;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a piece is killed/destroyed
    /// </summary>
    /// <subscriber class="Player">removes piece from owned pieces</subscriber>
    /// <subscriber class="PieceMovement">clears movement data and removes visibility</subscriber>
    public static event Action<Piece> ServerOnPieceDeath;

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public void Die(bool isPlayingAnimation = true)
    {
        ServerOnPieceDeath?.Invoke(GetComponent<Piece>());

        if (isPlayingAnimation) StartCoroutine(DeathAnim());
        else NetworkServer.Destroy(gameObject);
    }

    [Server]
    private IEnumerator DeathAnim()
    {
        originalPosition = transform.position;

        float timeOfDeath = Time.time + timeToSurvive;
        while (Time.time < timeOfDeath)
        {
            DisplacementUpdate();
            yield return null;
        }
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void DisplacementUpdate()
    {
        float jitter = UnityEngine.Random.Range(-maxJitter, maxJitter);

        transform.position = originalPosition + new Vector3(jitter, jitter, jitter);

        originalPosition.y -= fallSpeed * Time.deltaTime;
    }

    #endregion
    /************************************************************/
    #region Debug Functions

    //private void Update()
    //{
    //    if (Input.GetKey("space")) Die();
    //}

    //private void StopDeath()
    //{
    //    originalPosition.y = 0f;
    //    transform.position = originalPosition;
    //    StopAllCoroutines();
    //}

    #endregion
}
