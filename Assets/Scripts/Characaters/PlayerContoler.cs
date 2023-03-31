using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkMaxSpeed = 5f;
    [SerializeField] private ChunkManager _chunkManager;

    //Drill Test only
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _range;

    //
    private Vector2 _velocity = Vector2.zero;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Camera _viewCamera;
    private Vector3 _forward;

    private void Maining()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _forward,_range,_groundMask);

        if(hit.collider != null)
        {
            Debug.DrawLine(transform.position,hit.point,Color.red);

            if(Input.GetMouseButtonDown(0))
            {
                Chunk.ControlNode node = _chunkManager.GetNodeFromWorldPoint(hit.point);
                if(node != null)
                {
                    node._active = false;    
                    _chunkManager.RecreateChunkMesh(hit.point);
                }
            }
        }
    }

    private void Movement()
    {
        //Update visible chunks
        if(_chunkManager != null)
        {
            _chunkManager.ViewerPos = transform.position;
            _chunkManager.UpdateVisiableChunks();
        }
        else
        {
            Debug.LogWarning("Chunk menager not assignened");
        }
        
        //Rotation
        _forward = (Input.mousePosition - _viewCamera.WorldToScreenPoint(transform.position)).normalized;
        float angle = -Mathf.Atan2(_forward.x,_forward.y) * Mathf.Rad2Deg + 90;
        transform.rotation = Quaternion.AngleAxis(angle,Vector3.forward);


        //Movement
        _velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        _velocity = _velocity * _walkMaxSpeed;

        _rb.velocity = _velocity;
        _rb.angularVelocity = 0f;
        _rb.angularVelocity = 0f;

        Maining();
    }


    private void Awake() 
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _viewCamera = Camera.main;    
    }

    void Start()
    {
        if(_chunkManager != null)
        {
            _chunkManager.ClearData();
            _chunkManager.UpdateVisiableChunks();
        }
    
    }

    void Update()
    {   
        Movement();
    }
}
