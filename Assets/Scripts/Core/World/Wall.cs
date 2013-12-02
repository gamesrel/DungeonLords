using UnityEngine;
using System.Collections;

public sealed class Wall : Tile {
    private WALL mWallOrientation;
    public WALL WallOrientation { get { return mWallOrientation; } set {mWallOrientation = value; } }

    public override Pos TilePosition { get; set; }


    private OWNER mTileOwner;
    public override OWNER TileOwner { get { return mTileOwner; } }
    
    public override void ChangeTileOwner(PLAYER typeOfPlayer, OWNER value) {
        mRoof.ChangeTileOwner(typeOfPlayer, value);//if we change the owner of a wall, then the roof has to have the same owner
        ChangeMaterial(TextureManager.Instance.LoadWallMaterial(value, typeOfPlayer));
        TypeOfPlayer = typeOfPlayer;
        mTileOwner = value;
    }

    public GameObject WallRepresentation { get { return tileRepresentation; } set { tileRepresentation = value; } }
    
    private Roof mRoof;
    public Roof WallRoof { get { return mRoof; } set { mRoof = value; } }
    private Floor mFloor;
    public Floor WallFloor { get { return mFloor; } set { mFloor = value; } }

    public override PLAYER TypeOfPlayer {get; set; }//used to know which kind of player owns this tile

    public Wall(Pos pos, Floor f, Roof r, WALL orientation, Player p = null, string tileName = "") {
        mFloor = f;
        mRoof = r;
        TilePosition = pos;
        mWallOrientation = orientation;

        if(p == null) {//no one owns this tile
            mTileOwner = OWNER.UNOWNED;
        } else {
            mTileOwner = p.PlayerNumber;
            TypeOfPlayer = p.TypeOfPlayer;
        }

        Draw(tileName);
    }

    // Draws this object.
    protected override void Draw(string tileName = "") {
        tileRepresentation = TileCreator.CreateWall(mWallOrientation,
                                                    new Vector3(TilePosition.x, GLOBAL.FLOORHEIGHT, TilePosition.z),
                                                    TextureManager.Instance.LoadRawMaterial(mTileOwner)/*material*/,
                                                    tileName);

        tileRepresentation.layer = LayerMask.NameToLayer("Wall");
        tileRepresentation.AddComponent<BoxCollider>();

    }

    // Destructor of instance
    public void DestroyWall() {
        MonoBehaviour.Destroy(tileRepresentation);	
    }
}
