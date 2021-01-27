/**
 * File Name: Fort.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(Team), typeof(ColorSetter))]
public class Fort : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Settings")]
    [Tooltip("speed to highlight buy cells")]
    [SerializeField] float highlightSpeed = 1;

    [SyncVar(hook = nameof(HookOnMyCell))]
    HexCell myCell;

    float orientation;

    float interpolator;
    Color currentColor;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a fort is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Grid">adds the fort to the list of forts on the grid</subscriber>
    public static event Action<Fort> OnFortSpawned;

    /// <summary>
    /// Event for when a fort is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Grid">removes the fort to the list of forts on the grid</subscriber>
    public static event Action<Fort> OnFortDespawned;

    /// <summary>
    /// Server event for when a fort is captured; passes captured fort and new capturing team
    /// </summary>
    /// <subscriber class="Player">updates a player's forts</subscriber>
    public static event Action<Fort, Team> ServerOnFortCaptured;

    #endregion
    /************************************************************/
    #region Properties

    public static Fort Prefab { get; set; }

    public Team MyTeam { get; private set; }

    public HexCell MyCell
    {
        get
        {
            return myCell;
        }
        set
        {
            myCell = value;
            myCell.MyFort = this;

            if (isServer) ValidateLocation();
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public static Color HighlightColor { get; set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        MyTeam = GetComponent<Team>();
    }

    private void Start() // HACK: Start and OnDestroy belong in Server/Client Functions
    {
        OnFortSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        myCell.MyFort = null; // HACK: does this need to be transfered to the server?
        OnFortDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnStartServer()
    {
        Subscribe();
        ValidateLocation();
    }

    [Server]
    public override void OnStopServer()
    {
        Unsubscribe();
    }

    [Server]
    public void ValidateLocation()
    {
        transform.localPosition = myCell.Position;
    }

    #endregion
    /************************************************************/
    #region Client Functions

    #endregion
    /************************************************************/
    #region Class Functions

    public bool IsBuyCell(HexCell cell)
    {
        bool isBuyCell = (cell == MyCell);
        for (HexDirection d = HexDirection.NE; !isBuyCell && d <= HexDirection.NW; d++)
        {
            isBuyCell = (cell == MyCell.GetNeighbor(d));
        }

        return isBuyCell;
    }

    public void ShowBuyCells()
    {
        StopAllCoroutines();

        StartCoroutine(HighlightCellNeighbors());
    }

    public void HideBuyCells()
    {
        StopAllCoroutines();

        StartCoroutine(UnhighlightCellNeighbors());
    }

    // HACK: maybe we could shorted some code here
    private IEnumerator HighlightCellNeighbors()
    {
        currentColor = HighlightColor;
        currentColor.a = 0;
        interpolator = 0;

        while (true)
        {
            while (interpolator < 1)
            {
                MyCell.EnableHighlight(currentColor);
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    MyCell.GetNeighbor(d).EnableHighlight(currentColor);
                }
                currentColor.a = Mathf.Lerp(0, HighlightColor.a, interpolator);
                interpolator += Time.deltaTime * highlightSpeed;
                yield return null;
            }

            while (interpolator > 0)
            {
                MyCell.EnableHighlight(currentColor);
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    MyCell.GetNeighbor(d).EnableHighlight(currentColor);
                }
                currentColor.a = Mathf.Lerp(0, HighlightColor.a, interpolator);
                interpolator -= Time.deltaTime * highlightSpeed;
                yield return null;
            }
        }
    }

    private IEnumerator UnhighlightCellNeighbors()
    {
        while (interpolator > 0)
        {
            MyCell.EnableHighlight(currentColor);
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                MyCell.GetNeighbor(d).EnableHighlight(currentColor);
            }
            currentColor.a = Mathf.Lerp(0, HighlightColor.a, interpolator);
            interpolator -= Time.deltaTime * highlightSpeed;
            yield return null;
        }

        MyCell.DisableHighlight();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            MyCell.GetNeighbor(d).DisableHighlight();
        }
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((byte)MyTeam.TeamIndex);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Fort fort = Instantiate(Prefab);
        if (header >= 4) fort.MyTeam.SetTeam(reader.ReadByte());

        fort.MyCell = HexGrid.Singleton.GetCell(coordinates);
        fort.Orientation = orientation;

        // HACK: figure out to do with ParentTransformToGrid line (Fort.cs)
        //HexGrid.Singleton.ParentTransformToGrid(fort.transform);

        //NetworkServer.Spawn(fort.gameObject);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        if (!isServer) return;

        GameManager.ServerOnStopTurn += HandleServerOnStopTurn;
    }

    private void Unsubscribe()
    {
        if (!isServer) return;

        GameManager.ServerOnStopTurn -= HandleServerOnStopTurn;
    }

    [Server]
    public void HandleServerOnStopTurn()
    {
        Unit unit = myCell.MyUnit;

        if (!unit || MyTeam == unit.MyTeam) return;

        ServerOnFortCaptured?.Invoke(this, unit.MyTeam);

        MyTeam.SetTeam(unit.MyTeam);
    }

    private void HookOnMyCell(HexCell oldValue, HexCell newValue)
    {
        if (myCell) MyCell = myCell;
    }

    #endregion
}
