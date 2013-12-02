/*
  This is a singleton class for accessing materials 
 */

using UnityEngine;
using System.Collections;

public class TextureManager : MonoBehaviour {

    private static TextureManager instance;
    public static TextureManager Instance {
	get {
	    if(instance == null) {
		instance = GameObject.Find("MainWorld").GetComponent<TextureManager>();
		if(!instance)
		    Debug.LogError("Cannot find GameObject named 'MainWorld'");
	    }
	    return instance;
	}	
    }
    
    public Material UnownedMaterial;
    public Material ImpenetrableMaterial;
    public Material InfiniteGoldMaterial;
    public Material GoldMaterial;
    
    public Material[] FloorMaterials;
    public Material[] RoofMaterials;
    public Material[] WallMaterials;


    
    /*
      Loads a material depending on the owner of the tile.
      This is also thought for also returning a custom material in the future
      with a predefined tile so we can have a canvas of all the textures used in the game.
     */

    public Material LoadRoofMaterial(OWNER own, PLAYER typeOfPlayer) {
	switch(own) {
	case OWNER.UNOWNED:
	    return UnownedMaterial;
	case OWNER.FIRST:
	    switch(typeOfPlayer) {
	    case PLAYER.HERO:
		return RoofMaterials[0];
	    case PLAYER.DEVILGOD:
		return RoofMaterials[1];
	    default:
		Debug.LogError("Type of player not implemented/recognized " + typeOfPlayer);
		return null;
	    }
	    /*	case OWNER.SECOND:
		case OWNER.THIRD:
		case OWNER.FOURTH:
		case OWNER.FIFTH:
		case OWNER.SIXTH:
		case OWNER.SEVENTH:
		case OWNER.EIGTH:*/
	default:
	    Debug.LogError("OWNER for roof not implemented yet: " + own);
	    return null;
	}

    }

    public Material LoadWallMaterial(OWNER own, PLAYER typeOfPlayer) {
	switch(own) {
	case OWNER.UNOWNED:
	    return UnownedMaterial;
	case OWNER.FIRST:
	    switch(typeOfPlayer) {
	    case PLAYER.HERO:
		return WallMaterials[0];
	    case PLAYER.DEVILGOD:
		return WallMaterials[1];
	    default:
		Debug.LogError("Type of player not implemented/recognized " + typeOfPlayer);
		return null;
	    }
	default:
	    Debug.LogError("OWNER for wall not implemented yet: " + own);
	    return null;
	}
    }


    public Material LoadFloorMaterial(OWNER own, PLAYER typeOfPlayer) {
	switch(own) {
	case OWNER.UNOWNED:
	    return UnownedMaterial;
	case OWNER.FIRST:
	    switch(typeOfPlayer) {
	    case PLAYER.HERO:
		return FloorMaterials[0];
	    case PLAYER.DEVILGOD:
		return FloorMaterials[1];
	    default:
		Debug.LogError("Type of player not implemented/recognized " + typeOfPlayer);
		return null;
	    }
	default:
	    Debug.LogError("OWNER for floor not implemented yet: " + own);
	    return null;
	}
    }
	
    public Material LoadRawMaterial(OWNER own) {
	switch(own) {
	case OWNER.UNOWNED:
	    return UnownedMaterial;
	case OWNER.IMPENETRABLE:
	    return ImpenetrableMaterial;
	case OWNER.INFINITEGOLD:
	    return InfiniteGoldMaterial;
	case OWNER.GOLD:
	    return GoldMaterial;
	//case OWNER.NONE:
	    //break;
	default:
	    Debug.LogError("Owner not recognized: " + own);
	    return null;
	}
    }
}
