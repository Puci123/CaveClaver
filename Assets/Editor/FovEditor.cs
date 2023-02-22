using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfViwie))]
public class FovEditor : Editor
{
    private void OnSceneGUI() 
    {
        FieldOfViwie fow = (FieldOfViwie)target;
        
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position,-Vector3.forward,Vector3.up,360,fow.ViewRadius);

        Vector3 viweAngleA = fow.Angle2Dir(-fow.ViewAngle / 2, false);
        Vector3 viweAngleB = fow.Angle2Dir(+fow.ViewAngle / 2, false);
        Handles.DrawLine(fow.transform.position,fow.transform.position + viweAngleA * fow.ViewRadius);
        Handles.DrawLine(fow.transform.position,fow.transform.position + viweAngleB * fow.ViewRadius);


    }
}
