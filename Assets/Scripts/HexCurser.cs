/**
 * File Name: HexCurser.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: October 12, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCurser : MonoBehaviour
{

    public Vector3 headPosition;

    public Vector3 tailPosition;

    /********** MARK: Private Variables **********/
    #region Private Variables

    [Header("Cached References")]
    [Tooltip("head GameObject of the hex curser")]
    [SerializeField] GameObject curserHead;

    [Tooltip("body GameObject of the hex curser")]
    [SerializeField] GameObject curserBody;

    List<GameObject> nodes = new List<GameObject>();

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        SpawnCurserNodes();

        headPosition = new Vector3(60, 0, 0);
        tailPosition = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    protected void Update()
    {
        SetBodyPosition();

        curserHead.transform.position = headPosition;
        SetHeadRotation();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    protected void SpawnCurserNodes()
    {
        for (int i = 0; i < 10; i++) // HACK: hardcoded body count
        {
            GameObject node = Instantiate<GameObject>(curserBody, transform);
            node.transform.eulerAngles = new Vector3(90, 0, 0);
            node.SetActive(true);
            nodes.Add(node);
        }
    }

    protected void SetBodyPosition()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            float t = i / (float)nodes.Count;
            Vector3 pos = Vector3.Lerp(tailPosition, headPosition, t);
            nodes[i].transform.position = pos;
        }
    }

    protected void SetHeadRotation()
    {
        curserHead.transform.rotation = Quaternion.LookRotation(headPosition - tailPosition);
        curserHead.transform.eulerAngles += new Vector3(90, 0, 0);
    }

    #endregion

}
