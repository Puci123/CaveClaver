using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfViwie : MonoBehaviour
{
   
    [SerializeField,Range(0,360)] private float _viewAngle = 180f;
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField] private LayerMask _blockLight;
    [SerializeField] private float _resolution = 0.25f;
    [SerializeField] private MeshFilter _meshFilter;

    private Mesh _mesh;

    public float ViewRadius
    {
        get { return _viewRadius; }
    }

     public float ViewAngle
    {
        get { return _viewAngle; }
    }
    
    
    public Vector3 Angle2Dir(float angle,bool isGlobal)
    {
        if(!isGlobal)
            angle += transform.eulerAngles.z;
        
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),Mathf.Sin(angle * Mathf.Deg2Rad),0).normalized;
    }


    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = Angle2Dir(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position,dir,_viewRadius,_blockLight);
        
        if(hit.collider != null)
        {
            return new ViewCastInfo(true,hit.point,hit.distance,globalAngle);
        }

        return new ViewCastInfo(false,transform.position + _viewRadius * dir,_viewRadius,globalAngle);
    }

    private void DrawFOW()
    {
        List<Vector3> vivePoints = new List<Vector3>();
        int numberOfRays = Mathf.RoundToInt(_resolution * ViewAngle);
        float stepAngleSize = ViewAngle / numberOfRays;


        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = transform.eulerAngles.z - ViewAngle/2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            vivePoints.Add(newViewCast._point);
            Debug.DrawLine(transform.position,newViewCast._point,Color.cyan);

        }

        int vertCount = vivePoints.Count + 1;
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[(vertCount - 2) * 3];


        vertices[0] = Vector3.zero;
        for (int i =  0; i < vertCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(vivePoints[i]);
            if(i < vertCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }

        }

        _mesh.Clear();
        _mesh.vertices  = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }

    private void Start() 
    {
        _mesh = new Mesh();
        _mesh.name = "Fow mesh";
        _meshFilter.mesh = _mesh;    
    }

    private void LateUpdate() 
    {
        DrawFOW();    
    }


    public struct ViewCastInfo
    {
        public bool _hit;
        public Vector3 _point;
        public float _dst;
        public float _angle;

        public ViewCastInfo(bool hit,Vector3 point, float dst, float angle)
        {
            _hit = hit;
            _point = point;
            _dst = dst;
            _angle = angle;
        }

    }
}
