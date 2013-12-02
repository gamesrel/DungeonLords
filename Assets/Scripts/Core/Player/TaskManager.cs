/*
  This behaviour contains a list of player tasks and distributes it to the minions the player has.
  This behaviour should be attached to the same GameObject where the PlayerHandler is.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class TaskManager : MonoBehaviour {

    private List<Task> tasks;
    private List<int> numberOfMinionsInTask;//Represents the number of minions tasks[i] has working on; This has the same length than the tasks list and 
    
    public Player PlayerTasksOwner;//player from which this tasks are (in order to access the minion list) (gets set from player.cs)
    
    void Start() {
        tasks = new List<Task>();
        numberOfMinionsInTask = new List<int>();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.T)){
            ManageTasks();
        }
    }

    // for a given task, it returns the index in the tasks List.
    //This is used mostly for indexing the numberOfMinionsInTask list.
    private int GetTaskIndex(Task t) {
        for(int i = 0; i < tasks.Count; i++) {
            if(t.TaskID == tasks[i].TaskID)
                return i;
        }
        Debug.LogError("No task was found");
        return -1;
    }
    
    public void RemoveTask(Task t) {
        for(int i = 0; i < tasks.Count; i++) {
            if(tasks[i].TaskID == t.TaskID) {
                t.DestroyRepresentation();
                Debug.Log("Removing task");

                t.ClearMinions();
                tasks.RemoveAt(i);
                numberOfMinionsInTask.RemoveAt(i);//remove also the number of minions
                //Debug.Log("Current tasks: " + tasks.Count);
                return;
            }
        }
        //Debug.LogError("Couldnt Find task");
    }
    
    // select from a list of minions and assign a task t to certain minons depending their position/type of minino, etc.
    // Think of this function as the brain behind every minion
    private void AssignTask(Task t, List<Minion> minions) {
        // make sure when assigning a task to a minion remove it from the tasks list as the minion has the task already in his class assigned
        switch (t.TypeOfTask) {
        case TASK.DIG:
            StartCoroutine(MoveClosestFreeMinionsToTask(minions, t, typeof(Digger)));
            break;
        case TASK.CONVERTFLOORTILE:
            StartCoroutine(MoveClosestFreeMinionsToTask(minions, t, typeof(Digger)));
            Debug.Log("CONVERT FLOOR TILE");
            break;
        case TASK.CONVERTWALLTILE:
            Debug.Log("CONVERT WALL TILE");
            break;
        default:
            Debug.LogError("Unknown type of task: " + t.TypeOfTask);
            break;
        }
    }

    //Retrieves a list of Minions that dont have tasks that are the closest to a position p
    //this is the meat of the AI
    private bool finishedCalculatingFreeMinions = true;//this is done so this funcion gets called once fully and not "thereaded", consider this function as a big lock.
    private IEnumerator MoveClosestFreeMinionsToTask(List<Minion> minions, Task t, System.Type typeOfMinion = null) {
        bool exactPosition = (t.TypeOfTask == TASK.CONVERTFLOORTILE);//only make the free minion go to the exact task position if we are converting a floor tile, else check the surroundings
	
        while(!finishedCalculatingFreeMinions)
            yield return null;
        finishedCalculatingFreeMinions = false;
	
        //----Debug.Log("*** Analizing TASK: " + t.TaskID + " ****");
        List<Minion> arrMinions = new List<Minion>();//[t.NumberOfMinionsForTask];
        if(typeOfMinion != null) {//working when giving a type of minion
            for(int i = 0; i < minions.Count; i++) {//calculate paths on the respective minions...
                if(minions[i].GetType() == typeOfMinion  && minions[i].CurrentTask == null &&  //minion is free and is of typeOfMinion
                   !minions[i].IsGrabbed) {// and minion is not in the player's stack.
                    minions[i].CalculatePathTo(t.TaskPosition, exactPosition);//calculate the path to reach the surroundings or exact position  of p.
                    while(!minions[i].PathHasBeenCalculated()) //distance hasnt been calculated
                        yield return null;
                    /************/
                    //here the distance to p has been calculated for minion[i]. now lets do the magic and find the closest ones
                    /************/	    
                    Debug.Log("DISTANCE[" + i + "]:" + minions[i].GetDistanceToCalculatedPos() + " Minion:" + minions[i].minionRepresentation.name);
                    Debug.Log(minions[i].ClosestPositionAroundCalculatedDistanceToP());
                    if(minions[i].GetDistanceToCalculatedPos() != (int) PATH.UNREACHEABLE) {// check that the minion CAN reach the path. else it doesnt make sense.
                        if(arrMinions.Count < t.NumberOfMinionsForTask) {
                            //Debug.Log("Adding a minion " + minions[i].minionRepresentation.name + " to task #" + t.TaskID );
                            arrMinions.Add(minions[i]);
                        } else {//we have minions for this task, now lets search the shortest distances from all
                            /** Calculate minimum distances for all the minions **/
                            if(minions[i].GetDistanceToCalculatedPos() == -1)
                                Debug.LogError("Error: Distance -1!");
                            for(int j = 0; j < arrMinions.Count; j++) {//replace the minion with the biggest distance of the array
                                if( minions[i].GetDistanceToCalculatedPos() < arrMinions[j].GetDistanceToCalculatedPos()) {
                                    int biggestDistanceInArrMinions = -1;
                                    int index = 0;
                                    //find the biggest distance in arrMinions
                                    for(int k = 0; k < arrMinions.Count; k++) {
                                        if(biggestDistanceInArrMinions < arrMinions[k].GetDistanceToCalculatedPos()) {
                                            biggestDistanceInArrMinions = arrMinions[k].GetDistanceToCalculatedPos();
                                            index = k;
                                        }
                                    }
                                    arrMinions[index] = minions[i];
                                    break;
                                }
                            }
                            /****************************************************/
                        }
                    }
                }
            }
            bool reachablePath = false; //this is used to count or not a minion in a task if the path can be reached or not.
            //Make each minion of Arr minions go to P!
            for(int i = 0; i < arrMinions.Count; i++) {
                Pos closestPos = arrMinions[i].ClosestPositionAroundCalculatedDistanceToP();
                if(closestPos != null && arrMinions[i].CurrentTask == null) {
                    arrMinions[i].MarkGoToPosition(closestPos);//calculate path to get to p
                    while(!minions[i].PathHasBeenCalculated())//wait until the path has been calculated
                        yield return null;
                    arrMinions[i].StartMovingToPosition();//move to p!
                    //and assign the task to the minions that we selected
                    arrMinions[i].CurrentTask = t;
                    Debug.Log("ADDING MINION '" + arrMinions[i].minionRepresentation.name +"' TO TASK " + t.TaskID);
                    t.AddMinion(arrMinions[i]); //let the task know who is in charge of this task
                    reachablePath = true;
                } else {
                    reachablePath = false;
                }
            }
            //Debug.Log("******|Adding " + arrMinions.Count + " to the numberOfMinionsForTask|******");
            int taskIndex = GetTaskIndex(t);
            if(taskIndex != -1/*means the task hasnt been found.*/ && reachablePath)
                numberOfMinionsInTask[taskIndex] = arrMinions.Count;
	    
        } else {
            Debug.LogError("Still need to implement moving any kind of minion.");
        }
        /** print closest minions **/
        Debug.Log("Found " + arrMinions.Count + " minion(s) available");
        for (int i = 0; i < arrMinions.Count; i++) {
            Debug.Log("Closest minion[" + i + "]: " + arrMinions[i].minionRepresentation.name + " Distance: " + arrMinions[i].GetDistanceToCalculatedPos());
        }
        //---Debug.Log("*** *** *** ***");
        /***************************/
        finishedCalculatingFreeMinions = true;
    }
    
    public Task PositionMarkedForDig(Pos p) {//returns the task that is for digging at position p,if not task is found, returns null
        //Debug.Log("Searching if task already exists");
        for(int i = 0; i < tasks.Count; i++) {
            if(tasks[i].TypeOfTask == TASK.DIG && p == tasks[i].TaskPosition) {
                return tasks[i];
            }
        }
        return null;
    }
    public void DigAtPosition(Pos p, GameObject posRepresentation) {
        //Debug.Log("Dig at position: " + p.x + "," + p.z);
        tasks.Add(new Task(TASK.DIG, p, 1/*minions for this task*/, posRepresentation));
        numberOfMinionsInTask.Add(0);//0 minions are working in this added task
        Debug.Log("Dig At Position");
        ManageTasks();
    }

    //this is used when the player grabs a minion, we have to release the minion from its task and
    // manage the tasks all over again
    public void FreeTaskFromMinion(Minion m){
        for(int i = 0; i < tasks.Count; i++) {
            if(m.CurrentTask.TaskID == tasks[i].TaskID) {
                m.CurrentTask.RemoveMinionFromTask(m);
                numberOfMinionsInTask[i] -= 1;
                m.CurrentTask = null;
                if (numberOfMinionsInTask[i] < 0)
                    Debug.LogError("NUMbER IN MINIONS IN TASK NEGATIVE: " + numberOfMinionsInTask[i]);
                return;
            }
        }
        Debug.LogError("Error, no minion could be found for freeing its task");
    }

    //converts a floor to a playerOwner, but first it checks that the task doesnt already exists.
    public void ConvertFloor(Pos p, OWNER playerOwner) {
        for(int i = 0; i < tasks.Count; i++) {
            if(tasks[i].TaskPosition == p && tasks[i].TypeOfTask == TASK.CONVERTFLOORTILE) {
                Debug.Log("Convert floor task already exists. not doing anything");
                return;
            }
        }
        tasks.Add(new Task(TASK.CONVERTFLOORTILE, p, 1, null/*task representation*/));
        numberOfMinionsInTask.Add(0);
        Debug.Log("Convert floor " + p.ToString());
        ManageTasks();
    }

    public void ConvertRoof(Pos p, OWNER playerOwner) {

    }

    //Called each time we want to assign a tasks to the free minions
    public void ManageTasks() {
#if UNITY_EDITOR
	    PrintTasksAndMinionsPerTask();
#endif
        for(int i = 0; i < tasks.Count; i++) {
            if(numberOfMinionsInTask[i] < tasks[i].NumberOfMinionsForTask)//assign minions to a task if there can be minions to go for it
                AssignTask(tasks[i], PlayerTasksOwner.Minions);
        }
    }

    //finds in the task list the closest task for minion m which is located at position p
    public void FindClosestTaskFor(Minion m) {
        for(int i = 0; i < tasks.Count; i++) {
            //Check that the task is not full of minions
            if(tasks[i].assignedMinions.Count < tasks[i].NumberOfMinionsForTask) {
                //  tasks[i].TaskPosition
            }
        }
    }

#if UNITY_EDITOR
    private void PrintTasksAndMinionsPerTask() {
        Debug.Log("**** TASKS " + transform.name + " ****");
        for(int i = 0; i < tasks.Count ; i++) {
            string t = "-";
            if(tasks[i].assignedMinions != null) {
                if(tasks[i].assignedMinions.Count > 0)
                    t = tasks[i].assignedMinions[0].minionRepresentation.name;
            } else {
                t = "";
            }
            Debug.Log("Minions For task " + i + " :" + numberOfMinionsInTask[i] + " " +  t + " MaxMinions:" + tasks[i].NumberOfMinionsForTask + "->" + tasks[i].TypeOfTask);
        }
        Debug.Log("**************");
    }
#endif
}
