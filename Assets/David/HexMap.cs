using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexMap : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] [Range(0f, 30f)] float gridSize = 10f;
    [SerializeField] [Range(0f, 5)] float borderSize = 0.1f;
    [SerializeField] [Range(0f, 0.1f)] float eta = 0.001f;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public float GridSize
    {
        get
        {
            return gridSize;
        }
    }

    public float BorderSize
    {
        get
        {
            return borderSize;
        }
    }

    public float Eta
    {
        get
        {
            return eta;
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Update()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Updating Hex Map");

            // offset y for tile height
            Vector3 mapPosition = transform.position;
            mapPosition.y = -(GridSize / 5 + eta);
            transform.position = mapPosition;

            // update all hexes
            foreach (Transform child in transform)
            {
                child.GetComponent<HexSnap>().UpdateHex();
            }
        }
    }

    #endregion
}