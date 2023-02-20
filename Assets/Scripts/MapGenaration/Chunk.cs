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

    private Dictionary<int, List<Triangle>> _tringleDicitinery = new Dictionary<int, List<Triangle>>();
    private HashSet<int> checkedVertecies = new HashSet<int>();
    private List<List<int>> _outlines = new List<List<int>>();


    private void ConstructTriangle(Vertex a,Vertex b, Vertex c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);

        Triangle triangle = new Triangle(a.VertexIndex,b.VertexIndex,c.VertexIndex);
        AddTriangleToDictinery(a.VertexIndex,triangle);
        AddTriangleToDictinery(b.VertexIndex,triangle);
        AddTriangleToDictinery(c.VertexIndex,triangle);

    }

    private void AddTriangleToDictinery(int vertexIndexKey, Triangle triangle)
    {
        if(_tringleDicitinery.ContainsKey(vertexIndexKey))
        {
            _tringleDicitinery[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            _tringleDicitinery.Add(vertexIndexKey,triangleList);
        }

    }

    bool IsOutlineEdge(int a,int b)
    {
        List<Triangle> containVertexA = _tringleDicitinery[a];
        int sheredTrianlgeCount = 0;

        for (int i = 0; i < containVertexA.Count; i++)
        {
            if(containVertexA[i].Contains(b))
            {
                sheredTrianlgeCount++;
                if(sheredTrianlgeCount > 1) break;
            }
        }

        return sheredTrianlgeCount == 1;
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContatingVertex = _tringleDicitinery[vertexIndex];

        for (int i = 0; i < trianglesContatingVertex.Count; i++)
        {
            Triangle triangle = trianglesContatingVertex[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if(vertexB != vertexIndex && !checkedVertecies.Contains(vertexB) && IsOutlineEdge(vertexIndex,vertexB))
                    return vertexB;

            }
        }

        return -1;
    }
    
    public void AddVerteciesAssCheckec(params Vertex[] points)
    {
        foreach (Vertex item in points)
        {
            checkedVertecies.Add(item.VertexIndex);
        }
    }

    private void FollowOutiline(int vertexIndex, int outlineIndex)
    {
        _outlines[outlineIndex].Add(vertexIndex);
        checkedVertecies.Add(vertexIndex);

        int newVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if(newVertexIndex != -1) FollowOutiline(newVertexIndex,outlineIndex);
    
    }

    private void CalculetMeshOutilines()
    {
       for (int vertexIndex = 0; vertexIndex < _vertices.Count; vertexIndex++)
       {
            if(!checkedVertecies.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != - 1)
                {
                    checkedVertecies.Add(vertexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    _outlines.Add(newOutline);
                    FollowOutiline(newOutlineVertex,_outlines.Count - 1);
                }
            }
       }
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

    private void CreateCollider()
    {
        CalculetMeshOutilines();
        
        foreach (List<int> outline in _outlines)
        {
            EdgeCollider2D edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = _vertices[outline[i]];
            }

            edgeCollider2D.points = edgePoints;
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
              
                _cells[x,y] = new Cell(_grid[x,y+1],_grid[x,y],_grid[x + 1,y + 1],_grid[x+1,y],_isoLevel,this);
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

        CreateCollider();


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
                        Gizmos.DrawSphere(vertex.WorldPos + transform.position,0.1f);
                    }
                } 
            }
          
            Gizmos.color = Color.cyan;
            if(_outlines != null && _outlines.Count > 0)
            {
                foreach (List<int> list in _outlines)
                {
                    for (int i = 0; i < list.Count - 1; i++)
                    {
                        Gizmos.DrawSphere(_vertices[list[i]] + transform.position,0.1f);
                        Gizmos.DrawLine(_vertices[list[i]] + transform.position, _vertices[list[i + 1]] + transform.position);
                    }
                    
                    Gizmos.DrawSphere(_vertices[list[list.Count - 1]] + transform.position,0.1f);
                }
            }

        }
   }


    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        public int [] vertecies;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertecies = new int[3];
            vertecies[0] = vertexIndexA;
            vertecies[1] = vertexIndexB;
            vertecies[2] = vertexIndexC;

        }

        public int this[int i]
        {
            get{return vertecies[i];}
        }


        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;

        }
    }

}


