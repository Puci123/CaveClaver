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
    private List<List<int>> _outlineVertecies = new List<List<int>>();
    private HashSet<int>    _checkedVertecies = new HashSet<int>();

    private List<int> _triangles = new List<int>();
    private Dictionary<int,List<Triangle>> _hashedTriangles = new Dictionary<int, List<Triangle>>();

    private Mesh _mesh;

    private Material _material;



    private void ConstructTriangle(Vertex a,Vertex b, Vertex c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);

        Triangle triangle = new Triangle(a.VertexIndex,b.VertexIndex,c.VertexIndex);
        
        AddTriangleToHashedMap(a.VertexIndex,triangle);
        AddTriangleToHashedMap(b.VertexIndex,triangle);
        AddTriangleToHashedMap(c.VertexIndex,triangle);

    }

    private void AddTriangleToHashedMap(int key, Triangle tri)
    {
        if(_hashedTriangles.ContainsKey(key))
        {
            _hashedTriangles[key].Add(tri);
            Debug.Log(".");
        } 
        else
        {
            List<Triangle> triList = new List<Triangle>();
            triList.Add(tri);
            _hashedTriangles.Add(key,triList);
        }
    }

    private bool IsOutLine(int a, int b)
    {
        List<Triangle> containingA = _hashedTriangles[a];
        int common = 0;

        foreach (Triangle item in containingA)
        {
            if(item.ContainingVert(b)) common++;
            if(common > 1) break;
        }

        return common == 1;
    }

    private int GetOutLineEdge(int key)
    {
        
        
        List<Triangle> trianglesContaining = _hashedTriangles[key];
        foreach (Triangle triangle in trianglesContaining)
        {
            if(IsOutLine(key,triangle.aIndex) && key != triangle.aIndex && !_checkedVertecies.Contains(triangle.aIndex)) return triangle.aIndex;
            if(IsOutLine(key,triangle.bIndex) && key != triangle.bIndex && !_checkedVertecies.Contains(triangle.bIndex)) return triangle.bIndex;
            if(IsOutLine(key,triangle.cIndex) && key != triangle.cIndex && !_checkedVertecies.Contains(triangle.cIndex)) return triangle.cIndex;

        }
    

        return -1;
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
        _checkedVertecies.Clear();
        _hashedTriangles.Clear();

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
                _cells[x,y] = new Cell(_grid[x,y],_grid[x,y + 1],_grid[x + 1,y],  _grid[x + 1,y + 1], _isoLevel);
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

        CreateWallMesh(3f);
   } 


    private void  FollowOutLine(int vetrexIndex, int outlineIndex)
    {
        _outlineVertecies[outlineIndex].Add(vetrexIndex);
        _checkedVertecies.Add(vetrexIndex);

        int nextVertexIndex = GetOutLineEdge(vetrexIndex);
        if(nextVertexIndex != - 1)
        {
            FollowOutLine(nextVertexIndex,outlineIndex);
        } 

    }

   private void CreateOuttline()
   {
        for (int vetrexIndex = 0; vetrexIndex < _vertices.Count; vetrexIndex++)
        {
            if(!_checkedVertecies.Contains(vetrexIndex))
            {
                int newOutlineVetex = GetOutLineEdge(vetrexIndex);
                if(newOutlineVetex != - 1)
                {
                    _checkedVertecies.Add(vetrexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vetrexIndex);
                    _outlineVertecies.Add(newOutline);

                    FollowOutLine(vetrexIndex, _outlineVertecies.Count - 1);
                    //_outlineVertecies[_outlineVertecies.Count - 1].Add(vetrexIndex);
                }

            }
        }
   }

   public void CreateWallMesh(float wallHeight)
   {
        //_hashedTriangles.Clear();
        //_checkedVertecies.Clear();

        CreateOuttline();

        List<Vector3> wallVertecies = new List<Vector3>();
        List<int> wallTriangle = new List<int>();

        Mesh wallMesh = new Mesh();
        wallMesh.name = "Chunk mesh walls";

        foreach (List<int> outline in _outlineVertecies)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertecies.Count;
                wallVertecies.Add(_vertices[outline[i]]);       //left top
                wallVertecies.Add(_vertices[outline[i + 1]]);   //right top
                wallVertecies.Add(_vertices[outline[i]] - Vector3.forward * wallHeight);       //left bottom
                wallVertecies.Add(_vertices[outline[i + 1]] - Vector3.forward * wallHeight);   //right bottom


                wallTriangle.Add(startIndex + 0);
                wallTriangle.Add(startIndex + 2);
                wallTriangle.Add(startIndex + 3);

                wallTriangle.Add(startIndex + 3);
                wallTriangle.Add(startIndex + 1);
                wallTriangle.Add(startIndex + 0);

            }
        }

        wallMesh.vertices = wallVertecies.ToArray();
        wallMesh.triangles = wallTriangle.ToArray();

        GameObject temp = new GameObject();
        
        temp.transform.parent = transform;
        temp.transform.localPosition = Vector3.zero;
        temp.name = "ChunkWalls";
        temp.AddComponent<MeshFilter>().mesh = wallMesh;
        temp.AddComponent<MeshRenderer>().material = _material;

        Debug.Log(_hashedTriangles.Count);
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


   private struct Triangle
   {
        public int aIndex,bIndex,cIndex;

        public Triangle(int a, int b, int c)
        {
            aIndex = a;
            bIndex = b;
            cIndex = c;

        }

        public bool ContainingVert(int vert)
        {
            return (aIndex == vert || bIndex == vert || cIndex == vert);
        }
   }
}


