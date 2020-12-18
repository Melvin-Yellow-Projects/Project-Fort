using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitDeath : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    [SerializeField] [Range(0, 1f)] float maxJitter = 0.2f;

    //[SerializeField] [Range(0, 5f)] float fallSpeed = 0.5f;
    [SerializeField] [Range(0, 20f)] float fallSpeed = 5f;

    [SerializeField] [Range(0, 5f)] float timeToSurvive = 3f;

    Vector3 originalPosition;

    #endregion

    /********** MARK: Public Variables **********/
    #region Public Variables

    public bool IsDying { get; private set; } = false;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a unit is killed/destroyed
    /// </summary>
    /// <subscriber class="UnitMovement">clears movement data and removes visibility</subscriber>
    public event Action OnDeath;

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void Die(bool isPlayingAnimation = true)
    {
        OnDeath?.Invoke();

        IsDying = true;

        if (isPlayingAnimation) StartCoroutine(DeathAnim());
        else Destroy(gameObject);
    }

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

    private void DisplacementUpdate()
    {
        float jitter = UnityEngine.Random.Range(-maxJitter, maxJitter);

        transform.position = originalPosition + new Vector3(jitter, jitter, jitter);

        originalPosition.y -= fallSpeed * Time.deltaTime;
    }

    #endregion

    /********** MARK: Debug Functions **********/
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
