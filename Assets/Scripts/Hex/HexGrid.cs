/**
 * File Name: HexGrid.cs
 * Description: Script to track the data of a hex map/grid
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 9, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Mirror;

/// <summary>
/// Map/grid of HexCells
/// </summary>
public class HexGrid : NetworkBehaviour
{
    /************************************************************/
    #region Public Variables

    /* Cached References */
    [Header("Cached References")]
    [Tooltip("reference to the HexCell prefab")] // TODO: move this to initializer and make a static hexcell function
    public HexCell cellPrefab;

    [Tooltip("reference to the HexCell Label prefab")] // TODO: move this to initializer and make a static hexcell function
    public Text cellLabelPrefab;

    [Tooltip("reference to the HexGridChunk prefab")]
    public HexGridChunk chunkPrefab;

    /* Settings */
    [Header("Settings")]
    [Tooltip("number of cell in the x direction; effectively width")]
    [SyncVar]
    public int cellCountX = 20;

    [Tooltip("number of cell in the z direction; effectively height")]
    [SyncVar]
    public int cellCountZ = 15;

    [Tooltip("layers to ignore when raycasting")]
    [SerializeField] LayerMask layersToIgnore; // TODO: this would probably be better as a cell layer

    /* Variables */
    public List<Unit> units = new List<Unit>(); // HACK: should not be public

    List<Fort> forts = new List<Fort>();

    #endregion
    /************************************************************/
    #region Private Variables

    /// <summary>
    /// number of chunk columns
    /// </summary>
    [SyncVar] // TODO: verify this works
    private int chunkCountX;

    /// <summary>
    /// number of chunk rows
    /// </summary>
    [SyncVar]
    private int chunkCountZ;

    // references to the grid's chunks and cells
    private HexGridChunk[] chunks;
    private HexCell[] cells;

    HexCellShaderData cellShaderData;

    int unitCount = 0;

    #endregion
    /************************************************************/
    #region Public Properties

    public static HexGrid Prefab { get; set; }

    public static HexGrid Singleton { get; private set; }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void OnDestroy()
    {
        Unsubscribe();

        Singleton = null;
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public static void ServerSpawnMapTerrain()
    {
        Instantiate(Prefab).InitializeMap();

        SaveLoadMenu.LoadMapFromReader();

        if (NetworkServer.localConnection != null)
            NetworkServer.localConnection.identity.GetComponent<HumanPlayer>().HasCreatedMap = true;

        NetworkServer.Spawn(Singleton.gameObject); 
    }

    [Command(ignoreAuthority = true)]
    private void CmdUpdateCellData(int index, NetworkConnectionToClient conn = null)
    {
        // TODO: Validation Logic, can this connection see this cell? if not return
        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();
        GameNetworkManager.Singleton.ServerPlayerHasCreatedMap(player);

        //NetworkConnection conn = GameNetworkManager.HumanPlayers[1].netIdentity.connectionToClient;
        //NetworkConnection conn = playerIdentity.connectionToClient; 

        // HACK: this function could be inside of the HexCell class
        TargetUpdateCellData(conn, HexCellData.Instantiate(cells[index]));
    }

    [Server]
    public static void ServerSpawnMapEntities()
    {
        Debug.Log("Spawning Map Entities");

        for (int i = 0; i < Singleton.units.Count; i++)
        {
            NetworkServer.Spawn(Singleton.units[i].gameObject);
        }
        for (int i = 0; i < Singleton.forts.Count; i++)
        {
            NetworkServer.Spawn(Singleton.forts[i].gameObject);
        }
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [Client]
    public override void OnStartClient()
    {
        // this is needed because the HumanPlayer Script causes errors in the lobby menu if enabled
        for (int i = 0; i < GameNetworkManager.HumanPlayers.Count; i++)
        {
            if (GameNetworkManager.HumanPlayers[i].hasAuthority)
                GameNetworkManager.HumanPlayers[i].enabled = true;
        }
        // HACK: perhaps a static event that logs to clients to enable player is better

        if (!isClientOnly) return;

        InitializeMap();

        Debug.Log("I am a client and I'm fetching the Map!");
        for (int index = 0; index < cells.Length; index++) CmdUpdateCellData(index);
    }

    [TargetRpc]
    private void TargetUpdateCellData(NetworkConnection conn, HexCellData data)
    {
        cells[data.index].Elevation = data.elevation;
        cells[data.index].TerrainTypeIndex = data.terrainTypeIndex;
        cells[data.index].IsExplored = data.isExplored;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void InitializeMap()
    {
        Singleton = this;

        GameSession.Singleton.IsEditorMode = true; // FIXME: this line is for debugging
        if (GameSession.Singleton.IsEditorMode) Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        else Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
        //terrainMaterial.DisableKeyword("GRID_ON");

        cellShaderData = gameObject.AddComponent<HexCellShaderData>();

        CreateMap(cellCountX, cellCountZ);

        Subscribe();
    }

    public bool CreateMap(int x, int z)
    {
        ClearEntities();

        // destroy previous cells and chunks
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                Destroy(chunks[i].gameObject); // TODO: destroy chunks, no?
            }
        }

        // verify valid map; TODO: add support for chunks that are partially filled with cells
        if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
        {
            Debug.LogError("Unsupported map size.");
            return false;
        }

        // initialize number of cells
        cellCountX = x;
        cellCountZ = z;

        // initialize number of chunks
        chunkCountX = cellCountX / HexMetrics.chunkSizeX;
        chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

        // initialize shader data
        cellShaderData.Initialize(cellCountX, cellCountZ);

        CreateChunks(); // create chunks
        CreateCells(); // create cells

        return true;
    }

    /// <summary>
    /// Creates chunks of cells, builds grid row by row
    /// </summary>
    protected void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        int index = 0;
        for (int z = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                chunks[index] = Instantiate(chunkPrefab);
                chunks[index].transform.SetParent(transform);
                index++;
            }
        }
    }

    /// <summary>
    /// Creates each cell in the grid, builds grid row by row
    /// </summary>
    protected void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        int index = 0;
        for (int z = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, index++);
            }
        }
    }

    /// <summary>
    /// Instantiates and properly initializes a HexCell prefab 
    /// </summary>
    /// <param name="x">x hex offset coordinate</param>
    /// <param name="z">z hex offset coordinate</param>
    /// <param name="i">cell's index within array cells</param>
    private void CreateCell(int x, int z, int i)
    {
        // initialize cell position
        Vector3 cellPosition;
        cellPosition.x = x;
        cellPosition.y = 0f;
        cellPosition.z = z;

        // calculate x position
        cellPosition.x += (z / 2f); // offset x by half of z (horizontal shift)
        cellPosition.x -= (z / 2); // undo offset with integer math (this will effect odd rows) 
        cellPosition.x *= (2f * HexMetrics.innerRadius); // multiply by correct z hex metric

        // calculate z position
        cellPosition.z *= (1.5f * HexMetrics.outerRadius);

        // instantiate cell under the grid at its calculated position TODO: rewrite comment
        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cells[i] = cell;
        cell.transform.localPosition = cellPosition;

        // calculate cell's coordinates
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // set cell's shader data
        cell.ShaderData = cellShaderData;

        // set cell index
        cell.Index = i;

        // set if a cell is explorable
        cell.Explorable = x > 0 && z > 0 && x < cellCountX - 1 && z < cellCountZ - 1;

        // set cell name
        cell.name = "hexcell " + i.ToString();

        // instantiate the cell's label under the grid's Canvas TODO: rewrite comment
        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(cellPosition.x, cellPosition.z);

        // set label reference to cell's UI RectTransform
        cell.uiRectTransform = label.rectTransform;

        // set cell's elevation
        cell.Elevation = 0;

        /****** Neighbor Logic *****/

        // assign cell neighbors; skip first column, then connect West cell neighbors
        if (x > 0) cell.SetNeighbor(HexDirection.W, cells[i - 1]);

        // skip first row, then connect remaining cells
        if (z > 0)
        {
            // is z even? then connect even rows cells' Southeast & Southwest neighbors
            if ((z % 2) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]); // (i - width) gets neighbor i
                if (x > 0) cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1) cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
            }
        }

        // add cell to its chunk
        AddCellToChunk(x, z, cell);
    }

    /// <summary>
    /// Adds a cell to its corresponding chunk
    /// </summary>
    /// <param name="x">cell's offset coordinate X</param>
    /// <param name="z">cell's offset coordinate Z</param>
    /// <param name="cell">cell to add to its chunk</param>
    void AddCellToChunk(int x, int z, HexCell cell)
    {
        // gets the corresponding chunk given the offset x and z
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;

        // fetch chunk with chunk index calculation
        int chunkIndex = chunkX + chunkZ * chunkCountX;
        HexGridChunk chunk = chunks[chunkIndex];

        // gets the local index for x and z
        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

        // add the cell to the chunk using the local cell index
        int localCellIndex = localX + localZ * HexMetrics.chunkSizeX;
        chunk.AddCell(localCellIndex, cell);
    }

    public HexCell GetCell(int index)
    {
        return cells[index];
    }

    /// <summary>
    /// Gets the cell within the hex grid given a world position; assumes the position is a legal
    /// position
    /// </summary>
    /// <param name="position">world position to be converted</param>
    /// <returns>a HexCell contained by the grid</returns>
    public HexCell GetCell(Vector3 position)
    {
        // gets the relative local position
        Vector3 localPosition = transform.InverseTransformPoint(position);

        // converts local position into HexCoordinates 
        HexCoordinates coordinates = HexCoordinates.FromPosition(localPosition);

        // get a cell's index from the coordinates
        int index = coordinates.X + (coordinates.Z * cellCountX) + (coordinates.Z / 2);

        // return cell using index
        return cells[index];
    }

    /// <summary>
    /// Gets the corresponding cell given the HexCoordinates
    /// </summary>
    /// <param name="coordinates">a cell's coordinates</param>
    /// <returns>a HexCell</returns>
    public HexCell GetCell(HexCoordinates coordinates)
    {
        // z coordinate validation
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ) return null;

        // x coordinate validation
        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX) return null;

        // gets cell through index calculation
        int index = x + z * cellCountX;
        return cells[index];
    }

    /// <summary>
    /// TODO: comment GetCell and touch up vars
    /// </summary>
    /// <returns></returns>
    public HexCell GetCell(Ray inputRay)
    {
        RaycastHit hit;

        // did we hit anything? then return that HexCell
        if (!Physics.Raycast(inputRay, out hit, 1000, ~layersToIgnore)) return null;

        // draw line for 1 second
        Debug.DrawLine(inputRay.origin, hit.point, Color.white, 1f);

        return GetCell(hit.point);
    }

    public HexCell GetCellUnderMouse()
    {
        return GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    // TODO: comment SetCellLabel
    public void SetCellLabel(int index)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].LabelTypeIndex = index;
        }
    }

    public void ParentTransformToGrid(Transform trans)
    {
        trans.SetParent(transform);
    }

    public void ClearPaths()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Movement.Path.Clear();
        }
    }

    private void ClearEntities()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Die(isPlayingAnimation: false);
        }
        units.Clear();

        for (int i = 0; i < forts.Count; i++)
        {
            Destroy(forts[i].gameObject);
        }
        forts.Clear();
    }

    public void ResetVisibility()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].ResetVisibility();
        }

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];
            UnitPathfinding.IncreaseVisibility(unit.MyCell, unit.Movement.VisionRange);
        }
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    /// <summary>
    /// TODO: comment save
    /// </summary>
    /// <param name="writer"></param>
    public void Save(BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Save(writer);
        }

        writer.Write(forts.Count);
        for (int i = 0; i < forts.Count; i++)
        {
            forts[i].Save(writer);
        }

        writer.Write(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Save(writer);
        }
    }

    /// <summary>
    /// TODO comment load
    /// </summary>
    /// <param name="reader"></param>
    public void Load(BinaryReader mapReader, int header)
    {
        ClearEntities();

        //int x = 20, z = 15; // HACK: <- line not needed
        int x = mapReader.ReadInt32();
        int z = mapReader.ReadInt32();

        // we dont need to make another map if it's the same size as the existing one
        if (x != cellCountX || z != cellCountZ)
        {
            // check if map failed to be created
            if (!CreateMap(x, z)) return;
        }

        bool originalImmediateMode = cellShaderData.ImmediateMode;
        cellShaderData.ImmediateMode = true;

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Load(mapReader, header);
        }
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }
        int fortCount = mapReader.ReadInt32();
        for (int i = 0; i < fortCount; i++)
        {
            Fort.Load(mapReader, header);
        }

        int unitCount = mapReader.ReadInt32();
        for (int i = 0; i < unitCount; i++)
        {
            Unit.Load(mapReader, header);
        }

        cellShaderData.ImmediateMode = originalImmediateMode;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        Fort.OnFortSpawned += HandleOnFortSpawned;
        Fort.OnFortDespawned += HandleOnFortDespawned;

        Unit.OnUnitSpawned += HandleOnUnitSpawned;
        Unit.OnUnitDepawned += HandleOnUnitDepawned;
    }

    private void Unsubscribe()
    {
        Fort.OnFortSpawned -= HandleOnFortSpawned;
        Fort.OnFortDespawned -= HandleOnFortDespawned;

        Unit.OnUnitSpawned -= HandleOnUnitSpawned;
        Unit.OnUnitDepawned -= HandleOnUnitDepawned;
    }

    private void HandleOnFortSpawned(Fort fort)
    {
        // TODO: Server validation

        forts.Add(fort);
    }

    private void HandleOnFortDespawned(Fort fort)
    {
        // TODO: Server validation

        forts.Remove(fort);
    }

    private void HandleOnUnitSpawned(Unit unit)
    {
        // TODO: Server validation

        unit.name = $"Unit {unitCount}";
        unitCount += 1;

        units.Add(unit);
    }

    private void HandleOnUnitDepawned(Unit unit)
    {
        units.Remove(unit);
    }

    #endregion

}
