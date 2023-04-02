using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour
{
    enum State{seaking,hunting,waiting};

    [SerializeField] private float _speed;
    [SerializeField] private float _sensRange;
    [SerializeField] private float _roatationDumpTime;

    [SerializeField] private float _avoidencRange = 5;
    [SerializeField,Range(0,1)] private float _avoidencWeight = 0.1f;

    [SerializeField] private LayerMask _obstacles;
    [SerializeField] private State _state = State.seaking;
    [SerializeField] private Transform _player;

    private Rigidbody2D _rb;
    private int _numberOfRays = 9;
    private Vector3 _moveDir = Vector3.zero;


    private void LookForTarget()
    {
        if(Vector2.Distance(transform.position,_player.position) < _sensRange)
            _state = State.hunting;

    }

    private void Hunt()
    {
        Vector3 newMoveDir = Vector3.Normalize(_player.position - transform.position);
        int stepAngleSize = -180 / _numberOfRays;

        for (int i = 0; i < _numberOfRays; i++)
        {
            float angle = transform.eulerAngles.z - 180 + stepAngleSize * i;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),Mathf.Sin(angle * Mathf.Deg2Rad),0).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position,dir,_avoidencRange,_obstacles);

            if(hit.collider != null)
            {
                Vector3 htiPoint = dir * hit.distance;
                newMoveDir +=  -dir * 1/Mathf.Pow(hit.distance,3) * _avoidencWeight;
                Debug.DrawRay(transform.position,dir * hit.distance,Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position,dir * _avoidencRange,Color.green);
            }


        }

        newMoveDir = newMoveDir.normalized;
        _moveDir = Vector3.Slerp(_moveDir,newMoveDir, _roatationDumpTime).normalized;

        transform.up = _moveDir;
        _rb.velocity = _moveDir * _speed;
        _rb.angularVelocity = 0;
     
    }

    IEnumerator Bechevior()
    {
        while (true)
        {
            if(_state == State.seaking)
                LookForTarget();
            else if(_state == State.hunting)
                Hunt();

            yield return null;
        }
    }

    private void Awake() 
    {
        _rb = GetComponent<Rigidbody2D>();    
    }

    void Start()
    {
        StartCoroutine(Bechevior());
    }
    void Update()
    {
        
    }
}
