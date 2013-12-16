/*
  In this class is defined the world where the players will interact, this class is in charge also of placing the players acording to the
  number of players that will be playing in a certain world.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public sealed class DungeonWorld {
    //represents a tile matrix that contains the players.
    private static Tile[,] mWarZone;
    public static Tile[,] WarZone { get { return mWarZone; } }

    public static GameObject WarMap { get; set; }
    // Espacio vacío inconsistente.

    private static int mWorldWidth;
    public static int WorldWidth { get { return mWorldWidth;} }
    // aquí al menos mete un espacio
    private static int mWorldHeight;
    public static int WorldHeight { get { return mWorldHeight; } }


    public DungeonWorld(int width, int height, List<Player> playersList) {
        mWarZone = new Tile[width,height];
        mWorldWidth = width;
        mWorldHeight = height;
        CreateWorldLimits(width, height);
        PlacePlayers(playersList);
    }

    // Gets called from EntryPoint.cs
    private void PlacePlayers(List <Player> players) {
        for(int i = 0; i < players.Count; i++)
            CreateStartingPlace(players[i].PlayerPosition, players[i]);
    }


    // Creates the limits of the world, these limits are also placed in the WarZone matrix
    private void CreateWorldLimits(int width, int height){
        //limits are always roof and impenetrable
        Roof limit;
        GameObject WorldLimits = new GameObject("WorldLimits");
        // En los siguientes dos ciclos tenés el mismo código repetido con un solo
        // parametro adicional. Hace un método que transforme la matriz y llamalo
        // aquí con el parámetro adicional.
        for(int i = 0; i < width; i++) {
            limit = new Roof(new Pos(i,0), OWNER.IMPENETRABLE, "LIMIT " + i + ",0");
            limit.Representation.transform.parent = WorldLimits.transform;
            mWarZone[i, 0] = limit;

            limit = new Roof(new Pos(i,height-1), OWNER.IMPENETRABLE, "LIMIT " + i + "," + (height-1));
            limit.Representation.transform.parent = WorldLimits.transform;
            mWarZone[i, height-1] = limit;
        }
        for(int j = 1; j < height; j++) {
            limit = new Roof(new Pos(0,j), OWNER.IMPENETRABLE, "LIMIT 0," + j);
            limit.Representation.transform.parent = WorldLimits.transform;
            mWarZone[0, j] = limit;

            limit = new Roof(new Pos(width-1,j), OWNER.IMPENETRABLE, "LIMIT " + (width-1) + "," + j);
            limit.Representation.transform.parent = WorldLimits.transform;
            mWarZone[width-1, j] = limit;
        }
    }

    /************ MEAT OF THE GAME FOR CREATING THE WORLD ***************/
    // Depending on which tiles are near the created tile, it creates a new UNOWNED roof/floor with its
    // pertinent walls and saves it to the WarZone matrix.
    //
    // Consider this function like carving a hole.
    //
    public static void CreatePosition(Pos p/*Player player*/) { // wtf
        // en vez del comentario implementá un metodo que devuelva este booleano
        // y se llame p.isInside(Matriz)
        if(!(p.x > 0 && p.x < mWorldWidth-1 && p.z > 0 && p.z < mWorldHeight-1)) {//make sure Pos p is inside the WarZone Matrix.
            Debug.LogError("ERROR: Tile " + p.x + "," + p.z + " is not within the world limits.");
            return;
        }
        // no voy a comentar hasta el siguiente método. no me gusta un culo
        // la lógica de aquí hacia abajo, tenés basicamente medio programa en un
        // método. El comportamiento de cada Tile debería estar encapsulado en
        // metodos ya sea en clases independientes que hereden de Tile o en
        // una método común con params adicionales.

        Floor createdFloor = new Floor(p);
        if(mWarZone[p.x,p.z] != null)
            if(mWarZone[p.x,p.z].GetType() == typeof(Roof))//there is already a roof here so we need to delete it first and then place the new floor
                ((Roof) mWarZone[p.x, p.z]).DestroyRoof();

        mWarZone[p.x, p.z] = createdFloor;

        Roof roofLeft, roofRight, roofUp, roofDown;
        Floor floorLeft, floorRight, floorUp, floorDown;
        Wall leftWall, rightWall, upWall, downWall;

        //lets check each surrounding tiles from the tile at p
        //Left Tile
        if(p.x-1 > 0) { // check if its within the world limits
            if(mWarZone[p.x-1, p.z] == null) {//nothing to the left, lets build a roof and link it to the floor.
                roofLeft = new Roof(new Pos(p.x-1, p.z), OWNER.UNOWNED);
                roofLeft.Right = createdFloor;

                mWarZone[p.x-1, p.z] = roofLeft;

                createdFloor.Left = roofLeft;
                leftWall = new Wall(p, createdFloor, roofLeft, WALL.LEFT);
                createdFloor.LeftWall = leftWall;
            } else {//means there is something here!
                if(mWarZone[p.x-1, p.z].GetType() == typeof(Roof)) {//ROOF
                    roofLeft = (Roof) mWarZone[p.x-1, p.z];
                    roofLeft.Right = createdFloor;

                    createdFloor.Left = roofLeft;
                    leftWall = new Wall(p, createdFloor, roofLeft, WALL.LEFT);
                    createdFloor.LeftWall = leftWall;
                } else if (mWarZone[p.x-1, p.z].GetType() == typeof(Floor)) {//FLOOR
                    floorLeft = (Floor) mWarZone[p.x-1, p.z];

                    if(floorLeft.RightWall != null) floorLeft.RightWall.DestroyWall();//Destroy any created wall if there is one
                    floorLeft.RightWall = null;

                    createdFloor.Left = floorLeft;
                    floorLeft.Right = createdFloor;

                } else {//SOMETHING ELSE
                    Debug.LogError("Not supported tile type: " + mWarZone[p.x-1, p.z].GetType());
                }
            }
        }
        //Right Tile
        if(p.x+1 < mWorldWidth) {
            if(mWarZone[p.x+1, p.z] == null) {//nothing to the right of the current tile.
                roofRight = new Roof(new Pos(p.x+1, p.z), OWNER.UNOWNED);
                roofRight.Left = createdFloor;

                mWarZone[p.x+1, p.z] = roofRight;

                createdFloor.Right = roofRight;
                rightWall = new Wall(p, createdFloor, roofRight, WALL.RIGHT);
                createdFloor.RightWall = rightWall;
            } else {//means there is something here!
                if(mWarZone[p.x+1, p.z].GetType() == typeof(Roof)) {//ROOF
                    roofRight = (Roof) mWarZone[p.x+1, p.z];
                    roofRight.Left = createdFloor;

                    createdFloor.Right = roofRight;
                    rightWall = new Wall(p, createdFloor, roofRight, WALL.RIGHT);
                    createdFloor.RightWall = rightWall;
                } else if (mWarZone[p.x+1, p.z].GetType() == typeof(Floor)) {//FLOOR
                    floorRight = (Floor) mWarZone[p.x+1, p.z];

                    if(floorRight.LeftWall != null) floorRight.LeftWall.DestroyWall();//Destroy any created wall if there is one
                    floorRight.LeftWall = null;

                    createdFloor.Right = floorRight;
                    floorRight.Left = createdFloor;
                } else {//SOMETHING ELSE
                    Debug.LogError("Not supported tile type: " + mWarZone[p.x+1, p.z].GetType());
                }
            }
        }
        // Up tile
        if(p.z+1 < mWorldHeight) {
            if(mWarZone[p.x, p.z+1] == null) {//nothing up of the current tile.
                roofUp = new Roof(new Pos(p.x, p.z+1), OWNER.UNOWNED);
                roofUp.Down = createdFloor;

                mWarZone[p.x, p.z+1] = roofUp;

                createdFloor.Up = roofUp;
                upWall = new Wall(p, createdFloor, roofUp, WALL.UP);
                createdFloor.UpWall = upWall;
            } else {//means there is something here!
                if(mWarZone[p.x, p.z+1].GetType() == typeof(Roof)) {//ROOF
                    roofUp = (Roof) mWarZone[p.x, p.z+1];
                    roofUp.Down = createdFloor;

                    createdFloor.Up = roofUp;
                    upWall = new Wall(p, createdFloor, roofUp, WALL.UP);
                    createdFloor.UpWall = upWall;
                } else if (mWarZone[p.x, p.z+1].GetType() == typeof(Floor)) {//FLOOR
                    floorUp = (Floor) mWarZone[p.x, p.z+1];

                    if(floorUp.DownWall != null) floorUp.DownWall.DestroyWall();//Destroy any created wall if there is one
                    floorUp.DownWall = null;

                    createdFloor.Up = floorUp;
                    floorUp.Down = createdFloor;

                } else {//SOMETHING ELSE
                    Debug.LogError("Not supported tile type: " + mWarZone[p.x, p.z+1].GetType());
                }
            }
        }
        // Down Tile
        if(p.z-1 > 0) {
            if(mWarZone[p.x, p.z-1] == null) {//nothing down at the current tile.
                roofDown = new Roof(new Pos(p.x, p.z-1), OWNER.UNOWNED);
                roofDown.Up = createdFloor;

                mWarZone[p.x, p.z-1] = roofDown;

                createdFloor.Down = roofDown;
                downWall = new Wall(p, createdFloor, roofDown, WALL.DOWN);
                createdFloor.DownWall = downWall;
            } else {//means there is something here!
                if(mWarZone[p.x, p.z-1].GetType() == typeof(Roof)) {//ROOF
                    roofDown = (Roof) mWarZone[p.x, p.z-1];
                    roofDown.Up = createdFloor;

                    createdFloor.Down = roofDown;
                    downWall = new Wall(p, createdFloor, roofDown, WALL.DOWN);
                    createdFloor.DownWall = downWall;
                } else if (mWarZone[p.x, p.z-1].GetType() == typeof(Floor)) {//FLOOR
                    floorDown = (Floor) mWarZone[p.x, p.z-1];

                    if(floorDown.UpWall != null) floorDown.UpWall.DestroyWall();//Destroy any created wall
                    floorDown.UpWall = null;

                    createdFloor.Down = floorDown;
                    floorDown.Up = createdFloor;

                } else {//SOMETHING ELSE
                    Debug.LogError("Not supported tile type: " + mWarZone[p.x, p.z-1].GetType());
                }
            }
        }
    }

    // Changes the tile Owner of the selected floor to the player and makes all the surrounding walls / roofs UNOWNED.
    // think about this function as conquering a tile.
    public static void ChangeTileOwner(Pos pos, Player player, bool includeWallsAndRoof = false) {
        if(mWarZone[pos.x, pos.z] == null) {
            Debug.LogError("Error, trying to change a null tile: " + pos.x + "," + pos.z);
            return;
        }
        if(mWarZone[pos.x, pos.z].GetType() == typeof(Floor)) {
            ((Floor)mWarZone[pos.x, pos.z]).ChangeFloorOwner(player.TypeOfPlayer, player.PlayerNumber, includeWallsAndRoof);

        } else {
            Debug.LogError("Change tileowner is only for floor tiles, dont worry if you change the floor it *should* automagically change the owner of the surrounding walls/roofs");
        }
    }
    /****************************************************************/
    /*
      Creates a starting point at initialPos and depending if the starting point
      will contain a hero or a devil god.
      Starting Place form:
      *******************
      Hero:
      F F F F F
      F W D W F  where:
      F D W D F  F = Floor
      F W D W F  W = Wall
      F F F F F  D = Doorbricks_032

      Devil God:
      F F F F F
      F A A A F  Where:
      F A A A F  F = Floor
      F A A A F  A = Altar
      F F F F F
      *******************
      initialPos -> Initial position in the WarZone matrix (This is the center).
      hero -> True if the starting point will contain a hero.
      p -> Used to know who is the owner of this Starting place.
    */
    // StartingPlace debería ser uan clase que contenga la lógica de aquí.
    private void CreateStartingPlace(Pos initialPos, Player pl) {

        Pos tmpPos;
        switch(pl.TypeOfPlayer)
            {
                //TODO: Organize the initial layouts for each kind of  player.
            case PLAYER.HERO:
                // No uses números mágicos, esos números deberían venir de un
                // método (i < numRows()) o de una variable (algo = initialPos.z - 3;)
                for(int i = 1; i < 6; i++) {
                    for(int j = 1; j < 6; j++) {
                        // espacio inconsistente entre los - y los +
                        tmpPos = new Pos(initialPos.x-3 + i, initialPos.z-3 + j);
                        CreatePosition(tmpPos);
                        ChangeTileOwner(tmpPos, pl, true/*extend change of owner to walls and roof*/);
                    }
                }
                break;
            case PLAYER.DEVILGOD:
                // Si tuvieras los tiles en una clase adicional te ahorrarías
                // mucho camello como este. Incluso un arreglo: tiles.map(...)
                for(int i = 1; i < 6; i++) {
                    for(int j = 1; j < 6; j++) {
                        tmpPos = new Pos(initialPos.x-3 + i, initialPos.z-3 + j);
                        CreatePosition (tmpPos);
                        ChangeTileOwner(tmpPos, pl, true/*extend change of owner to walls and roof*/);
                    }
                }
                break;

            }
    }

    /**/
    private void PrintWarZone() {
        string s = "";
        for(int i = 0; i < mWorldWidth; i++) {
            for(int j = 0; j < mWorldHeight; j++) {
                if(mWarZone[i,j] == null) {                // no entiendo el comentario abajo.
                    s = s + "   0" + ((int)OWNER.UNOWNED);//UNOWNED < 0 --> "-4"
                } else {
                    //This is done so when we print the matrix, the numbers are organized
                    if(((int)mWarZone[i,j].TileOwner) < 0) {//organize this for a better drawing...
                        s = s + "   " + "XX";//((int)WarZone[i,j].TileOwner);
                        // codigo comentado/ sin usar arriba otra vez, borralo.
                    } else {
                        s = s + "   0" + ((int)mWarZone[i,j].TileOwner);
                        // demasiado if/else, podrías abstraer lo que estás haciendo
                        // aquí a un método
                    }
                }
            }
            s += "\n";
        }
        Debug.Log(s);
    }
}
