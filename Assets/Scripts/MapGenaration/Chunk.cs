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
   
    private Vertex[,] _grid;
    private Cell[,] _cells;

    private Vector2Int _gridSize; 
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
  

    private Mesh _mesh;
    private Material _material;



    private void ConstructTriangle(Vertex a,Vertex b, Vertex c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);
    }

    public void SetUp(Vector2 chunkSize, Vector2 cellSize, float isoLevel, LayeredPerlinNoise noise,Material material)
    {
        _chunkSize = chunkSize;
        _cellSize = cellSize;
        _isoLevel = isoLevel;
        _noise = noise;
        _material = material;

        GetComponent<MeshRenderer>().material = material;
    }

    private void AssignVertecies(Vertex[] vert)
    {
        foreach (Vertex item in vert)
        {
            if(item.VertexIndex == -1)
            {
                item.VertexIndex = _vertices.Count;
                _vertices.Add(item.WorldPos);
            }
        }
    }

    public void Points2Mesh(params Vertex[] points)
    {
        
        if(points.Length >= 3)
            ConstructTriangle(points[0],points[1],points[2]);
            
        if(points.Length >= 4)
            ConstructTriangle(points[0],points[2],points[3]);
           
        if(points.Length >= 5)
            ConstructTriangle(points[0],points[3],points[4]);
            
        if(points.Length >= 6)
            ConstructTriangle(points[0],points[4],points[5]);

        
    }


   public void CreateChunk()
   {
        _vertices.Clear();
        _triangles.Clear();
        
        _gridSize.x = Mathf.RoundToInt(_chunkSize.x / _cellSize.x) + 1;
        _gridSize.y = Mathf.RoundToInt(_chunkSize.y / _cellSize.y) + 1;
        Vector3 centerOffset =  -(_chunkSize / 2);

        _grid = new Vertex[_gridSize.x, _gridSize.y];
        _cells = new Cell[_gridSize.x, _gridSize.y];

        //Create grid
        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                _grid[x,y] = new Vertex(new Vector3(x * _cellSize.x, y * _cellSize.y, 0) + centerOffset, 
                                        _noise.GetNoiseValue(x * _cellSize.x + transform.position.x, y * _cellSize.y + transform.position.y));


            }   
        }

        //Triangluate
         for (int y = 0; y < _gridSize.y - 1; y++)
        {
            for (int x = 0; x < _gridSize.x - 1; x++)
            {
              
                _cells[x,y] = new Cell(_grid[x,y+1],_grid[x,y],_grid[x + 1,y + 1],_grid[x+1,y],_isoLevel);
                List<Vertex> vert = _cells[x,y].Triangluate();
            

                if(vert != null)
                { 
                    AssignVertecies(vert.ToArray());
                    Points2Mesh(vert.ToArray());
                }

            }   
        }

        _mesh = new Mesh();
        _mesh.name = "Chunk mesh Top";

        GetComponent<MeshFilter>().mesh = _mesh;

        //Debug.Log(_triangles.Count);

        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();

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
                foreach (Vertex vertex in _grid)
                {
                    if(vertex != null)
                    {
                        Gizmos.color = new Color(vertex.Value,vertex.Value,vertex.Value,1);
                        Gizmos.DrawSphere(vertex.WorldPos,0.1f);
                    }
                } 
            }
        }
   }
}


