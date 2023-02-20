using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkMaxSpeed = 5f;
    [SerializeField] private ChunkManager _chunkManager;
    [SerializeField] private Camera _camera;

    private Vector2 _velocity = Vector2.zero;

    private Rigidbody2D _rb;
    private Animator _anim;

    private void Awake() 
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();    
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
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = -Mathf.Atan2(dir.x,dir.y) * Mathf.Rad2Deg + 90;
        transform.rotation = Quaternion.AngleAxis(angle,Vector3.forward);


        //Movement
        _velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        _velocity = _velocity * _walkMaxSpeed;

        _rb.velocity = _velocity;
        _rb.angularVelocity = 0f;
        _rb.angularVelocity = 0f;
    }
}
