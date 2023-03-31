using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Chunk : MonoBehaviour
{

    private SquareGrid _squareGrid;
   

    private List<Vector3> _vertcies = new List<Vector3>();
    private List<int> _triangles = new List<int>();

    private Dictionary<int,List<Triangle>> _triangleDictionary = new Dictionary<int, List<Triangle>>();    
    private List<List<int>> _outlines = new List<List<int>>();
    private HashSet<int> _checkedVertecies = new HashSet<int>();

    private Transform _chunkTop;

    [SerializeField] private bool _drawGizmos = false;
    // Temporary
    ControlNode _pointingAtTemp;


    public class Node
    {
        public Vector3 _position;
        public int _vertexIndex = -1;

        public Node(Vector3 pos)
        {
            _position = pos;
        }
    }

    public class ControlNode : Node
    {
        public bool _active;
        public Node _above, _right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            _active = active;
            _above = new Node(_position + Vector3.up * squareSize / 2f);
            _right = new Node(_position + Vector3.right * squareSize / 2f);

        }
    }

    public class Square
    {
        public ControlNode _topLeft, _topRight,_bottomRight,_bottomLeft;
        public Node _middleLeft, _middleTop, _middleRight, _middleDown;

        public Square(ControlNode topLeft,ControlNode topRight,ControlNode bottomRight,ControlNode bottomLeft)
        {
            _topLeft = topLeft;
            _topRight = topRight;
            _bottomRight = bottomRight;
            _bottomLeft = bottomLeft;

            _middleTop = _topLeft._right;
            _middleRight = _bottomRight._above;
            _middleDown = _bottomLeft._right;
            _middleLeft = _bottomLeft._above;
        }

        public int GetConfiguration()
        {
            int config = 0;

            if(_topLeft._active) config  += 8;
            if(_topRight._active) config += 4;
            if(_bottomRight._active) config += 2;
            if(_bottomLeft._active) config += 1;

            return config;

        }

        public void ClearNodeIndecies()
        {
            _topLeft._vertexIndex = -1;
            _middleTop._vertexIndex = -1;
            _topRight._vertexIndex = -1;
            _middleRight._vertexIndex  = -1;
            _bottomRight._vertexIndex = -1;
            _middleDown._vertexIndex = -1;
            _bottomLeft._vertexIndex = -1;
            _middleLeft._vertexIndex = -1;

        }
    }

    public class SquareGrid
    {
        public Square[,] _squares;
        public float _squareSize;

        public int GridSizeX
        {
            get { return _squares.GetLength(0); }
        }

        public int gridSizeY
        {
            get { return _squares.GetLength(1); }
        }
        

        public SquareGrid(int [,] map, float squareSize)
        {
            _squareSize = squareSize;

            int gridSizeX = map.GetLength(0);
            int gridSizeY = map.GetLength(1);
            float mapWidth = squareSize * gridSizeX;
            float mapHeight = squareSize * gridSizeY;
            ControlNode[,] controlNodes = new ControlNode[gridSizeX,gridSizeY];

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize /2f, -mapHeight / 2 + y * squareSize + squareSize /2f, 0);
                    controlNodes [x,y] = new ControlNode(pos,map[x,y] == 1,squareSize);    
                }   
            }


            _squares = new Square[gridSizeX - 1, gridSizeY - 1];
            
            for (int x = 0; x < gridSizeX - 1; x++)
            {
                for (int y = 0; y < gridSizeY - 1; y++)
                {
                    _squares[x,y] = new Square(controlNodes[x,y + 1],controlNodes[x + 1, y + 1],controlNodes[x + 1, y], controlNodes[x,y]);  
                }   
            }


        }

        public Square GetSquareFromWorldPoint(Vector3 worldPoint)
        {
            float percentX = Mathf.Clamp01((worldPoint.x + 0.5f * GridSizeX * _squareSize)/ (GridSizeX * _squareSize));
            float percentY = Mathf.Clamp01((worldPoint.y + 0.5f * gridSizeY * _squareSize)/ (gridSizeY * _squareSize)); 
            
            int xCord = Mathf.RoundToInt(percentX * (GridSizeX - 1));
            int yCord = Mathf.RoundToInt(percentY * (gridSizeY - 1));

            return _squares[xCord,yCord];
        }

        public ControlNode GetActiveNodeFromWorldPoint(Vector3 worldPoint)
        {
            Square square = GetSquareFromWorldPoint(worldPoint);
            ControlNode closest = null;
            float distance = Mathf.Infinity;

            if(Vector2.Distance(square._topLeft._position,worldPoint) < distance && square._topLeft._active)
            {
                closest = square._topLeft;
                distance = Vector2.Distance(square._topLeft._position,closest._position);
            }

            if(Vector2.Distance(square._topRight._position,worldPoint) < distance && square._topRight._active)
            {
                closest = square._topRight;
                distance = Vector2.Distance(square._topRight._position,closest._position);
            }

            if(Vector2.Distance(square._bottomRight._position,worldPoint) < distance && square._bottomRight._active)
            {
                closest = square._bottomRight;
                distance = Vector2.Distance(square._bottomRight._position,closest._position);
            }

            if(Vector2.Distance(square._bottomLeft._position,worldPoint) < distance && square._bottomLeft._active)
            {
                closest = square._bottomLeft;
                distance = Vector2.Distance(square._bottomLeft._position,closest._position);
            }


            return closest;
        }
    }

    public struct  Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        private int[] vertecies;


        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertecies = new int[3];
            vertecies[0] = a;
            vertecies[1] = b;
            vertecies[2] = c;


        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }

        public int this[int i]
        {
            get
            {
                return vertecies[i];
            }
        } 

    }

    private void AssigineVertecies(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if(points[i]._vertexIndex == - 1)
            {
                points[i]._vertexIndex = _vertcies.Count;
                _vertcies.Add(points[i]._position);
            }
        }
    }

    private void AddTriangelToDictionary(int key, Triangle triangle)
    {
        if(_triangleDictionary.ContainsKey(key))
        {
            _triangleDictionary[key].Add(triangle);
        }
        else
        {
            List<Triangle> newTriangleList = new List<Triangle>();
            newTriangleList.Add(triangle);
            _triangleDictionary.Add(key,newTriangleList);
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        _triangles.Add(a._vertexIndex);
        _triangles.Add(b._vertexIndex);
        _triangles.Add(c._vertexIndex);

        Triangle triangle = new Triangle(a._vertexIndex,b._vertexIndex,c._vertexIndex);

        AddTriangelToDictionary(a._vertexIndex,triangle);
        AddTriangelToDictionary(b._vertexIndex,triangle);
        AddTriangelToDictionary(c._vertexIndex,triangle);

    }

    private bool IsOutlineEdge(int vertexIndexA, int vertexIndexB)
    {
        List<Triangle> containsVertexA = _triangleDictionary[vertexIndexA];
        int shaheredTriangel = 0;


        for (int i = 0; i < containsVertexA.Count; i++)
        {
            if(containsVertexA[i].Contains(vertexIndexB))
            {
                shaheredTriangel++;
                if(shaheredTriangel > 1) break;
            }

        }

        return shaheredTriangel == 1;
    }

    int GetConectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContaingVertex = _triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContaingVertex.Count; i++)
        {
            Triangle triangle = trianglesContaingVertex[i];
            
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];

                if(vertexIndex != vertexB && !_checkedVertecies.Contains(vertexB))
                {
                    if(IsOutlineEdge(vertexIndex,vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    private void FollowOutline(int vertexIndex,int outlineIndex)
    {
        //Debug.Log(vertexIndex);
        
        _outlines[outlineIndex].Add(vertexIndex);
        _checkedVertecies.Add(vertexIndex);
        
        int nexVertexIndex = GetConectedOutlineVertex(vertexIndex);
        if(nexVertexIndex != -1) FollowOutline(nexVertexIndex,outlineIndex);
    }

    private void CalculateMeshOutline()
    {
        for (int vertexIndex = 0; vertexIndex < _vertcies.Count; vertexIndex++)
        {
            if(!_checkedVertecies.Contains(vertexIndex))
            {
                int newVertexIndex = GetConectedOutlineVertex(vertexIndex);

                if(newVertexIndex != - 1)
                {
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    _checkedVertecies.Add(vertexIndex);
                    _outlines.Add(newOutline);
                    FollowOutline(newVertexIndex,_outlines.Count - 1);
                    newOutline.Add(vertexIndex);
                }

            }
        }
    }

    public void CreateCollider()
    {

        CalculateMeshOutline();

        foreach (List<int> outline in _outlines)
        {
            EdgeCollider2D edgeCollider2D = _chunkTop.gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];
            
            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = _vertcies[outline[i]];    
            }

            edgeCollider2D.points = edgePoints;
        } 

    }


    public void Points2Mesh(params Node[] points)
    {
        AssigineVertecies(points);

        if(points.Length >= 3)
            CreateTriangle(points[0],points[1],points[2]);
        if(points.Length >= 4)
            CreateTriangle(points[0],points[2],points[3]);
        if(points.Length >= 5)
            CreateTriangle(points[0],points[3],points[4]);
        if(points.Length >= 6)
            CreateTriangle(points[0],points[4],points[5]);
    }   

    public void TriangulateSquare(Square square)
    {
        switch (square.GetConfiguration()) 
        {
		case 0:
			break;

		// 1 points:
		case 1:
			Points2Mesh(square._middleLeft, square._middleDown, square._bottomLeft);
			break;
		case 2:
			Points2Mesh(square._bottomRight, square._middleDown, square._middleRight);
			break;
		case 4:
			Points2Mesh(square._topRight, square._middleRight, square._middleTop);
			break;
		case 8:
			Points2Mesh(square._topLeft, square. _middleTop, square._middleLeft);
			break;

		// 2 points:
		case 3:
			Points2Mesh(square. _middleRight, square._bottomRight, square._bottomLeft, square._middleLeft);
			break;
		case 6:
			Points2Mesh(square. _middleTop, square._topRight, square._bottomRight, square._middleDown);
			break;
		case 9:
			Points2Mesh(square._topLeft, square. _middleTop, square._middleDown, square._bottomLeft);
			break;
		case 12:
			Points2Mesh(square._topLeft, square._topRight, square. _middleRight, square._middleLeft);
			break;
		case 5:
			Points2Mesh(square. _middleTop, square._topRight, square. _middleRight, square._middleDown, square._bottomLeft, square._middleLeft);
			break;
		case 10:
			Points2Mesh(square._topLeft, square. _middleTop, square. _middleRight, square._bottomRight, square._middleDown, square._middleLeft);
			break;

		// 3 point:
		case 7:
			Points2Mesh(square. _middleTop, square._topRight, square._bottomRight, square._bottomLeft, square._middleLeft);
			break;
		case 11:
			Points2Mesh(square._topLeft, square. _middleTop, square. _middleRight, square._bottomRight, square._bottomLeft);
			break;
		case 13:
			Points2Mesh(square._topLeft, square._topRight, square. _middleRight, square._middleDown, square._bottomLeft);
			break;
		case 14:
			Points2Mesh(square._topLeft, square._topRight, square._bottomRight, square._middleDown, square._middleLeft);
			break;

		// 4 point:
		case 15:
			Points2Mesh(square._topLeft, square._topRight, square._bottomRight, square._bottomLeft);
			break;
		}
    }

   public void CreateMesh(Material material)
   {

        _triangles = new List<int>();
        _vertcies = new List<Vector3>();

        for (int y = 0; y < _squareGrid.gridSizeY; y++)
        {
            for (int x = 0; x < _squareGrid.GridSizeX; x++)
            {
                TriangulateSquare(_squareGrid._squares[x,y]);
            }            
        }

        Mesh mesh = new Mesh();
        mesh.name = "Chunk mesh top";
        
       Debug.Log("Created mesh with: " + _vertcies.Count + " vertecies and " + _triangles.Count + "triangles");

        _chunkTop.gameObject.AddComponent<MeshFilter>().mesh = mesh;
        _chunkTop.gameObject.AddComponent<MeshRenderer>().material = material;

        mesh.vertices = _vertcies.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.RecalculateNormals();
   }

    public void CreateFloor(Material floorMaterial)
    {
        Mesh floorMesh = new Mesh();
        floorMesh.name = "floor mesh";

        Vector3[] floorVertecies = new Vector3[4];
        int[] floorTriangles = {0,3,1,3,2,1};

        floorVertecies[0] = _squareGrid._squares[0,0]._bottomLeft._position;
        floorVertecies[1] = _squareGrid._squares[_squareGrid.GridSizeX - 1,0]._bottomRight._position;
        floorVertecies[2] = _squareGrid._squares[_squareGrid.GridSizeX - 1,_squareGrid.gridSizeY - 1]._topRight._position;
        floorVertecies[3] = _squareGrid._squares[0,_squareGrid.gridSizeY - 1]._topLeft._position;

        floorMesh.vertices  = floorVertecies;
        floorMesh.triangles = floorTriangles;
        floorMesh.RecalculateNormals();

        GameObject floor = new GameObject("Chunk floor");
        floor.transform.parent = transform;
        floor.transform.localPosition = new Vector3(0,0,10);
        floor.AddComponent<MeshFilter>().mesh = floorMesh;
        floor.AddComponent<MeshRenderer>().material = floorMaterial;

    }

    public void CreateChunk(int [,] valueMap,Material topMaterial,Material floorMaterial,float squareSize)
    {
        if(transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        _squareGrid = new SquareGrid(valueMap,squareSize);
        _checkedVertecies.Clear();
        _outlines.Clear();
        _triangleDictionary.Clear();

        _chunkTop = new GameObject("Chunk top").transform;
        _chunkTop.parent = transform;
        _chunkTop.localPosition = Vector3.zero;

        CreateMesh(topMaterial);
        CreateFloor(floorMaterial);
        CreateCollider();

        Debug.Log("Chunk created");

    }

    public ControlNode DigInChunk(Vector3 hitPos)
    {
      _pointingAtTemp = _squareGrid.GetActiveNodeFromWorldPoint(transform.InverseTransformPoint(hitPos));
      return _pointingAtTemp;
    }

    public void ReconsctructChunkMesh(Material topMaterial, Material floorMaterial)
    {
        foreach (Transform child in transform)
        {
                Destroy(child.gameObject);
        }

        _chunkTop = new GameObject("Chunk top").transform;
        _chunkTop.parent = transform;
        _chunkTop.localPosition = Vector3.zero;

        foreach (Square square in _squareGrid._squares)
        {
            square.ClearNodeIndecies();
        }

        _outlines.Clear();
        _triangleDictionary.Clear();
        _checkedVertecies.Clear();

        CreateMesh(topMaterial);
        CreateFloor(floorMaterial);
        CreateCollider();

    }

    private void OnDrawGizmos() 
    {

        Gizmos.color = Color.magenta;
        if(_pointingAtTemp != null)
            Gizmos.DrawCube(_pointingAtTemp._position + transform.position, Vector3.one * 0.2f);

        if(_drawGizmos)
        {
            
            if(_squareGrid != null)
            {
                for (int y = 0; y < _squareGrid.gridSizeY; y++)
                {
                    for (int x = 0; x < _squareGrid.GridSizeX; x++)
                    {
                        Gizmos.color = _squareGrid._squares[x,y]._topLeft._active ? Color.white : Color.black;
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._topLeft._position + transform.position, 0.1f * Vector3.one);
                        
                        Gizmos.color = _squareGrid._squares[x,y]._topRight._active ? Color.white : Color.black;
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._topRight._position + transform.position, 0.1f * Vector3.one);

                        Gizmos.color = _squareGrid._squares[x,y]._bottomRight._active ? Color.white : Color.black;
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._bottomRight._position + transform.position, 0.1f * Vector3.one);

                        Gizmos.color = _squareGrid._squares[x,y]._bottomLeft._active ? Color.white : Color.black;
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._bottomLeft._position + transform.position, 0.1f * Vector3.one);

                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._middleTop._position + transform.position, 0.05f * Vector3.one);
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._middleRight._position + transform.position, 0.05f * Vector3.one);
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._middleDown._position + transform.position, 0.05f * Vector3.one);
                        Gizmos.DrawCube(_squareGrid._squares[x,y]._middleLeft._position + transform.position, 0.05f * Vector3.one);
                    
                    }
                }
            
            }

            if(_outlines != null)
            {
                Gizmos.color = Color.red;
                foreach (List<int> triangles in _outlines)
                {
                   for (int i = 0; i < triangles.Count - 1; i++)
                   {
                        Gizmos.DrawCube(_vertcies[triangles[i]] + transform.position,0.05f * Vector3.one);
                        Gizmos.DrawLine(_vertcies[triangles[i]] + transform.position,_vertcies[triangles[i + 1]] + transform.position);
                   }
                }
            }    
        }
    }


}


