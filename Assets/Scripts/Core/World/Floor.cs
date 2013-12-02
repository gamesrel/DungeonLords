using UnityEngine;
using System.Collections;

public sealed class Floor : Tile {

    private Tile mUp;
    public Tile Up { get { return mUp; } set { mUp = value; } }
    private Tile mDown;
    public Tile Down { get { return mDown; } set { mDown = value; } }
    private Tile mLeft;
    public Tile Left { get { return mLeft; } set { mLeft = value; } }
    private Tile mRight;
    public Tile Right { get { return mRight; } set { mRight = value; } }

    private Wall mUpWall;
    public Wall UpWall { get { return mUpWall; } set { mUpWall = value; } }
    private Wall mDownWall;
    public Wall DownWall { get { return mDownWall; } set { mDownWall = value; } }
    private Wall mLeftWall;
    public Wall LeftWall { get { return mLeftWall; } set { mLeftWall = value; } }
    private Wall mRightWall;
    public Wall RightWall { get { return mRightWall; } set { mRightWall = value; } }

    private float mHealth;//health of the roof before it gets converted to floor.
    public float FloorHealth { get { return mHealth; } set { mHealth = value; } }
    
    public override Pos TilePosition { get; set; }


    //change the owner of the floor and automatically change any wall nest to the floor to UNOWNED
    // also if the wall's owner is changed, the roof also gets set automatically.
    private OWNER mTileOwner;
    public override OWNER TileOwner { get { return mTileOwner; } }

    //only changes the floor owner and doesnt extend the change to the surrounding walls/roofs
    // no use for this atm... perhaps for a world editor?
    public  override void ChangeTileOwner(PLAYER typeOfPlayer, OWNER value) {
	mTileOwner = value;
	TypeOfPlayer = typeOfPlayer;
	ChangeMaterial(TextureManager.Instance.LoadFloorMaterial(value, typeOfPlayer));
    }


    //Changes the floor owner and also if explicity expresed it extend the change to the surrounding tiles
    public void ChangeFloorOwner(PLAYER typeOfPl, OWNER value, bool extendChangeToWallsAndRoof = false) {
	TypeOfPlayer = typeOfPl;
	if(mUpWall != null) {
	    mUpWall.ChangeTileOwner(typeOfPl, extendChangeToWallsAndRoof ? value : OWNER.UNOWNED);
	}
	if(mDownWall != null) {
	    mDownWall.ChangeTileOwner(typeOfPl, extendChangeToWallsAndRoof ? value : OWNER.UNOWNED);
	}
	if(mLeftWall != null) {
	    mLeftWall.ChangeTileOwner(typeOfPl, extendChangeToWallsAndRoof ? value : OWNER.UNOWNED);
	}
	if(mRightWall != null) {
	    mRightWall.ChangeTileOwner(typeOfPl, extendChangeToWallsAndRoof ? value : OWNER.UNOWNED);
	}
	ChangeMaterial(TextureManager.Instance.LoadFloorMaterial(value, TypeOfPlayer));
	mTileOwner = value;
	//recalculate floor health
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
	    //means the floor has an owner, have to be carefull to not let the user digg other built roof
	    mHealth = 100;
	    break;	      
	}
    }
    

    public override PLAYER TypeOfPlayer { get; set; }//used to know which kind of player owns this tile
    
    public Floor(Pos pos, Player p = null, string tileName = "")
    {
	TilePosition = pos;
	mUpWall = mDownWall = mLeftWall = mRightWall = null;
	mUp = mDown = mLeft = mRight = null;
	if(p == null) {//no one owns this tile
	    mTileOwner = OWNER.UNOWNED;
	} else {
	    mTileOwner = p.PlayerNumber;
	    TypeOfPlayer = p.TypeOfPlayer;
	}
	//Set the floor health
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
	    //means the floor has an owner, have to be carefull to not let the user digg other built roof
	    mHealth = 100;
	    break;	      
	}
	Draw(tileName);
    }
    
    protected override void Draw(string tileName = "") {
	tileRepresentation = TileCreator.CreateTile(new Vector3(TilePosition.x, GLOBAL.FLOORHEIGHT, TilePosition.z),
						    TextureManager.Instance.LoadRawMaterial(mTileOwner)/*material*/,
						    tileName);
	tileRepresentation.layer = LayerMask.NameToLayer("Ground");
	BoxCollider b = tileRepresentation.AddComponent<BoxCollider>();
	b.center = Vector3.zero;
	b.size = new Vector3(1, 0.01f, 1);
	
    }
}
