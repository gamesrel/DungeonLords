/*
  this class handles the actions the local player can do. IE: Mark a wall for digging.
*/

using UnityEngine;
using System.Collections;

public class PlayerHandler : MonoBehaviour {


    public Player PlayerDescr { get; set; }//gets set in EntryPoint.cs AND is the only connection to the player info that this GO should use.
    private Plane roofPlane;
    private Plane floorPlane;
    void Start () {
        roofPlane = new Plane(new Vector3(0,2,0), new Vector3(0,2,10), new Vector3(10,2,10));
        floorPlane = new Plane(new Vector3(0,0,0), new Vector3(0,0,10), new Vector3(10,0,10));
	
    } 
    void Update () {
        CheckForRoofOrFloorCollision();

    }

    void OnGUI() {
        DrawGrabbedMinions();
    }
    
    private float enterFloor = 0;
    private float enterRoof = 0;
    private void CheckForRoofOrFloorCollision() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int posXRoof = Mathf.RoundToInt(ray.GetPoint(enterRoof).x);
        int posZRoof = Mathf.RoundToInt(ray.GetPoint(enterRoof).z);
        int posXFloor = Mathf.RoundToInt(ray.GetPoint(enterFloor).x);
        int posZFloor = Mathf.RoundToInt(ray.GetPoint(enterFloor).z);


        //throw the 1rst ray, true if the mouse collides with the roof and position is within limits and see if the ray collides with a roof
        if(roofPlane.Raycast(ray,out enterRoof) && posXRoof >= 0 && posXRoof < DungeonWorld.WorldWidth && posZRoof >= 0 && posZRoof < DungeonWorld.WorldHeight) {
            if(DungeonWorld.WarZone[posXRoof, posZRoof] == null || DungeonWorld.WarZone[posXRoof, posZRoof].GetType() == typeof(Roof)) {

                //Debug.Log("TECHO!!!" + posXRoof + "," + posZRoof);//"Draw silouette of roof here
                ShowTmpRoof(new Pos(posXRoof, posZRoof));//ojo, tener en cuenta que roofs nulos tambien se deberian dibujar
	    
            } else if(floorPlane.Raycast(ray,out enterFloor) && posXFloor >= 0 && posXFloor < DungeonWorld.WorldWidth && posZFloor >= 0 && posZFloor < DungeonWorld.WorldHeight) {//same for floor
                if(DungeonWorld.WarZone[posXRoof, posZRoof] != null && DungeonWorld.WarZone[posXFloor, posZFloor] != null && DungeonWorld.WarZone[posXFloor, posZFloor].GetType() == typeof(Floor)) {
                    Floor selectedFloor = (Floor) DungeonWorld.WarZone[posXFloor, posZFloor];
                    if(Input.GetMouseButtonDown(1) && !Input.GetButton("Option") && PlayerDescr.GrabbedMinions.Count > 0 && //right click and we are not pressing option & grabbed minions !empty
                       selectedFloor.TileOwner == PlayerDescr.PlayerNumber && selectedFloor.TypeOfPlayer == PlayerDescr.TypeOfPlayer) {//Make sure that the position where we are releasing the minion belongs to the player.
                        PlayerDescr.PopMinion(new Pos(posXFloor, posZFloor));
                    }
                    //Debug.Log("PISO!!!" + posXFloor + "," + posZFloor);
                    if(tmpMouseRoof != null)//we are touching a floor, clear the last used roof
                        Destroy(tmpMouseRoof);
                }
            }
        }
    }

    //shows roof where the mouse cursor is over if there is a roof/null pos where the mouse is located.
    private Pos lastPos;
    private GameObject tmpMouseRoof;//represents the tile where the mouse is (if we are touching a roof), else is null.
    private bool canDig = false;
    private void ShowTmpRoof(Pos p) {
        if(p != lastPos) {
            if(tmpMouseRoof != null)//Destroy the old one if it exists and create a tile where the mouse is.
                Destroy(tmpMouseRoof);

            if(DungeonWorld.WarZone[p.x, p.z] == null || (DungeonWorld.WarZone[p.x,p.z] != null && DungeonWorld.WarZone[p.x,p.z].TileOwner != OWNER.IMPENETRABLE)) {//if tile null or tile not impenetrable
                tmpMouseRoof = TileCreator.CreateTile(new Vector3(p.x, GLOBAL.WALLHEIGHT, p.z), null, "MouseOver");
                tmpMouseRoof.transform.position += new Vector3(0,0.001f,0);//move the tile a little bit up
                canDig = true;
            } else {
                canDig = false;
            }
	    
            lastPos = p;
        }
        if(Input.GetMouseButtonDown(0) && canDig) {
            Task positionAlreadyMarkedForDigging = PlayerDescr.PlayerTasks.PositionMarkedForDig(p);
            if(positionAlreadyMarkedForDigging == null)//check if the position has NOT been marked for digging or not
                PlayerDescr.PlayerTasks.DigAtPosition(p, Instantiate(tmpMouseRoof) as GameObject);
            else//lets unselect that position
                PlayerDescr.PlayerTasks.RemoveTask(positionAlreadyMarkedForDigging);
        }
    }

    //Draws the current minions that the player has grabbed
    private readonly int maxWidth = 4;
    private readonly int maxHeight = 2;
    private readonly int spacing = 20;//spacing in px from texture to texture
    private readonly int mouseOffset = 10;//px; this is an offset for drawing the textures not where the mouse position is
    private void DrawGrabbedMinions() {
        Minion[] grabbedMinionsToDraw = PlayerDescr.GrabbedMinions.ToArray();
        int count = 0;
        for(int i = 0; i < maxHeight; i++) {
            for(int j = 0; j < maxWidth; j++) {
                if(count < grabbedMinionsToDraw.Length) {
                    GUI.DrawTexture(new Rect(mouseOffset + Input.mousePosition.x + j * spacing,//each texture will be of 16x16px
                                             mouseOffset + Screen.height - Input.mousePosition.y + i * spacing,
                                             16,
                                             16),
                                    grabbedMinionsToDraw[count].MinionHandlerRef.IconRepresentation);
                } else {
                    return;
                }		
                count++;
            }
        }
    }
}