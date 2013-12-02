/*
  Simple class that contains all the info for a roof tile.
 */

using UnityEngine;
using System.Collections;

public sealed class Roof : Tile {

    private Tile mUp;
    public Tile Up { get { return mUp; } set { mUp = value; } }
    private Tile mDown;
    public Tile Down { get { return mDown; } set { mDown = value; } }
    private Tile mLeft;
    public Tile Left { get { return mLeft; } set { mLeft = value; } }
    private Tile mRight;
    public Tile Right { get { return mRight; } set { mRight = value; } }

    private float mHealth;//health of the roof before it gets converted to floor.
    public float RoofHealth { get { return mHealth; } set { mHealth = value; } }

    //gets set automatically if a wall next to it has changed the owner.(Check wall set on Wall.cs)
    private OWNER mTileOwner;
    public override OWNER TileOwner { get { return mTileOwner; } }
    
    public override void ChangeTileOwner(PLAYER typeOfPlayer, OWNER roofOwner) {
	TypeOfPlayer = typeOfPlayer;
	mTileOwner = roofOwner;
	switch (mTileOwner) {
	case OWNER.IMPENETRABLE | OWNER.INFINITEGOLD:
	    mHealth = Mathf.Infinity;
	    break;
	case OWNER.GOLD:
	    mHealth = 100;
	    break;
	case OWNER.UNOWNED:
	    mHealth = 25;
	    break;
	default:
	    //means the roof has an owner, have to be carefull to not let the user digg other built roof
	    mHealth = 100;
	    break;	      
	}
	ChangeMaterial(TextureManager.Instance.LoadRoofMaterial(roofOwner, typeOfPlayer));
    }

    public override Pos TilePosition { get; set; }
    public override PLAYER TypeOfPlayer {get; set;}
    
    public Roof(Pos pos, Player p = null, string tileName = "")
    {
	TilePosition = pos;
	mUp = mDown = mLeft = mRight = null;

	if(p == null) {//no one owns this tile
	    mTileOwner = OWNER.UNOWNED;
	} else {
	    mTileOwner = p.PlayerNumber;
	    TypeOfPlayer = p.TypeOfPlayer;
	}
	Draw(tileName);

    }
    public Roof(Pos pos, OWNER own, string tileName = "")
    {
	TilePosition = pos;
	mTileOwner = own;
	mUp = mDown = mLeft = mRight = null;
	
	Draw(tileName);

    }

    protected override void Draw(string tileName = "") {
	tileRepresentation = TileCreator.CreateTile(new Vector3(TilePosition.x, GLOBAL.WALLHEIGHT, TilePosition.z),
						    TextureManager.Instance.LoadRawMaterial(mTileOwner)/*material*/,
						    tileName);
    }

    public void DestroyRoof() {
	MonoBehaviour.Destroy(tileRepresentation);
    }
}
