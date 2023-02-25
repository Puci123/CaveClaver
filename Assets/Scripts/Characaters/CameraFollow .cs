using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField, Range(0,1)] private float _smoothSpeed = 0.125f;
    
    private float zPos;

    void Awake()
    {
        zPos = transform.position.z;
    }

    private void FixedUpdate()
    {
        Vector3 smoothPos = Vector3.Lerp(transform.position, _target.position, _smoothSpeed);
        transform.position = new Vector3(smoothPos.x, smoothPos.y, zPos);

    }
}
