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
        Debug.Log("Mouse touched minion " + gameObject.name);
    }

    void OnMouseExit() {
        Debug.Log("Mouse exit, continue moving");
    }

    //dissapears this minion and stops any task he is doing at the moment
    public void GrabThisMinion() {
        if(minionLord.CanGrabAnotherMinion()) {

            minionLord.GrabMinion(ReferenceMinion);

            //cancel any task this minion has.
            minionLord.PlayerTasks.FreeTaskFromMinion(ReferenceMinion);


            ReferenceMinion.IsGrabbed = true;
            gameObject.SetActive(false);
            Debug.Log("Minion " + transform.name + " grabbed.");
        }
    }

    //releases the minion in the position pos
    // mismo comentario que el archivo anterior
    public void ReleaseMinion(Pos p) {
        ReferenceMinion.IsGrabbed = false;
        transform.position = p.ToVector3() + new Vector3(0,1,0);
        gameObject.SetActive(true);
        Debug.Log("Released minion " + transform.name + " at: " + p.ToString());
        minionLord.PlayerTasks.ManageTasks();
    }
}
