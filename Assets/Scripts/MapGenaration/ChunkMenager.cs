using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChunkManager", menuName = "Manager/Chunk manager", order = 0)]

public class ChunkManager : ScriptableObject 
{
    public Vector2 ViewerPos = Vector2.zero;

    [HideInInspector] public bool NoiseOptionsFoldOut = false;

    [SerializeField] private Vector2 _chunkSize;
    [SerializeField] private Vector2 _cellSize;
    [SerializeField, Range(0,1)] private float _isoLevel = 0.5f;
    [SerializeField] private LayeredPerlinNoise _noise;
    [SerializeField] private Material _groundMaterial;
    [SerializeField] private Material _floorMaterial;

    [SerializeField] private float _viewDistance = 50f;

    private List<GameObject> _chunksVisibleLastFrame = new List<GameObject>();
    private Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
    private Transform _map; 


    public bool NoiseAssigned
    {
        get { return _noise != null; }
    }

    public LayeredPerlinNoise Noise
    {
        get { return _noise; }
    }
    

    public void ClearData()
    {
        foreach (Chunk item in _chunks.Values)
        {
            if(item != null)
                DestroyImmediate(item.gameObject);
        }

        _chunks = new Dictionary<Vector2Int, Chunk>();
        _chunksVisibleLastFrame.Clear();
        ViewerPos = Vector2.zero;


        if(_map != null)
            DestroyImmediate(_map.gameObject);
    }

    public void CreateChunk(Vector3 pos, Vector2Int cord)
    {
        
        GameObject chunk = new GameObject();
        chunk.transform.position = pos;
        chunk.AddComponent<Chunk>().CreateChunk(CreateValueMap(pos),_groundMaterial,_floorMaterial,_cellSize.x);
        chunk.transform.parent = _map;
        chunk.transform.name = "Chunk: " + cord.x + " : " + cord.y;
        
        _chunks.Add(cord,chunk.GetComponent<Chunk>());
        _chunksVisibleLastFrame.Add(_chunks[cord].gameObject);


    }

    public void UpdateVisiableChunks()
    {
        if(_map == null)
        {
            _map = new GameObject("Map").transform;
        }

        foreach (GameObject item in _chunksVisibleLastFrame)
        {
            item.SetActive(false);
        }
        _chunksVisibleLastFrame.Clear();

        int chunksInRange = Mathf.RoundToInt(_viewDistance / _chunkSize.x);
        int currentChunkCordX = Mathf.RoundToInt(ViewerPos.x / _chunkSize.x);
        int currentChunkCordY = Mathf.RoundToInt(ViewerPos.y / _chunkSize.y);

        for (int yOff = -chunksInRange; yOff < chunksInRange; yOff++)
        {
            for (int xOff = -chunksInRange; xOff < chunksInRange; xOff++)
            {
                Vector2Int chunkCord = new Vector2Int(currentChunkCordX + xOff,currentChunkCordY + yOff);    

                if(!_chunks.ContainsKey(chunkCord))
                {
                    CreateChunk(new Vector3(chunkCord.x * (_chunkSize.x - _cellSize.x), 
                                            chunkCord.y * (_chunkSize.y - _cellSize.y), 0),
                                            chunkCord);
                }
                else
                {
                    _chunks[chunkCord].gameObject.SetActive(true);
                    _chunksVisibleLastFrame.Add(_chunks[chunkCord].gameObject);
                }
            }
        }

    }    

    public Chunk.ControlNode GetNodeFromWorldPoint(Vector3 hitPos)
    {
        Vector2Int chunkCord = new Vector2Int( Mathf.RoundToInt(hitPos.x / _chunkSize.x),Mathf.RoundToInt(hitPos.y / _chunkSize.y));
        return _chunks[chunkCord].DigInChunk(hitPos);
    }

    public void RecreateChunkMesh(Vector3 hitPos)
    {
        Vector2Int chunkCord = new Vector2Int( Mathf.RoundToInt(hitPos.x / _chunkSize.x),Mathf.RoundToInt(hitPos.y / _chunkSize.y));
        _chunks[chunkCord].ReconsctructChunkMesh(_groundMaterial,_floorMaterial);
    }

    private int[,] CreateValueMap(Vector3 pos)
    {
        int gridSizeX = Mathf.RoundToInt(_chunkSize.x / _cellSize.x);
        int gridSizeY = Mathf.RoundToInt(_chunkSize.y / _cellSize.y);
        int[,] valueMap = new int[gridSizeX,gridSizeY];


        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                valueMap[x,y] = _noise.GetNoiseValue(pos.x + x * _cellSize.x, pos.y + y * _cellSize.y) > _isoLevel ? 1 : 0;
            }
        }

        return valueMap;

    }

}