/**
 * File Name: Piece.cs
 * Description: Script for managing a piece
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *      
 *      Previously known as Unit.cs & HexUnit.cs
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Mirror;

/// <summary>
/// a piece that is able to interact with a hex map 
/// </summary>
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(ColorSetter))]
[RequireComponent(typeof(PieceDisplay))]
public class Piece : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Settings")]
    [Tooltip("configuration file for this piece")]
    [SerializeField] PieceConfig configuration = null;

    [Tooltip("ID for this piece")]
    [SerializeField] int id = 0;

    [Tooltip("class title name for this piece")]
    [SerializeField] string classTitle = null;

    [Tooltip("piece title name for this piece")]
    [SerializeField] string pieceTitle = null;

    [Tooltip("how much this piece costs")]
    [SerializeField] int credits = 0;

    //[Tooltip("sprite asset for the piece")]
    //[SerializeField] Sprite artwork = null;

    bool isSelected = false;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a piece is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Player">adds piece to player's list of owned units</subscriber>
    /// <subscriber class="Grid">adds unit to list of units</subscriber>
    public static event Action<Piece> OnPieceSpawned;

    /// <summary>
    /// Event for when a unit is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Player">removes unit from player's list of owned units</subscriber>
    /// <subscriber class="Grid">removes unit from list of units</subscriber>
    public static event Action<Piece> OnPieceDespawned;

    #endregion
    /************************************************************/
    #region Properties

    public static List<Piece> Prefabs { get; set; }

    public int Id
    {
        get
        {
            return id;
        }
    }

    public string ClassTitle
    {
        get
        {
            return classTitle;
        }
    }

    public string PieceTitle
    {
        get
        {
            return pieceTitle;
        }
    }

    public int Credits
    {
        get
        {
            return credits;
        }
    }

    public Team MyTeam { get; private set; }

    public ColorSetter MyColorSetter { get; private set; }

    public PieceMovement Movement { get; private set; }

    public PieceCombat CombatHandler { get; private set; }

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            isSelected = value;

            //Movement.RefreshPath(); // shows that the unit is selected or not

            if (value) Movement.ShowMovementRange();
            else Movement.HideMovementRange();
        }
    }

    public HexCell MyCell
    {
        get
        {
            return Movement.MyCell;
        }
        set
        {
            Movement.MyCell = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        MyTeam = GetComponent<Team>();
        MyColorSetter = GetComponent<ColorSetter>();
        Movement = GetComponent<PieceMovement>();
        CombatHandler = GetComponentInChildren<PieceCombat>();

        if (!MyTeam || !MyColorSetter || !Movement || !CombatHandler)
            Debug.LogError($"piece {name} is missing an essential component");

        HexGrid.Pieces.Add(this); // HACK: should this be an event?
        name = $"piece {UnityEngine.Random.Range(0, 100000)}";
    }

    private void OnDestroy()
    {
        HexGrid.Pieces.Remove(this); // HACK: should this be an event?
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        OnPieceSpawned?.Invoke(this);

        ValidateLocation();
    }

    public override void OnStopServer()
    {
        OnPieceDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (!isServer) OnPieceSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isServer) OnPieceDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void ValidateLocation()
    {
        //UnitPathfinding.IncreaseVisibility(MyCell, Movement.VisionRange); // FIXME: vision no work
        if (isServer || !GameSession.IsOnline) transform.localPosition = MyCell.Position;
    }

    public void Die(bool isPlayingAnimation = true)
    {
        MyTeam.SetTeam(9); // black team
        GetComponent<PieceDeath>().Die(isPlayingAnimation);
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        MyCell.coordinates.Save(writer);
        writer.Write((byte)Id);
        writer.Write((byte)MyTeam.Id);
        writer.Write(Movement.Orientation);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);

        Piece piece = Instantiate(Prefabs[reader.ReadByte()]);

        piece.MyTeam.SetTeam(reader.ReadByte());

        piece.Movement.MyCell = HexGrid.Singleton.GetCell(coordinates);
        piece.Movement.Orientation = reader.ReadSingle();

        // HACK: figure out to do with ParentTransformToGrid line (Piece.cs)
        //HexGrid.Singleton.ParentTransformToGrid(piece.transform);

        //NetworkServer.Spawn(piece.gameObject);
    }

    #endregion
}


