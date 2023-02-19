using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkMaxSpeed = 5f;
   

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
        
    }

    void Update()
    {
        //Rotating
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

         Vector2 dir = new Vector2
        (
            mousePos.x - transform.position.x,
            mousePos.y - transform.position.y
        ).normalized;

        transform.right = dir;

        //Movement
        _velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        _velocity = _velocity * _walkMaxSpeed;

        _rb.velocity = _velocity;
        _rb.angularVelocity = 0f;

    }
}
