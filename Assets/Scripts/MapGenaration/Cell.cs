using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell 
{
    public Vertex LU,LD,RU,RD;
    private Vector3 MU,ML,MD,MR;

    private float _isoLevel = 0.5f;

    private int GetCellID()
    {
        int val = 0;

        if(LU.Value > _isoLevel) val += 1;
        if(RU.Value > _isoLevel) val += 2;
        if(RD.Value > _isoLevel) val += 4;
        if(LD.Value > _isoLevel) val += 8;

        return val;

     }

     private Vector3 Interpolate(Vertex a, Vertex b)
     {
        if(Mathf.Abs(_isoLevel - a.Value) < 0.0001f) return a.WorldPos;
        if(Mathf.Abs(_isoLevel - b.Value) < 0.0001f) return b.WorldPos;
        if(Mathf.Abs(a.Value - b.Value) < 0.0001f) return b.WorldPos;

        float mu = (_isoLevel - a.Value)/(b.Value - a.Value);
      
        return new Vector3(
            a.WorldPos.x + mu * (b.WorldPos.x - a.WorldPos.x), 
            a.WorldPos.y + mu * (b.WorldPos.y - a.WorldPos.y),
            a.WorldPos.z);
     }

    public Cell(Vertex lu,Vertex ld,Vertex ru,Vertex rd, float isoLevel)
    {
        LU = lu;
        LD = ld;
        RU = ru;
        RD = rd;

        _isoLevel = isoLevel;

       MU = Interpolate(LU,RU);
       ML = Interpolate(LU,LD);
       MD = Interpolate(LD,RD);
       MR = Interpolate(RU,RD);

    }

    public List<Vector3> Triangluate()
    {
        switch (GetCellID())
        {
            // 0 points
            case 0:
            break;
            
            // 1 point
            case 1:
            return Points2Mesh(ML,LU.WorldPos,MU);
            break;
             
            case 2:
            return Points2Mesh(MU,RU.WorldPos,MR);
            break;

            case 4:
            return Points2Mesh(MR,RD.WorldPos,MD);
            break;

            case 8:
            return Points2Mesh(MD,LD.WorldPos,ML);
            break;

            // 2 points
            case 3:
            return Points2Mesh(ML,LU.WorldPos,RU.WorldPos,MR);
            break;
            
            case 5:
            return Points2Mesh(ML,LU.WorldPos,MU,MR,RD.WorldPos,MD);
            break;

            case 6:
            return Points2Mesh(MU,RU.WorldPos,RD.WorldPos,MD);
            break;
            
            case 9:
            return Points2Mesh(LU.WorldPos,MU,MD,LD.WorldPos);
            break;

            case 10:
            return Points2Mesh(MD,LD.WorldPos,ML,MU,RU.WorldPos,MR);
            break;

            case 12:
            return Points2Mesh(LD.WorldPos,ML,MR,RD.WorldPos);
            break;

            //3 points
            case 7:
            return Points2Mesh(ML,LU.WorldPos,RU.WorldPos,RD.WorldPos,MD);
            break;

            case 11:
            return Points2Mesh(LD.WorldPos,LU.WorldPos,RU.WorldPos,MR,MD);
            break;

            case 13:
            return Points2Mesh(LU.WorldPos,MU,MR,RD.WorldPos,LD.WorldPos);
            break;
            
            case 14:
            return Points2Mesh(LD.WorldPos,ML,MU,RU.WorldPos,RD.WorldPos);
            break;

            // 4 points
            case 15:
            return Points2Mesh(LU.WorldPos,RU.WorldPos,RD.WorldPos,LD.WorldPos);
            break;


        }

        return null;
    }

    public List<Vector3> Points2Mesh(params Vector3[] points)
    {
        List<Vector3> vert = new List<Vector3>();

        if(points.Length >= 3)
        {
            vert.Add(points[0]);
            vert.Add(points[1]);
            vert.Add(points[2]);

        }
        if(points.Length >= 4)
        {
            vert.Add(points[0]);
            vert.Add(points[2]);
            vert.Add(points[3]);

        }
        if(points.Length >= 5)
        {
            vert.Add(points[0]);
            vert.Add(points[3]);
            vert.Add(points[4]);
        }
        if(points.Length >= 6)
        {
            vert.Add(points[0]);
            vert.Add(points[4]);
            vert.Add(points[5]);
        }

        return vert;
    }
}
