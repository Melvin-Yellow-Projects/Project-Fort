﻿/**
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

    public static Material material = null;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("head GameObject of the hex curser")]
    [SerializeField] GameObject curserHead = null;

    Transform bodyTransform;

    List<Vector3> points = new List<Vector3>();

    float errorColor = 0f;

    float alpha = 1f;

    //int collisionIndex = -1;

    /* Configurables */
    float lineWidth = 1f;
    //float deltaT = 0.1f;

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
            Refresh();
        }
    }

    public bool IsSelected
    {
        set
        {
            alpha = (value) ? 1f : (100f / 255f);
        }
    }

    public bool HasError
    {
        set
        {
            errorColor = (value) ? (150f / 255f) : 0;
        }
    }

    //public int CollisionIndex
    //{
    //    get
    //    {
    //        return collisionIndex;
    //    }
    //    set
    //    {
    //        collisionIndex = Mathf.Clamp(value, -1, points.Count - 1);
    //    }
    //}

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

    private Vector3 TailPoint
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

    private Color Color
    {
        get
        {
            return new Color(150f / 255f, errorColor, errorColor, alpha);
        }
    }
    #endregion

    /********** MARK: Initialization Functions **********/
    #region Initialization Functions

    public static HexCurser Initialize(List<Vector3> points)
    {
        if (!prefab || !material) Debug.LogError("HexCurser prefab and material are not found");

        HexCurser curser = Instantiate<HexCurser>(prefab);

        curser.bodyTransform = curser.transform.Find("Body");

        curser.points = points;

        // TODO: i think this line is so the first frame doesn't incorrectly display the head?
        curser.UpdateHead();

        return curser;
    }

    public static HexCurser Initialize(Vector3 tail, Vector3 head)
    {
        if (!prefab || !material) Debug.LogError("HexCurser prefab and material are not found");

        HexCurser curser = Instantiate<HexCurser>(prefab);

        curser.bodyTransform = curser.transform.Find("Body");

        curser.points.Add(tail);

        curser.points.Add(head);

        return curser;
    }

    public void Redraw(List<Vector3> points)
    {
        this.points.Clear();

        this.points = points;

        Refresh();
    }

    #endregion

    /********** MARK: Update Functions **********/
    #region Update Functions

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    protected void Update()
    {
        ClearBody();
        UpdateHead();
        UpdateBody();
        enabled = false;
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
        curserHead.GetComponent<SpriteRenderer>().color = Color;
        //if (collisionIndex == -1) curserHead.GetComponent<SpriteRenderer>().color = DefaultColor;
        //else curserHead.GetComponent<SpriteRenderer>().color = ErrorColor;
    }

    protected void UpdateBody()
    {

        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawLine(points[i], points[i + 1]);
        }
    }

    #endregion

    /********** MARK: Other Functions **********/
    #region Other Functions

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    // HACK: this might now work
    public void RemovePoint(Vector3 point)
    {
        points.Remove(point);
    }

    public void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject myLine = new GameObject();

        myLine.transform.parent = bodyTransform;

        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        material.color = Color;
        lr.material = material;
        //lr.SetColors(Color, Color);
        //lr.startColor = Color;
        //lr.endColor = Color;
        //lr.SetWidth(0.1f, 0.1f);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void ClearBody()
    {
        foreach (Transform child in bodyTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void Refresh()
    {
        enabled = true;
    }

    public void DestroyCurser()
    {
        Destroy(gameObject);
    }

    #endregion

}