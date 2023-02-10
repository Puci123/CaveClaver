using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
   [SerializeField] private Vector2 _chunkSize;
   [SerializeField] private Vector2 _cellSize;
   [SerializeField, Range(0,1)] private float _isoLevel = 0.5f;
   [SerializeField] private LayeredPerlinNoise _noise;
   [SerializeField] private bool _drawGizmos = false;
   
   private Cell[,] _grid;
   private Vector2Int _gridSize; 

    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();

    private Mesh _mesh;

    public void SetUp(Vector2 chunkSize, Vector2 cellSize, float isoLevel, LayeredPerlinNoise noise,Material material)
    {
        _chunkSize = chunkSize;
        _cellSize = cellSize;
        _isoLevel = isoLevel;
        _noise = noise;

        GetComponent<MeshRenderer>().material = material;
    }

   public void CreateChunk()
   {
        _vertices.Clear();
        _triangles.Clear();
        _gridSize.x = Mathf.RoundToInt(_chunkSize.x / _cellSize.x) + 1;
        _gridSize.y = Mathf.RoundToInt(_chunkSize.y / _cellSize.y) + 1;
        Vector3 centerOffset =  -(_chunkSize / 2);

        _grid = new Cell[_gridSize.x, _gridSize.y];

        for (int y = 0; y < _gridSize.y - 1; y++)
        {
            for (int x = 0; x < _gridSize.x - 1; x++)
            {
                Vertex ld = new Vertex(new Vector3(x * _cellSize.x, y * _cellSize.y, 0) + centerOffset, _noise.GetNoiseValue(x * _cellSize.x + transform.position.x, y * _cellSize.y + transform.position.y));
                Vertex lu = new Vertex(new Vector3(x * _cellSize.x, (y  + 1)* _cellSize.y, 0) + centerOffset, _noise.GetNoiseValue(x * _cellSize.x+ transform.position.x, (y  + 1)* _cellSize.y+ transform.position.y));
                Vertex rd = new Vertex(new Vector3((x + 1) * _cellSize.x, y * _cellSize.y, 0) + centerOffset, _noise.GetNoiseValue((x + 1) * _cellSize.x+ transform.position.x, y * _cellSize.y+ transform.position.y));
                Vertex ru = new Vertex(new Vector3((x + 1) * _cellSize.x, (y + 1) * _cellSize.y, 0) + centerOffset, _noise.GetNoiseValue((x + 1) * _cellSize.x+ transform.position.x, (y + 1) * _cellSize.y+ transform.position.y));

                _grid[x,y] = new Cell(lu,ld,ru,rd,_isoLevel);
                List<Vector3> temp = _grid[x,y].Triangluate();

                if(temp != null && temp.Count >= 3)
                {
                    foreach (Vector3 item in temp)
                    {
                        _triangles.Add(_vertices.Count);
                        _vertices.Add(item);
                    }
                }

            }   
        }

        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
   } 



   private void Start() 
   {
        CreateChunk();
   }

   private void OnDrawGizmos() 
   {
        if(_drawGizmos)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position,_chunkSize);
            

            if(_grid != null && _grid.Length > 0)
            {
                Gizmos.color = Color.cyan;
                foreach (Cell cell in _grid)
                {
                    if(cell != null)
                    {
                        Gizmos.color = Color.white * cell.LD.Value;
                        Gizmos.DrawSphere(cell.LD.WorldPos + transform.position,0.1f);

                        Gizmos.color = Color.white * cell.LU.Value;
                        Gizmos.DrawSphere(cell.LU.WorldPos + transform.position,0.1f);

                        Gizmos.color = Color.white * cell.RD.Value;
                        Gizmos.DrawSphere(cell.RD.WorldPos+ transform.position,0.1f);

                        Gizmos.color = Color.white * cell.RU.Value;
                        Gizmos.DrawSphere(cell.RU.WorldPos+ transform.position,0.1f);
                    }
                } 
            }
        }
   }
}


