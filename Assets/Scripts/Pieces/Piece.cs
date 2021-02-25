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

    ///// <summary>
    ///// Event for when a unit is despawned, called in the OnDestroy Method
    ///// </summary>
    ///// <subscriber class="Player">removes unit from player's list of owned units</subscriber>
    ///// <subscriber class="Grid">removes unit from list of units</subscriber>
    //public static event Action<Piece> OnPieceDespawned;

    #endregion
    /************************************************************/
    #region Properties

    public static List<Piece> Prefabs { get; set; }

    /** Configuration Convenience Properties **/
    public PieceConfig Configuration => configuration;
    public PieceType Type => configuration.Type;
    public string PieceTitle => configuration.PieceTitle;
    public int Credits => configuration.Credits;

    public Team MyTeam { get; private set; }

    public ColorSetter MyColorSetter { get; private set; }

    public PieceMovement Movement { get; private set; }

    public PieceCollisionHandler CollisionHandler { get; private set; }

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

    public bool HasCaptured { get; set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        MyTeam = GetComponent<Team>();
        MyColorSetter = GetComponent<ColorSetter>();
        Movement = GetComponent<PieceMovement>();
        CollisionHandler = GetComponentInChildren<PieceCollisionHandler>();

        if (!MyTeam || !MyColorSetter || !Movement || !CollisionHandler)
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
        //OnPieceDespawned?.Invoke(this);
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
        //if (!isServer) OnPieceDespawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void ValidateLocation()
    {
        //UnitPathfinding.IncreaseVisibility(MyCell, Movement.VisionRange); // FIXME: vision no work
        if (isServer || !GameSession.IsOnline) transform.localPosition = MyCell.Position;
    }

    public bool CanCapturePiece(Piece piece)
    {
        // TODO: be absolutely certain this line is needed
        if (piece.GetComponent<PieceDeath>().IsDying) return false; 

        foreach (PieceType type in Configuration.CaptureTypes)
            if (piece.Configuration.Type == type) return true;
        return false;
    }

    public bool TryToCapturePiece(Piece piece)
    {
        if (CanCapturePiece(piece))
        {
            if (PieceCollisionHandler.IsBorderCollision(this, piece))
                piece.CollisionHandler.gameObject.SetActive(false);
            piece.Die();
            HasCaptured = true;
            return true;
        }

        return false;
    }

    public bool TryToBlockPiece(Piece piece)
    {
        if (!piece.CanCapturePiece(this))
        {
            piece.Movement.CancelAction(); // tell piece to bonk
            // TODO: set flag?
            return true;
        }

        return false;
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
        writer.Write((byte)Type);
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


