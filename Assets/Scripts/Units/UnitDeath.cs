using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class UnitDeath : MonoBehaviour
{
    /************************************************************/
    #region Public Variables

    [SerializeField] [Range(0, 1f)] float maxJitter = 0.2f;

    //[SerializeField] [Range(0, 5f)] float fallSpeed = 0.5f;
    [SerializeField] [Range(0, 20f)] float fallSpeed = 5f;

    [SerializeField] [Range(0, 5f)] float timeToSurvive = 3f;

    Vector3 originalPosition;

    #endregion
    /********** MARK: Public Variables **********/
    #region Public Variables

    public bool IsDying { get; [Server] private set; } = false;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a unit is killed/destroyed
    /// </summary>
    /// <subscriber class="UnitMovement">clears movement data and removes visibility</subscriber>
    public event Action ServerOnDeath;

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public void Die(bool isPlayingAnimation = true)
    {
        ServerOnDeath?.Invoke();

        IsDying = true;

        if (isPlayingAnimation) StartCoroutine(DeathAnim());
        else Destroy(gameObject);
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
        Destroy(gameObject);
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
