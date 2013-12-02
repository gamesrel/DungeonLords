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
    /*protected*/public GameObject minionRepresentation;
    public Task CurrentTask;
    public bool IsGrabbed = false;//means the minion is not in the player's stack
    
    protected AIMoveHandler mMinionMoveHandler;//reference to the handler that moves any minionRepresentation
    // this reference SHOULD NOT be accessed from any other class that is why is marked as protected

    protected MinionHandler mMinionHandler;//reference to the handler that lets the player keep a minion / change the minion color when the mouse is over.
    public MinionHandler MinionHandlerRef { get { return mMinionHandler; } }
    
    protected abstract void Draw(string objName = "");

    //This gets called when the minion has reached the task's position. This gets called in ReachedEndOfPath() at AIMoveHandler.cs
    public abstract void HandleCurrentTask();

    //make sure to call CanReachPosition before calling this function
    // gives the order to the AI to mark a position to go.
    public void MarkGoToPosition(Pos p) {
        mMinionMoveHandler.MarkGoToPosition(p);
    }

    public bool CanReachPosition(Pos p) {
        return AstarPath.active.GetNearest(p.ToVector3()).node.walkable;
    }

    //stops moving to the path abruptly (used when canceling a task and the minion is moving towards it)
    public void StopMoving() {
        mMinionMoveHandler.Stop();
    }
    
    public void StartMovingToPosition() {
        mMinionMoveHandler.StartCoroutine(mMinionMoveHandler.StartMovingToPath());
    }
    public void StopMovingToCurrentPosition() {
        mMinionMoveHandler.StartCoroutine(mMinionMoveHandler.StopMovingToPath());
    }


    public bool PathHasBeenCalculated() {
        return mMinionMoveHandler.PathHasBeenCalculated();
    }
    
    /***** for calculating the distance to a position ******/
    public void CalculatePathTo(Pos p, bool exactPosition) {
        mMinionMoveHandler.StartCoroutine(mMinionMoveHandler.CalculateDistanceToPos(p, exactPosition));
    }
    public int GetDistanceToCalculatedPos() {// if returns  -1 means the distance hasnt being finished calculating
        return mMinionMoveHandler.GetDistanceToCalculatedPos();
    }
    public Pos ClosestPositionAroundCalculatedDistanceToP() {
        if(mMinionMoveHandler.ClosestPositionAroundCalculatedDistanceToP != null) {
            return mMinionMoveHandler.ClosestPositionAroundCalculatedDistanceToP;
        } else {
            Debug.LogError("Closest position null " + mMinionMoveHandler.name);
            return null;
        }
    }
    /*************/

}