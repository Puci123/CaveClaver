using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell 
{
    public Vertex LU,LD,RU,RD;
    private Vertex MU,ML,MD,MR;
    private Chunk _chunk;

    private float _isoLevel = 0.5f;

    private int GetCellID()
    {
        int val = 0;

        if(LU.Value > _isoLevel) val += 8;
        if(RU.Value > _isoLevel) val += 4;
        if(RD.Value > _isoLevel) val += 2;
        if(LD.Value > _isoLevel) val += 1;

        return val;

     }

     private Vertex Interpolate(Vertex a, Vertex b)
     {
        if(Mathf.Abs(_isoLevel - a.Value) < 0.0001f) return a;
        if(Mathf.Abs(_isoLevel - b.Value) < 0.0001f) return b;
        if(Mathf.Abs(a.Value - b.Value) < 0.0001f) return   b;

        float mu = (_isoLevel - a.Value)/(b.Value - a.Value);
      
        Vector3 newPos = new Vector3(
            a.WorldPos.x + mu * (b.WorldPos.x - a.WorldPos.x), 
            a.WorldPos.y + mu * (b.WorldPos.y - a.WorldPos.y),
            a.WorldPos.z);

        return new Vertex(newPos,1f);
     }

    public Cell(Vertex lu,Vertex ld,Vertex ru,Vertex rd, float isoLevel, Chunk chunk)
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

        _chunk = chunk;

    }

    public List<Vertex> Triangluate()
    {
        switch (GetCellID()) 
        {
            case 0:
                break;

            // 1 points:
            case 1:
                return new List<Vertex>() {ML, MD,LD};
                break;
            case 2:
                return new List<Vertex>() {RD, MD, MR};
                break;
            case 4:
                return new List<Vertex>() {RU, MR, MU};
                break;
            case 8:
                return new List<Vertex>() {LU, MU, ML};
                break;

            // 2 points:
            case 3:
                return new List<Vertex>() {MR, RD,LD, ML};
                break;
            case 6:
                return new List<Vertex>() {MU, RU, RD, MD};
                break;
            case 9:
                return new List<Vertex>() {LU, MU, MD,LD};
                break;
            case 12:
                return new List<Vertex>() {LU, RU, MR, ML};
                break;
            case 5:
                return new List<Vertex>() {MU, RU, MR, MD,LD, ML};
                break;
            case 10:
                return new List<Vertex>() {LU, MU, MR, RD, MD, ML};
                break;
            
            // 3 point:
            case 7:
                return new List<Vertex>() {MU, RU, RD,LD, ML};
                break;
            case 11:
                return new List<Vertex>() {LU, MU, MR, RD,LD};
                break;
            case 13:
                return new List<Vertex>() {LU, RU, MR, MD,LD};
                break;
            case 14:
                return new List<Vertex>() {LU, RU, RD, MD, ML};
                break;

            // 4 point:
            case 15:
                _chunk.AddVerteciesAssCheckec(LU, RU, RD,LD);
                return new List<Vertex>() {LU, RU, RD,LD};
                break;
            
		}

        return null;

    }
}
