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
        CurrentTask = null; //the creature is not doing anything at the momment // orly
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
        // no te ahorres estas llaves, vas a necesitarlas si te extendes en el
        // futuro
        if(!mMinionMoveHandler)
            Debug.LogError("AIMoveHandler could not be found for digger.");

    }

    public override void HandleCurrentTask() {
        if(CurrentTask != null) {
            switch (CurrentTask.TypeOfTask) {
            case TASK.DIG:
                //Handle stuff! -- comentario inutil
                mMinionMoveHandler.StartCoroutine(Dig(CurrentTask.TaskPosition));
                break;
            case TASK.CONVERTFLOORTILE: // CONVERT_FLOOR_TITLE es un toque más fácil de leer o qué?
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
        // No me sirve un culo este comentario aquí, y hacelo vos por mí.
    }

    private IEnumerator Dig(Pos p) {
        Tile t = DungeonWorld.WarZone[p.x,p.z];
        bool canDig = true;
        if(t == null || t.GetType() != typeof(Roof)) {
            //we are trying to dig in something that it isnt a floor -- el nombre explica esto, inutil
            canDig = false;
            //Debug.LogError("Cant dig in this kind of tile " + t.GetType());
            // de ahora en adelante no te repito esto tampoco: no dejés código
            // comentado en el repositorio.
        }
        if(canDig) {
            // mvoe esto a su propio metodo, se puede llamar Dig :)
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
        // qué tal usar nombres como tile y floor para las variables?
        // así no tengo que pensar :P
        Tile t = DungeonWorld.WarZone[p.x,p.z];
        Floor f = (Floor) t;
        // si Health es una propiedad de Floor, por que no se llama Health y ya?
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

        // este comentario no me dice nada que no pueda leer en el codigo
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
        // estás repitiendo código como un hpta aquí, abstrae esta cantidad
        // tan carechimbas de ifs a un método. Se nota el copypaste, respetate :P
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
        // pro-tip: podés buscar los TODO con grep/ack, no necesitas dejar bloques
        // gigantes de espacio en blanco.
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

    // por que hptas?
    //functions available from minion.cs
    // MarkGoToPosition(pos p)
    // CanReachPosition(Pos p)
    // StartMovingToPosition()
    // StopMovingToCurrentPosition()
    // DistanceToCalculatedPosition()
    //
}
