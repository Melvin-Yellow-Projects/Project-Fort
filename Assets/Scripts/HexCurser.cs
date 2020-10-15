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

    float alpha = 1f;

    int collisionIndex = -1;

    int count = 0;

    #endregion

    /********** MARK: Public Properties **********/
    #region Public Properties

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

    public bool IsSelected
    {
        set
        {
            alpha = (value) ? 1f : (100f / 255f); 
        }
    }

    public int CollisionIndex
    {
        get
        {
            return collisionIndex;
        }
        set
        {
            collisionIndex = Mathf.Clamp(value, -1, points.Count - 1);
        }
    }

    #endregion

    /********** MARK: Private Properties **********/
    #region Private Properties

    private Vector3 PenultimatePoint
    {
        get
        {
            return points[points.Count - 2];
        }
    }

    private Color DefaultColor
    {
        get
        {
            return new Color(150f / 255f, 0, 0, alpha);
        }
    }

    private Color ErrorColor
    {
        get
        {
            return new Color(150f / 255f, 150f / 255f, 150f / 255f, alpha);
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

        curser.CreateBodies();

        return curser;
    }

    protected void CreateBodies()
    {
        int newCount = count + 10;
        for (int i = count; i < newCount; i++) // HACK: hardcoded body count
        {
            GameObject body = Instantiate<GameObject>(curserBody, transform);
            body.transform.eulerAngles = new Vector3(90, 0, 0);
            body.SetActive(true);
            bodies.Add(body);
            interpolators.Add(0.1f * i);
        }

        count = newCount;
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

        // set color
        if (collisionIndex == -1) curserHead.GetComponent<SpriteRenderer>().color = DefaultColor;
        else curserHead.GetComponent<SpriteRenderer>().color = ErrorColor;
    }

    protected void UpdateBodies()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            // set color
            if (collisionIndex == -1) bodies[i].GetComponent<SpriteRenderer>().color = DefaultColor;
            else bodies[i].GetComponent<SpriteRenderer>().color = ErrorColor;

            // set position
            bodies[i].transform.position = GetBodyPosition(i);

            // update interpolator
            interpolators[i] += Time.deltaTime;

            // reset a body's interpolator if greater than 1
            if (interpolators[i] >= 1) interpolators[i] = 0; 
        }
    }

    #endregion

    /********** MARK: Other Functions **********/
    #region Other Functions

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
        //CreateBodies();
    }

    private Vector3 GetBodyPosition(int interpolatorIndex)
    {
        float interpolator = interpolators[interpolatorIndex];

        // get point index from interpolator
        int pointIndex = InterpolatorToIndex(interpolator);

        // min index interpolator
        float tMin = IndexToInterpolator(pointIndex);

        // max index interpolator
        float tMax = IndexToInterpolator(pointIndex + 1);

        // normalize interpolator with the max and min index range
        float t = GeneralUtilities.Normalization(interpolator, tMin, tMax);

        // calculate points
        Vector3 a = points[pointIndex];
        Vector3 b = points[pointIndex + 1];

        return Vector3.Lerp(a, b, t);
    }

    private float IndexToInterpolator(int index)
    {
        return index / (float)(points.Count - 1);
    }

    private int InterpolatorToIndex(float interpolator)
    {
        return (int)((points.Count - 1) * interpolator);
    }

    #endregion

}
