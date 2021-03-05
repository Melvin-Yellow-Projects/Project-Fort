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
    private static int IdAutoIncrement { get; set; }

    /** Configuration Convenience Properties **/
    public PieceConfig Configuration => configuration;
    public PieceType Type => configuration.Type;
    public string PieceTitle => configuration.PieceTitle;
    public int Credits => configuration.Credits;

    public int Id { get; private set; }

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

    #endregion
    /************************************************************/
    #region Piece Flags

    /// <summary>
    /// Not only does this piece have a path, but it's path has been confirmed and it can move
    /// </summary>
    public bool HasMove { get; set; } = false;

    public bool IsActive => Movement.EnRouteCell;

    public bool ForcedActive { get; set; } = false;

    public bool HasCaptured { get; set; } = false;

    public bool HasBonked { get; set; } = false;

    // HACK: if dying removes the collider too earily from a piece, racetime errors occur when 
    //          there is a collision of 3 or more pieces; maybe this can be solved with a 
    //          combat zone collider that would be spawned in place of the dying pieces
    public bool IsDying { get; set; } = false;

    // HACK: when a piece can die but not have racetime errors, remove this to just IsDying
    //          this also relies on the next turn step to set the piece to actually die, otherwise
    //          it waits indefinitely until the next step; this is not noticeable unless the step 
    //          count played is less than the number of moves left for this piece (or other pieces)
    public bool WillDie { get; set; } = false;

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
        Id = IdAutoIncrement++;
        name = $"piece {Id}";
    }

    private void OnDestroy()
    {
        // this forces the removal of this piece's reference from the hex grid
        HexGrid.Pieces.Remove(this);
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
        foreach (PieceType type in Configuration.CaptureTypes)
            if (piece.Configuration.Type == type) return true;
        return false;
    }

    public bool TryToCapturePiece(Piece piece)
    {
        if (CanCapturePiece(piece))
        {
            // HACK: i don't like this, this is only being done because the piece is "in the way"; 
            //          can this be improved? maybe only disable when battle is decisive 
            if (!PieceCollisionHandler.IsCenterCollision(this, piece))
                piece.CollisionHandler.gameObject.SetActive(false); 
            piece.Die(); // TODO: this doesn't create racetime collision errors right?
            //HexGrid.Pieces.Remove(this); 
            HasCaptured = true;
            return true;
        }

        return false;
    }

    public bool CanBlockPiece(Piece piece)
    {
        if (piece.CanCapturePiece(this)) return false;

        if (CanCapturePiece(piece) && IsActive) return false;

        return true;
    }

    public bool TryToBlockPiece(Piece piece)
    {
        bool hasBlocked = CanBlockPiece(piece);
        Debug.Log($"{Type} has {hasBlocked} blocked {piece.Type}");
        if (hasBlocked) piece.Movement.Server_Bonk();

        return hasBlocked;
    }

    public void Die(bool isPlayingAnimation = true)
    {
        // black team HACK: this is to force units to trade off better when colliding
        MyTeam.SetTeam(9); 
        IsDying = true;
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


