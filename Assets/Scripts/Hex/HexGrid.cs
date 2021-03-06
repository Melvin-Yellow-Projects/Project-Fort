﻿/**
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

    [Tooltip("layer(s) for the HexGrid Map")]
    [SerializeField] LayerMask mapLayers; 

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

    int fortCount = 0;
    int unitCount = 0;

    int numberOfCellsLoaded = 0;

    #endregion
    /************************************************************/
    #region Public Properties

    public static HexGrid Singleton { get; private set; }

    public static List<Unit> Units { get; private set; } = new List<Unit>();

    public static List<Fort> Forts { get; private set; } = new List<Fort>();

    #endregion
    /************************************************************/
    #region Unity Functions

    private void OnDestroy()
    {
        Debug.LogWarning("HexGrid calling OnDestroy()");

        // HACK brute force clearing
        Units.Clear();
        Forts.Clear();

        Singleton = null;
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStopServer()
    {
        ClearEntities();

        Debug.LogWarning($"number of forts after OnStopServer(): {Forts.Count}");
    }

    [Command(ignoreAuthority = true)]
    private void CmdReadyForMapTerrain(
        BinaryReader mapReader, NetworkConnectionToClient conn = null)
    {
        if (conn.identity.GetComponent<HumanPlayer>().IsReadyForMapData) return;
        conn.identity.GetComponent<HumanPlayer>().IsReadyForMapData = true;

        // TODO: validate whether or not game has begun

        if (mapReader != null)
        {
            SaveLoadMenu.MapReader = mapReader;
            SaveLoadMenu.LoadMapFromReader();
        }

        if (!GameNetworkManager.ServerArePlayersReadyForMapData()) return;
        GameNetworkManager.ServerSetPlayersToNotReadyForMapData();

        HexCellData[] data = new HexCellData[cells.Length];
        for (int i = 0; i < cells.Length; i++) data[i] = HexCellData.Instantiate(cells[i]);

        // HACK: 15 is a hardcoded value -> (because only ~10 bytes per cell)
        int numberOfPackets = GeneralUtilities.RoundUp(data.Length * 15f / 1000f); 
        int packetSize = GeneralUtilities.RoundUp((float) data.Length / numberOfPackets);
        for (int i = 0; i < numberOfPackets; i++)
        {
            List<HexCellData> packet = new List<HexCellData>();
            for (int j = 0; j < packetSize; j++)
            {
                if (i * packetSize + j >= data.Length) break;
                packet.Add(data[i * packetSize + j]);
            }
            // HACK: maybe this should be TargetRpc?
            RpcSpawnMapTerrain(cellCountX, cellCountZ, packet.ToArray()); 
        }
    }

    [Command(ignoreAuthority = true)]
    private void CmdReadyForMapEntities(NetworkConnectionToClient conn = null)
    {
        if (conn.identity.GetComponent<HumanPlayer>().IsReadyForMapData) return;
        conn.identity.GetComponent<HumanPlayer>().IsReadyForMapData = true;

        // TODO: validate whether or not game has begun

        if (!GameNetworkManager.ServerArePlayersReadyForMapData()) return;
        GameNetworkManager.ServerSetPlayersToNotReadyForMapData();

        ServerSpawnMapEntities();
    }

    [Server]
    public static void ServerSpawnMapEntities()
    {
        Debug.Log("Spawning Map Entities");

        for (int i = 0; i < Units.Count; i++)
        {
            NetworkServer.Spawn(Units[i].gameObject,
                ownerConnection: Units[i].MyTeam.AuthoritiveConnection);
        }
        for (int i = 0; i < Forts.Count; i++)
        {
            NetworkServer.Spawn(Forts[i].gameObject,
                ownerConnection: Forts[i].MyTeam.AuthoritiveConnection);
        }

        // TODO: do clients need to send ready message after they attempted to spawn all the units?
        GameManager.Singleton.ServerStartGame();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    // HACK: this function can be improved
    public override void OnStartClient()
    {
        InitializeMap();

        // this is needed because the HumanPlayer Script causes errors in the lobby menu if enabled
        // HACK: perhaps a static event that logs to clients to enable player is better
        if (!SceneLoader.IsGameScene) return;

        NetworkClient.connection.identity.GetComponent<HumanPlayer>().enabled = true;

        CmdReadyForMapTerrain(SaveLoadMenu.MapReader);

        LoadingDisplay.SetFillProgress(0.05f);
    }

    [ClientRpc]
    private void RpcSpawnMapTerrain(int cellCountX, int cellCountZ, HexCellData[] data)
    {
        numberOfCellsLoaded += data.Length;

        if (isServer)
        {
            if (numberOfCellsLoaded == cells.Length) CmdReadyForMapEntities();
            LoadingDisplay.SetFillProgress(0.5f);
            return;
        }

        if (this.cellCountX != cellCountX || this.cellCountZ != cellCountZ)
            CreateMap(cellCountX, cellCountZ);

        int index;
        for (int i = 0; i < data.Length; i++)
        {
            index = data[i].index;
            cells[index].Elevation = data[i].elevation;
            cells[index].TerrainTypeIndex = data[i].terrainTypeIndex;
            cells[index].IsExplored = data[i].isExplored;

            // FIXME: Is this code correct?
            cells[index].ShaderData.RefreshTerrain(cells[index]);
            cells[index].ShaderData.RefreshVisibility(cells[index]);
        }

        float percent = (float)numberOfCellsLoaded / cells.Length;
        LoadingDisplay.SetFillProgress(percent);
        if (numberOfCellsLoaded == cells.Length) CmdReadyForMapEntities();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void InitializeMap()
    {
        Debug.Log("Initializing HexGrid Map");

        Singleton = this;

        if (GameSession.IsEditorMode) Shader.EnableKeyword("MAP_EDITOR_MODE");
        else Shader.DisableKeyword("MAP_EDITOR_MODE");
        //terrainMaterial.DisableKeyword("GRID_ON");
        Shader.SetGlobalFloat("_HexCellSize", HexMetrics.outerRadius);

        cellShaderData = gameObject.AddComponent<HexCellShaderData>();

        CreateMap(cellCountX, cellCountZ);
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
        if (index < 0 || index >= cells.Length) return null; // FIXME: why is this error happening?
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
        if (!Physics.Raycast(inputRay, out hit, 1000, mapLayers)) return null;

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
        for (int i = 0; i < Units.Count; i++)
        {
            Units[i].Movement.Path.Clear();
        }
    }

    private void ClearEntities()
    {
        for (int i = 0; i < Units.Count; i++)
        {
            Destroy(Units[i].gameObject);
        }
        Units.Clear();

        for (int i = 0; i < Forts.Count; i++)
        {
            Destroy(Forts[i].gameObject);
        }
        Forts.Clear();
    }

    public void ResetVisibility()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].ResetVisibility();
        }

        for (int i = 0; i < Units.Count; i++)
        {
            Unit unit = Units[i];
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

        for (int i = 0; i < cells.Length; i++) cells[i].Save(writer);

        writer.Write(Forts.Count);
        for (int i = 0; i < Forts.Count; i++) Forts[i].Save(writer);

        writer.Write(Units.Count);
        for (int i = 0; i < Units.Count; i++) Units[i].Save(writer);
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

}
