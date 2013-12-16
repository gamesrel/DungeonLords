/*
  This is an abstract class that contains the basic attributes of any minion
*/

using UnityEngine;

public abstract class Minion {
    protected Player lord;//minion's owner
    public Player Lord { get { return lord; } }
    public Pos Position;
    protected string name;
    protected int level = 1;
    protected int health;
    protected int attack;
    protected int defense;
    /*protected*/public GameObject minionRepresentation; // protected o public?
    public Task CurrentTask;
    public bool IsGrabbed = false;//means the minion is not in the player's stack


    protected MinionHandler mMinionHandler;//reference to the handler that lets the player keep a minion / change the minion color when the mouse is over.

    public MinionHandler MinionHandlerRef { get { return mMinionHandler; } }

    protected abstract void Draw(string objName = "");

    public abstract void HandleCurrentTask();
}