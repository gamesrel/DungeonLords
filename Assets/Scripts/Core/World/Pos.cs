/*
  This is just a simple class that represents positions of a tile, its used by every tile.
*/

using UnityEngine;

public sealed class Pos
{
    private int mPosX;
    public int x { 
        get { return mPosX; }
        set { mPosX = value; } 
    }

    private int mPosZ;
    public int z { 
        get { return mPosZ; }
        set { mPosZ = value; } 
    }

    public Pos(int x, int z)
    {
        mPosX = x;
        mPosZ = z;
    }

    public Vector3 ToVector3() {
        return new Vector3(x,0,z);
    }

    public static Pos Vector3ToPos(Vector3 v) {
        return new Pos(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
    }

    public override string ToString() {
        return "(" + mPosX + "," + mPosZ + ")";
    }
    
    /*** operator overriding ***/
    public static bool operator ==(Pos p1, Pos p2) {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(p1, p2)) {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)p1 == null) || ((object)p2 == null)) {
            return false;
        }
        return p1.x == p2.x && p1.z == p2.z;
    }
        
    public static bool operator !=(Pos p1, Pos p2) {
        return !(p1 == p2);
    }
    
    public override bool Equals(System.Object obj) {
        //Check for null and compare run-time types.
        if (obj == null || GetType() != obj.GetType()) return false;
        Pos p = (Pos)obj;
        return (x == p.x) && (z == p.z);
    }

    public override int GetHashCode() {
        return mPosX ^ mPosZ;
    }
    /****************************/
}
