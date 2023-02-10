using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpectator : MonoBehaviour
{

    [SerializeField] private ChunkManager _chunkManager;
    [SerializeField] private float _speed;
    
    void Start()
    {
        _chunkManager.ClearData();
        _chunkManager.UpdateVisiableChunks();
    }

    void Update()
    {
        Vector3 offset = new Vector3(Input.GetAxis("Horizontal") * _speed, Input.GetAxis("Vertical") * _speed,0) * Time.deltaTime;
        transform.position += offset;     

        _chunkManager.ViewerPos = transform.position;
        _chunkManager.UpdateVisiableChunks();   
    }
}
