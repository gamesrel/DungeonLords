/*
  This class contains tasks a certain player asks its creature to do.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task {
    private static int taskID = 0;
    private TASK mTypeOfTask;
    public TASK TypeOfTask { get { return mTypeOfTask; } }
    public Pos TaskPosition;
    private GameObject taskRepresentation;

    private int mNumberOfMinionsForTask = 0;//tells us how many minions can be working on this task at the same time -- orly
    public int NumberOfMinionsForTask { get { return mNumberOfMinionsForTask; } }

    private int mID = 0;
    public int TaskID { get { return mID; } }
    public List<Minion> assignedMinions;//represents the assigned minions for this task -- orly

    public Task(TASK t, Pos p, int maxNumberOfMinionsForThisTask, GameObject representation = null) {
        mTypeOfTask = t;
        TaskPosition = p;
        mID = taskID;
        taskRepresentation = representation;
        mNumberOfMinionsForTask = maxNumberOfMinionsForThisTask;
        taskID++;
        assignedMinions = new List<Minion>();
    }

    public void DestroyRepresentation() {
        if(taskRepresentation)
            MonoBehaviour.Destroy(taskRepresentation);
    }

    public void ClearMinions() {
        Debug.Log("Clearing minions from task " + mID + " #ofMinions for this task: " + assignedMinions.Count);
        for(int i = 0; i < assignedMinions.Count; i++) {
            assignedMinions[i].CurrentTask = null;
            Debug.Log("Stopping " + assignedMinions[i].minionRepresentation.name);
        }
        assignedMinions.Clear();
    }

    public void RemoveMinionFromTask(Minion m) {
        for(int i = 0; i < assignedMinions.Count; i++) {
            if(m == assignedMinions[i]) {
                assignedMinions.RemoveAt(i);
                return;
            }
        }
        Debug.LogError("couldnt find minion to remove from task");
    }

    public void AddMinion(Minion m) {
        assignedMinions.Add(m);
    }
}
