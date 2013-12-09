/*
  This script reacts to any player action.

  lets say when the player puts the mouse over a minion, the minion starts flashing and stops.
  if the player clicks over a minion, the minion dissapears and the player can move the minion

*/
// Meté esto a tu .emacs, cerrá este archivo y volve a abrirlo.
// (setq-default show-trailing-whitespace t)

using UnityEngine;
using System.Collections;

public class MinionHandler : MonoBehaviour {

    public Minion ReferenceMinion;//Should get set when the behaviour is created (Draw function in minion.cs)
    private Player minionLord;

    public Texture2D IconRepresentation;

    // Use this for initialization
    void Start () {
        minionLord = ReferenceMinion.Lord;
    }

    // Update is called once per frame
    void Update () {

    }

    void OnMouseOver() {
        //Grab the minion -- es un toque obvio este comentario
        if(Input.GetMouseButtonDown(0) && Main.LocalPlayer == minionLord /*dont let the user get enemy's minions*/) {
            GrabThisMinion();
        }
    }

    void OnMouseEnter() {
        // tenés un comentario para un método de una línea, repensa
        // los nombres si el actual te genera dudas y necesitas el
        // comentario
        //dont move to any position while the mouse is over the minion.
        ReferenceMinion.StopMovingToCurrentPosition();
    }

    void OnMouseExit() {
        //continue moving to the position if we dont have the mouse over the minion anymore.
        ReferenceMinion.StartMovingToPosition();
    }

    //dissapears this minion and stops any task he is doing at the moment
    public void GrabThisMinion() {
        if(minionLord.CanGrabAnotherMinion()) {
            // agarrar, cancelar las tareas y actualizar el estado
            // deberíá hacer algo que haga el lord, no debería estar el
            // código aquí
            minionLord.GrabMinion(ReferenceMinion);
            ReferenceMinion.StopMoving();

            //cancel any task this minion has.
            minionLord.PlayerTasks.FreeTaskFromMinion(ReferenceMinion);


            ReferenceMinion.IsGrabbed = true;
            gameObject.SetActiveRecursively(false);
            Debug.Log("Minion " + transform.name + " grabbed.");
        }
    }

    //releases the minion in the position pos
    // mismo comentario que el archivo anterior
    public void ReleaseMinion(Pos p) {
        ReferenceMinion.IsGrabbed = false;
        transform.position = p.ToVector3() + new Vector3(0,1,0);
        gameObject.SetActiveRecursively(true);
        Debug.Log("Released minion " + transform.name + " at: " + p.ToString());
        minionLord.PlayerTasks.ManageTasks();
    }
}
