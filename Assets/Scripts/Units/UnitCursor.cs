/**
 * File Name: UnitCursor.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: October 12, 2020
 * 
 * Additional Comments: 
 *      Previously known as HexCursor.cs
 *      
 *      FIXME: will flash red if it is initialized with error on frame 1
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCursor : MonoBehaviour
{
    /************************************************************/
    #region Private Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("head GameObject of the hex cursor")]
    [SerializeField] GameObject cursorHead = null;

    Transform bodyTransform;

    List<Vector3> points = new List<Vector3>();

    // HACK: this will always be tints of red
    //float color = (150f / 255f);
    float color = 1f;

    float errorColor = 0f;
    
    float maxAlpha = 0.5f;
    float minAlpha = 0.2f;
    float alpha = 0; // set to max alpha in init func

    //int collisionIndex = -1;

    /* Configurables */
    float lineWidth = 1f;
    //float deltaT = 0.1f;

    #endregion
    /************************************************************/
    #region Public Properties

    public static UnitCursor Prefab { get; set; }

    public static Material MyMaterial { get; set; }

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
            alpha = (value) ? maxAlpha : minAlpha;
        }
    }

    public bool HasError
    {
        set
        {
            errorColor = (value) ? color : 0;
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
    /************************************************************/
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
            // HACK this is bad
            //return new Color(color, errorColor, errorColor, alpha);
            return PlayerMenu.MyPlayer.MyTeam.TeamColor;
        }
    }
    #endregion
    /************************************************************/
    #region Initialization Functions

    public static UnitCursor Initialize(List<Vector3> points)
    {
        if (!Prefab || !MyMaterial) Debug.LogError("UnitCursor prefab and material are not found");

        UnitCursor cursor = Instantiate<UnitCursor>(Prefab);

        cursor.bodyTransform = cursor.transform.Find("Body");

        cursor.points = points;

        cursor.alpha = cursor.maxAlpha;

        // TODO: i think this line is so the first frame doesn't incorrectly display the head?
        cursor.UpdateHead();

        return cursor;
    }

    public static UnitCursor Initialize(Vector3 tail, Vector3 head)
    {
        if (!Prefab || !MyMaterial) Debug.LogError("UnitCursor prefab and material are not found");

        UnitCursor cursor = Instantiate<UnitCursor>(Prefab);

        cursor.bodyTransform = cursor.transform.Find("Body");

        cursor.points.Add(tail);

        cursor.points.Add(head);

        return cursor;
    }

    public void Redraw(List<Vector3> points)
    {
        this.points.Clear();

        this.points = points;

        Refresh();
    }

    #endregion
    /************************************************************/
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
        cursorHead.transform.position = HeadPoint;

        // set rotation
        cursorHead.transform.rotation = Quaternion.LookRotation(HeadPoint - PenultimatePoint);
        Vector3 eulerAngles = cursorHead.transform.eulerAngles;
        eulerAngles.x = 90;
        cursorHead.transform.eulerAngles = eulerAngles;

        // set color
        cursorHead.GetComponent<SpriteRenderer>().color = Color;
        //if (collisionIndex == -1) cursorHead.GetComponent<SpriteRenderer>().color = DefaultColor;
        //else cursorHead.GetComponent<SpriteRenderer>().color = ErrorColor;
    }

    protected void UpdateBody()
    {

        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawLine(points[i], points[i + 1]);
        }
    }

    #endregion
    /************************************************************/
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
        //material.color = Color;
        lr.material = MyMaterial;
        //lr.SetColors(Color, Color);
        lr.startColor = Color;
        lr.endColor = Color;
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

    public void DestroyCursor()
    {
        Destroy(gameObject);
    }

    #endregion

}