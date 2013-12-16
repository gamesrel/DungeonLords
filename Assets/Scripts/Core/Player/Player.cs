/*
  This class represents the player / players that are playing in the game.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public sealed class Player {

    // ?
    //    private float health = 1000;
    //    private int gold = 100;
    //    private int mana = 100;
    private Pos mPlayerPos;
    public Pos PlayerPosition { get { return mPlayerPos; } }

    private List<Minion> mMinions;
    public List<Minion> Minions { get { return mMinions; } }

    private Stack<Minion> mGrabbedMinions;
    public Stack<Minion> GrabbedMinions { get { return mGrabbedMinions; } }

    private OWNER mPlayerNumber;
    public OWNER PlayerNumber { get { return mPlayerNumber; } }

    private PLAYER mTypeOfPlayer;
    public PLAYER TypeOfPlayer { get { return mTypeOfPlayer; } }


    public TaskManager PlayerTasks;

    private GameObject objRepresentation;
    public GameObject PlayerGameObjRepresentation { get { return objRepresentation; } }

    public Player(OWNER own, PLAYER typeOfPlayer, Pos startingPos, bool playerControlledByAI, GameObject playerObj = null/*in case is null create a new one*/) {
        if(!playerObj) {  // reversa esta lógica, tenés mas ©odigo en el else y te ahorras uan negacion
            objRepresentation = new GameObject("Player " + ((playerControlledByAI) ? "AI" : ""));
            /*TODO DEFINE AI HANDLERS AND ADD THEM*/
        } else {
            objRepresentation = playerObj;
            if(!objRepresentation.GetComponent<PlayerHandler>())
                objRepresentation.AddComponent<PlayerHandler>();
            objRepresentation.GetComponent<PlayerHandler>().PlayerDescr = this;
        }

        PlayerTasks = objRepresentation.AddComponent<TaskManager>();//Add taskManager object
        PlayerTasks.PlayerTasksOwner = this;


        if((int) own < 1)
            Debug.LogError("ERROR: Player number cannot be negative");
        mPlayerNumber = own;
        mTypeOfPlayer = typeOfPlayer;
        mPlayerPos = startingPos;
        mMinions = new List<Minion>();
        mGrabbedMinions = new Stack<Minion>();
        CreateStartingDiggers(GLOBAL.STARTINGDIGGERS);
    }

    //adds a minion to the grabbed minions stack
    public void GrabMinion(Minion m) {
        mGrabbedMinions.Push(m);
    }

    //releases a grabbed minion at pos p if there is any inthe stack
    public void PopMinion(Pos p) {
        if(mGrabbedMinions.Count > 0) {
            Minion m = mGrabbedMinions.Pop();
            m.MinionHandlerRef.ReleaseMinion(p);
        }
    }

    // Crea una constante (NUM_MAX_MINIONS?) y te ahorras el comentario
    // y el magic number.
    //Max 8 minions can be grabbed at the same time.
    public bool CanGrabAnotherMinion() {
        return mGrabbedMinions.Count < 8;
    }

    private void CreateStartingDiggers(int numberOfDiggers) {
        //initial positions where the diggers will be placed, if we have more diggers than array.length then just % the array.
        Pos[] initialPositions = {
            // te reformatié esto para que veas cuanto te estás repitiendo. Cada 4 líneas puede ser reemplazado
            // por una llamada a un metodo.
            new Pos(mPlayerPos.x-2,mPlayerPos.z-2),
            new Pos(mPlayerPos.x-2,mPlayerPos.z+2),
            new Pos(mPlayerPos.x+2,mPlayerPos.z+2),
            new Pos(mPlayerPos.x+2, mPlayerPos.z-2),

            new Pos(mPlayerPos.x-2,mPlayerPos.z+0),
            new Pos(mPlayerPos.x+0,mPlayerPos.z+2),
            new Pos(mPlayerPos.x+2,mPlayerPos.z+0),
            new Pos(mPlayerPos.x+0, mPlayerPos.z-2),

            new Pos(mPlayerPos.x-2,mPlayerPos.z+1),
            new Pos(mPlayerPos.x+1,mPlayerPos.z+2),
            new Pos(mPlayerPos.x+2,mPlayerPos.z-1),
            new Pos(mPlayerPos.x-1, mPlayerPos.z-2),

            new Pos(mPlayerPos.x-2,mPlayerPos.z-1),
            new Pos(mPlayerPos.x-1,mPlayerPos.z+2),
            new Pos(mPlayerPos.x+2,mPlayerPos.z+1),
            new Pos(mPlayerPos.x+1, mPlayerPos.z-2) };
        // no podés usar Select en vez de ciclos de una línea?
        for(int i = 0; i < numberOfDiggers; i++) {
            mMinions.Add(new Digger(this, initialPositions[i%initialPositions.Length], "Digger " + i));
        }
    }
}
