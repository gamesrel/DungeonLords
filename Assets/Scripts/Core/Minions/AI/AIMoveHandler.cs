/*
  This class makes a GameObject follow a certain path that is calculated from the AStarPath.cs in the scene.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public sealed class AIMoveHandler: MonoBehaviour {
    public float pickNextWaypointDistance = 0.5f;// The minimum distance to a waypoint to consider it as "reached"
    public float speed = 5;// Units per second */
    public float rotationSpeed = 1; // How fast the AI can turn around 

    private Seeker seeker;// Seeker component which handles pathfinding calls
    private CharacterController controller;// CharacterController which handles movement

    private Transform tr;//Transform, cached because of performance
    private int pathIndex = 0;
    private Vector3[] path;//This is the path the AI is currently following
    private bool moveToPath = false;

    public Minion ReferenceMinion;//Should get set when the behaviour is created (Draw function in minion.cs)

    private bool finishedCalculatingPath = false;//true when the paths surrounding the point the path has to be calculated are done
    private bool finishedCalculatingPathForPi = false;//true when a surrounding path has been calculated is P.x+1,p.z from P
    
    void Start () {
	seeker = GetComponent<Seeker>();
	controller = GetComponent<CharacterController>();
	tr = transform;
	
    }
	
    // Called when a path has completed it's calculation
    private void OnPathComplete (Path p) {
	if (p.error) //path didn't succeed, don't proceed
	    return;	
	path = p.vectorPath;//Get the calculated path as a Vector3 array
	//Find the segment in the path which is closest to the AI
	float minDist = Mathf.Infinity;
	int notCloserHits = 0;
	pathIndex = 0;//OJO ADICIONE ESTO.. deberia funcionar... restarts the path index
	for (int i = 0; i < path.Length - 1;i++) {
	    float dist = Mathfx.DistancePointSegmentStrict (path[i],path[i+1],tr.position);
	    if (dist < minDist) {
		notCloserHits = 0;
		minDist = dist;
		pathIndex = i+1;
	    } else if (notCloserHits > 6) {
		break;
	    }
	}
	moveToPath = false;
	finishedCalculatingPath = true;
    }

    public bool PathHasBeenCalculated() {
	return finishedCalculatingPath;
    }
    
    // Stops the AI. Does not prevent new path calls from making the AI move again
    public void Stop () {
	pathIndex = -1;
	moveToPath = false;
    }

    //coroutine
    public IEnumerator StopMovingToPath() {
	while(!finishedCalculatingPath)
	    yield return null;
	moveToPath = false;
    }
    
    //flag to make the behaviour move the transform to the path set by "GoToPosition";
    //coroutine
    public IEnumerator StartMovingToPath() {
	while(!finishedCalculatingPath)
	    yield return null;
	moveToPath = true;
    }
    // Start a new path to targetPoint, to move the behaviour call StartMovingToPath after this function
    public void MarkGoToPosition(Pos targetPoint) {
	finishedCalculatingPath = false;
	if (seeker == null)
	    return;
	//Start a new path from transform.positon to target.position, return the result to OnPathComplete
	seeker.StartPath (tr.position, targetPoint.ToVector3(), OnPathComplete);
    }

    //The AI has reached the end of the path
    private void ReachedEndOfPath () {
	Debug.Log("***** Path Completed for " + transform.name + "! *****");
	pathIndex = -1;
	moveToPath = false;
	ReferenceMinion.HandleCurrentTask();//we have reached the end of the calculated path, now lets do what we have to do wth the current task.
    }


    void Update () {
	if(path == null || pathIndex >= path.Length || pathIndex < 0 || !moveToPath) { //means the character is iddle
	    //TODO: Play idle animations
	} else {
	    ReferenceMinion.Position = Pos.Vector3ToPos(tr.position);
	    //Change target to the next waypoint if the current one is close enough
	    Vector3 currentWaypoint = path[pathIndex];
	    currentWaypoint.y = tr.position.y;
	    while ((currentWaypoint - tr.position).sqrMagnitude < pickNextWaypointDistance*pickNextWaypointDistance) {
		pathIndex++;
		if (pathIndex >= path.Length) {
		    //Use a lower pickNextWaypointDistance for the last point. If it isn't that close, then decrement the pathIndex to the previous value and break the loop
		    if ((currentWaypoint - tr.position).sqrMagnitude < (pickNextWaypointDistance*0.2)*(pickNextWaypointDistance*0.2)) {
			ReachedEndOfPath ();
			return;
		    } else {
			pathIndex--;
			break; //Break the loop, otherwise it will try to check for the last point in an infinite loop
		    }
		}
		currentWaypoint = path[pathIndex];
		currentWaypoint.y = tr.position.y;
	    }		
	    Vector3 dir = currentWaypoint - tr.position;
	    // Rotate towards the target
	    tr.rotation = Quaternion.Slerp (tr.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
	    tr.eulerAngles = new Vector3(0, tr.eulerAngles.y, 0);
	    
	    Vector3 forwardDir = tr.forward;
	    forwardDir = forwardDir * speed;
	    forwardDir *= Mathf.Clamp01 (Vector3.Dot (dir, tr.forward));
	    
	    controller.SimpleMove (forwardDir);
	}
    }








    /********************/
    // This is how getting a distance to a position works:
    // 1. Call Calculate DistanceToPos.
    // 2. Wait for the path calculation to finish. FinsihedCalculatingDistanceToPos()
    // 3. keep checking  PathHasBeenCalculated() until it returns true

    private int currentCalculatedDistance =  -1;// means the path hasnt been finished calculating
    private Pos mClosestPositionAroundCalculatedDistanceToP = null; // this is the closest position around the P param from CalculateDistanceToPos(p)
    public Pos ClosestPositionAroundCalculatedDistanceToP { get { return mClosestPositionAroundCalculatedDistanceToP; } }
    public IEnumerator CalculateDistanceToPos(Pos p, bool exactPosition) {
	finishedCalculatingPath = false;
	mClosestPositionAroundCalculatedDistanceToP = null;
	List<Pos> positionsToCalculatedDistance = new List<Pos>();
	if(!exactPosition) {////this is done when we want to check the distance to the surrounding position (IE: when we are excavating, converting roofs.)
	    /* check world limits */
	    if(p.z+1 < DungeonWorld.WorldHeight-1)//limits are always impenetrable
		if(DungeonWorld.WarZone[p.x,p.z+1] != null && DungeonWorld.WarZone[p.x,p.z+1].GetType() == typeof(Floor)) {//check that position is a floor
		    //		Debug.Log("UP");
		    positionsToCalculatedDistance.Add(new Pos(p.x, p.z+1));//pos up
		}
	
	    if(p.z-1 > 0/*0->Impenetrable always*/)
		if(DungeonWorld.WarZone[p.x,p.z-1] != null && DungeonWorld.WarZone[p.x,p.z-1].GetType() == typeof(Floor)) {//check that position is a floor
		    //		Debug.Log("DOWN");
		    positionsToCalculatedDistance.Add(new Pos(p.x, p.z-1));//down
		}
	
	    if(p.x-1 > 0/*0->Impenetrable always*/)
		if(DungeonWorld.WarZone[p.x-1,p.z] != null && DungeonWorld.WarZone[p.x-1,p.z].GetType() == typeof(Floor)) {//check that position is a floor
		    //		Debug.Log("LEFT");
		    positionsToCalculatedDistance.Add(new Pos(p.x-1, p.z));//left
		}
	
	    if(p.x+1 < DungeonWorld.WorldWidth-1)//limits are always impenetrable
		if(DungeonWorld.WarZone[p.x+1,p.z] != null && DungeonWorld.WarZone[p.x+1,p.z].GetType() == typeof(Floor)) {//check that position is a floor
		    positionsToCalculatedDistance.Add(new Pos(p.x+1, p.z));//right
		    //		Debug.Log("RIGHT");
		}
	} else {//this is done when we want to check the distance to the exact position (IE: when we want to convert a floor we need the minion to stand exactly above the floor)
	    positionsToCalculatedDistance.Add(p);//only one position to go to, we dont want surrounding positions
	}
	
	/***********************/
	int minDistanceAroundP = (int)PATH.UNREACHEABLE;//9999;
	//Debug.Log("****Size of positions to calculateDistance: " + positionsToCalculateDistance.Count + "** Player " + transform.name);
	for(int i = 0; i < positionsToCalculatedDistance.Count; i++) {//get the shortest surounding distance to p
	    //	currentCalculatedDistance = -1;// -1 means the path hasnt been finished calculating
	    finishedCalculatingPathForPi = false;//gets set to true in the callback "FinishedCalculatingDistanceToPos()"
	    seeker.StartPath(tr.position, positionsToCalculatedDistance[i].ToVector3(), FinishedCalculatingDistanceToPos);//calculate the path
	    
	    while(!finishedCalculatingPathForPi) {
		yield return null;
	    }
	    if(minDistanceAroundP > currentCalculatedDistance) {
		minDistanceAroundP = currentCalculatedDistance;
		mClosestPositionAroundCalculatedDistanceToP = positionsToCalculatedDistance[i];
		//Debug.Log("Closest Position: " + mClosestPositionAroundCalculatedDistanceToP.ToString());
	    }
	}
	currentCalculatedDistance = minDistanceAroundP;
	finishedCalculatingPath = true;
    }

    //This is a callback
    private void FinishedCalculatingDistanceToPos (Path p) {
	currentCalculatedDistance = -1;
	if (p.error) {//path didn't succeed, don't proceed
	    Debug.LogError("Couldnt find a path for " + tr.name);
	    currentCalculatedDistance = (int) PATH.UNREACHEABLE;//98989;
	    finishedCalculatingPathForPi = true;
	    return;
	}
	Vector3[] calculatedPath = p.vectorPath;//Get the calculated path as a Vector3 array
	//Find the segment in the path which is closest to the AI
	float minDist = Mathf.Infinity;
	int notCloserHits = 0;
	int myPathIndex = 0;
	for (int i = 0; i < calculatedPath.Length - 1; i++) {
	    float dist = Mathfx.DistancePointSegmentStrict (calculatedPath[i], calculatedPath[i+1], tr.position);
	    if (dist < minDist) {
		notCloserHits = 0;
		minDist = dist;
		myPathIndex = i+1;
	    } else if (notCloserHits > 6) {
		break;
	    }
	}
	currentCalculatedDistance = calculatedPath.Length - myPathIndex;
	finishedCalculatingPathForPi = true;
    }

    public int GetDistanceToCalculatedPos() {
	return currentCalculatedDistance;
    }
    /************************/


    
}
