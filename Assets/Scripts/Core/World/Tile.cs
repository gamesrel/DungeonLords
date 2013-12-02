/*
  This class represents the basic structure of any world, a tile, considered as the minimun piece of land.
  There are 3 types of tiles, Roof, Wall and Floor.
*/

using UnityEngine;
using System.Collections;

public abstract class Tile {
    public abstract PLAYER TypeOfPlayer { get; set; }
    public abstract OWNER TileOwner { get; }
    public abstract void ChangeTileOwner(PLAYER typeOfPlayer, OWNER value);
    
    public abstract Pos TilePosition { get; set; }

    protected GameObject tileRepresentation;
    public GameObject Representation { get { return tileRepresentation; } }

    public void ChangeMaterial(Material m) {
        Representation.renderer.material = m;
    }

    
    protected abstract void Draw(string s = "");// draws the current tile and optionally with a tile name
}