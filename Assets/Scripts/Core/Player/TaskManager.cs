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
            Debug.Log("DIG");
            break;
        case TASK.CONVERTFLOORTILE:
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
