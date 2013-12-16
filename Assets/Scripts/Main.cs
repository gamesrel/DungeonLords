/*
  This class is used as entry point for initialization of the world,players and everything
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class Main : MonoBehaviour {

    public GameObject localPlayerGameObject;
    public OWNER localPlayerOWNER = OWNER.FIRST;
    public PLAYER localPlayerKindOfPlayer = PLAYER.DEVILGOD;

    private static List<Player> playersList;// MAXIMUM 8 PLAYERS ATM (see OWNER enum that stores the number of players)

    private static Player mLocalPlayer;
    public static Player LocalPlayer { get { return mLocalPlayer; } }//refers to the real human player that is playing the game on this machine.

    void Awake () {
        playersList = new List<Player>();

        if(!localPlayerGameObject)
            Debug.LogError("No local player found.");

        int worldWidth = 20;
        int worldHeight = 20;

        mLocalPlayer = new Player(localPlayerOWNER, localPlayerKindOfPlayer, new Pos(6,6), false/*no AI controls this player*/, localPlayerGameObject);
        playersList.Add(mLocalPlayer);

        /***** Add players here *****/
        playersList.Add(new Player(OWNER.FIRST, PLAYER.HERO, new Pos(15,15), true/*AI 1*/));


        /****************************/

        new DungeonWorld(worldWidth, worldHeight, playersList);


        CameraOrbit.MaxMoveableDistance = new Vector2(worldWidth, worldHeight);//set world limits where the camera can move.

    }
}
