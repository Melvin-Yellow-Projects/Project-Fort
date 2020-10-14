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
    /********** MARK: Public Variables **********/
    #region Public Variables

    public static HexCurser prefab = null;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("head GameObject of the hex curser")]
    [SerializeField] GameObject curserHead = null;

    [Tooltip("body GameObject of the hex curser")]
    [SerializeField] GameObject curserBody = null;

    List<GameObject> bodies = new List<GameObject>();
    List<float> interpolators = new List<float>();

    List<Vector3> points = new List<Vector3>();

    // frequency

    float speed = 1f;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public Vector3 HeadPoint
    {
        get
        {
            return points[points.Count - 1];
        }
        set
        {
            value.y = 0;
            points[points.Count - 1] = value;
        }
    }

    private Vector3 PenultimatePoint
    {
        get
        {
            return points[points.Count - 2];
        }
    }

    public Vector3 TailPoint
    {
        get
        {
            return points[0];
        }
        set
        {
            points[0] = value;
        }
    }

    #endregion

    /********** MARK: Initialization Functions **********/
    #region Initialization Functions

    public static HexCurser Initialize(Vector3 tail, Vector3 head)
    {
        HexCurser curser = Instantiate<HexCurser>(prefab);

        curser.points.Add(tail);

        curser.points.Add(head);

        curser.CreateBody();

        return curser;
    }

    protected void CreateBody()
    {
        for (int i = 0; i < 10; i++) // HACK: hardcoded body count
        {
            GameObject body = Instantiate<GameObject>(curserBody, transform);
            body.transform.eulerAngles = new Vector3(90, 0, 0);
            body.SetActive(true);
            bodies.Add(body);
            interpolators.Add(0.1f * i);
        }
    }

    #endregion

    /********** MARK: Update Functions **********/
    #region Update Functions

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    protected void Update()
    {
        UpdateHead();

        UpdateBodies();
    }

    protected void UpdateHead()
    {
        // set position
        curserHead.transform.position = HeadPoint;

        // set rotation
        curserHead.transform.rotation = Quaternion.LookRotation(HeadPoint - PenultimatePoint);
        Vector3 eulerAngles = curserHead.transform.eulerAngles;
        eulerAngles.x = 90;
        curserHead.transform.eulerAngles = eulerAngles;
    }

    protected void UpdateBodies()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            //float t = i / (float)bodies.Count;
            //Vector3 pos = Vector3.Lerp(TailPosition, HeadPosition, t);
            //bodies[i].transform.position = pos;

            bodies[i].transform.position = GetBodyPosition(interpolators[i]);
            interpolators[i] += Time.deltaTime * speed;

            if (interpolators[i] >= 1) interpolators[i] = 0;
        }
    }

    #endregion

    /********** MARK: Other Functions **********/
    #region Other Functions

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    private Vector3 GetBodyPosition(float interpolator)
    {
        float count = points.Count - 1;

        interpolator = Mathf.Clamp(interpolator, 0.001f, 0.999f);

        // calculate index
        int index = (int)(count * interpolator);
        float t1 = index / count;
        float t2 = (index + 1) / count;

        // normalization
        float t = (interpolator - t1) / (t2 - t1);

        // calculate points
        Vector3 a = points[index];
        Vector3 b = points[index + 1];

        return Vector3.Lerp(a, b, t);
    }

    #endregion

}
