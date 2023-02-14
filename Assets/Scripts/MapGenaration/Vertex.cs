using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Vector3 WorldPos;
    public float Value;
    public int VertexIndex = -1;

    public Vertex(Vector3 pos, float value)
    {
        WorldPos = pos;
        Value = value;

    }    
}