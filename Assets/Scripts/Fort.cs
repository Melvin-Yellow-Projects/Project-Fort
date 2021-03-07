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
    /// Server event for when a fort is captured; passes captured fort and its previous team
    /// </summary>
    /// <subscriber class="Player">updates a player's forts</subscriber>
    public static event Action<Fort, int> Server_OnFortCaptured;

    #endregion
    /************************************************************/
    #region Properties

    public static Fort Prefab { get; set; }
    private static int IdAutoIncrement { get; set; }

    public int Id { get; private set; }

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

            ValidateLocation();
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

        HexGrid.Forts.Add(this); // HACK: should this be an event?
        Id = IdAutoIncrement++;
        name = $"fort {Id}";
    }

    private void OnDestroy()
    {
        HexGrid.Forts.Remove(this); // HACK: should this be an event?
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        OnFortSpawned?.Invoke(this);

        Subscribe();

        ValidateLocation();
    }

    public override void OnStopServer()
    {
        Unsubscribe();

        myCell.MyFort = null;

        OnFortDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (!isServer) OnFortSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isServer) OnFortDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void ValidateLocation()
    {
        if (isServer || !GameSession.IsOnline) transform.localPosition = myCell.Position;
    }

    public List<HexCell> GetBuyCells()
    {
        List<HexCell> buyCells = new List<HexCell>();

        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            buyCells.Add(MyCell.GetNeighbor(d));
        }

        return buyCells;
    }

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
        HighlightColor = MyTeam.TeamColor * 1.5f;
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
                    MyCell.GetNeighbor(d).EnableHighlight(currentColor, useSmallHighlight: true);
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
                    MyCell.GetNeighbor(d).EnableHighlight(currentColor, useSmallHighlight: true);
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
                MyCell.GetNeighbor(d).EnableHighlight(currentColor, useSmallHighlight: true);
            }
            currentColor.a = Mathf.Lerp(0, HighlightColor.a, interpolator);
            interpolator -= Time.deltaTime * highlightSpeed;
            yield return null;
        }

        MyCell.DisableHighlight();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            MyCell.GetNeighbor(d).DisableHighlight(useSmallHighlight: true);
        }
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((byte)MyTeam.Id);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Fort fort = Instantiate(Prefab);
        fort.MyTeam.SetTeam(reader.ReadByte());

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
        GameManager.Server_OnStopTurn += Server_HandleOnStopTurn;
    }

    private void Unsubscribe()
    {
        GameManager.Server_OnStopTurn -= Server_HandleOnStopTurn;
    }

    [Server]
    public void Server_HandleOnStopTurn()
    {
        Piece unit = myCell.MyPiece;

        if (!unit || MyTeam == unit.MyTeam) return;

        Debug.Log($"Fort {name} was captured by team {unit.MyTeam.Id}");

        int previousTeamId = MyTeam.Id;

        MyTeam.SetTeam(unit.MyTeam);

        Server_OnFortCaptured?.Invoke(this, previousTeamId);
    }

    private void HookOnMyCell(HexCell oldValue, HexCell newValue)
    {
        if (myCell) MyCell = myCell;
    }

    #endregion
}
