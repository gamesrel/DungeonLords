using UnityEngine;
using System.Collections;

public sealed class Digger : Minion {

    private int workSpeed = 10;
    
    public Digger(Player lordPlayer, Pos pos, string nam = "", int diggerLevel = 1) {
	Position = pos;
	name = nam;
	lord = lordPlayer;
	level = diggerLevel;
	workSpeed = 10 * diggerLevel;
	Draw(name);
	CurrentTask = null; //the creature is not doing anything at the momment
    }

    protected override void Draw(string objName = "") {
	minionRepresentation = GameObject.Instantiate(Resources.Load("Creatures/Devil/Digger/Digger") as GameObject,
					new Vector3(Position.x, 1, Position.z),
					Quaternion.identity) as GameObject;
	if(!minionRepresentation)
	    Debug.LogError("Couldnt load game object for 'digger' obj");
	if(objName != "")
	    minionRepresentation.name = objName + " - " + lord.PlayerGameObjRepresentation.name;

	mMinionMoveHandler = minionRepresentation.GetComponent<AIMoveHandler>();
	mMinionMoveHandler.ReferenceMinion = this;
	mMinionHandler = minionRepresentation.GetComponent<MinionHandler>();
	mMinionHandler.ReferenceMinion = this;
	if(!mMinionMoveHandler)
	    Debug.LogError("AIMoveHandler could not be found for digger.");
	
    }

    public override void HandleCurrentTask() {
	if(CurrentTask != null) {	    
	    switch (CurrentTask.TypeOfTask) {
	    case TASK.DIG:
		//Handle stuff!
		mMinionMoveHandler.StartCoroutine(Dig(CurrentTask.TaskPosition));
		break;
	    case TASK.CONVERTFLOORTILE:
		mMinionMoveHandler.StartCoroutine(ConvertFloorOwner(CurrentTask.TaskPosition));
		break;
		
	    default:
		Debug.LogError("Unknown digging task" + CurrentTask.TypeOfTask);
		break;
	    }
	} else {
	    Debug.Log("Not doing anything");
	}
	// Make sure to call lord.PlayerTasks.ManageTasks(); after each task coroutine has been completed.
    }

    private IEnumerator Dig(Pos p) {
	Tile t = DungeonWorld.WarZone[p.x,p.z];
	bool canDig = true;
	if(t == null || t.GetType() != typeof(Roof)) {
	    //we are trying to dig in something that it isnt a floor
	    canDig = false;
	    //Debug.LogError("Cant dig in this kind of tile " + t.GetType());
	}
	if(canDig) {
	    Roof r = (Roof) t;
	    while(r.RoofHealth > 0 && CurrentTask != null) {
		r.RoofHealth -= workSpeed;
		yield return new WaitForSeconds(1);//wait 1 second to dig again
		//Debug.Log("Digging " + minionRepresentation.name);
	    }
	    if(CurrentTask != null)
		FinishedDigging(p);
	}
    }

    private IEnumerator ConvertFloorOwner(Pos p) {
	Tile t = DungeonWorld.WarZone[p.x,p.z];
	Floor f = (Floor) t;
	while(f.FloorHealth > 0 && CurrentTask != null) {
	    f.FloorHealth -= workSpeed;
	    yield return new WaitForSeconds(1);//wait 1 second to continue converting the floor again.
	}
	if(CurrentTask != null)
	    FinishedConvertingFloorOwner(p);
    }

    private IEnumerator ConvertRoofOwner(Pos p) {
	//TODO
	
	yield return null;
    }
    
    private void FinishedDigging(Pos p) {
	DungeonWorld.CreatePosition(p/*CurrentTask.TaskPosition*/);
	DungeonWorld.AIWorldGraph.Scan();

	//Task has been finished!, lets remove it from the task manager
	lord.PlayerTasks.RemoveTask(CurrentTask);
	CurrentTask = null;//set to null the current task as we have finished it

	// Create a new task for converting this created position
	lord.PlayerTasks.ConvertFloor(p, lord.PlayerNumber);

	//Check for available tasks
	lord.PlayerTasks.ManageTasks();
    }

    private void FinishedConvertingFloorOwner(Pos p) {
	Floor f = (Floor) DungeonWorld.WarZone[p.x,p.z];
	/************ check surrounding Floors and create tasks for owning them ************/
	if(f.Up != null &&//make sure there is a tile
	   f.Up.GetType() == typeof(Floor) &&//the tile is a floor
	   (f.Up.TileOwner != lord.PlayerNumber || f.Up.TypeOfPlayer != lord.TypeOfPlayer))//the owner of the tile is different from the lord's owner or the type of player is different
	    lord.PlayerTasks.ConvertFloor(f.Up.TilePosition, lord.PlayerNumber);
	
	if(f.Down != null &&//make sure there is a tile
	   f.Down.GetType() == typeof(Floor) &&//the tile is a floor
	   (f.Down.TileOwner != lord.PlayerNumber || f.Down.TypeOfPlayer != lord.TypeOfPlayer))//the owner of the tile is different from the lord's owner or the type of player is different
	    lord.PlayerTasks.ConvertFloor(f.Down.TilePosition, lord.PlayerNumber);
	
	if(f.Left != null &&//make sure there is a tile
	   f.Left.GetType() == typeof(Floor) &&//the tile is a floor
	   (f.Left.TileOwner != lord.PlayerNumber || f.Left.TypeOfPlayer != lord.TypeOfPlayer))//the owner of the tile is different from the lord's owner or the type of player is different
	    lord.PlayerTasks.ConvertFloor(f.Left.TilePosition, lord.PlayerNumber);
	
	if(f.Right != null &&//make sure there is a tile
	   f.Right.GetType() == typeof(Floor) &&//the tile is a floor
	   (f.Right.TileOwner != lord.PlayerNumber || f.Right.TypeOfPlayer != lord.TypeOfPlayer))//the owner of the tile is different from the lord's owner or the type of player is different
	    lord.PlayerTasks.ConvertFloor(f.Right.TilePosition, lord.PlayerNumber);
	/**********************************************************************************/
	//Finally change this tile owner
	f.ChangeFloorOwner(lord.TypeOfPlayer, lord.PlayerNumber, false/*dont change surrounding tiles*/);

	//Task has been finished!, lets remove it from the task manager
	lord.PlayerTasks.RemoveTask(CurrentTask);
	CurrentTask = null;//set to null the current task as we have finished it
	//
	//
	//
	//
	//TODO:
	// Crear task para convertir muros!
	//
	//
	//
	//
	//
	lord.PlayerTasks.ManageTasks();//check for available tasks
    }
    private void FinishedConvertingRoofOwner(Pos p) {

    }
    
    //functions available from minion.cs
    // MarkGoToPosition(pos p)
    // CanReachPosition(Pos p)
    // StartMovingToPosition()
    // StopMovingToCurrentPosition()
    // DistanceToCalculatedPosition()
    //
	
	
}
