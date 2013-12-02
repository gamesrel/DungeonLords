using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RaycastModifier))]
public class RaycastModifierEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		GUILayout.Label ("Graph raycasting is not supported by any of the built-in graphs in the Free version of the A* Pathfinding Project","helpBox");
	}
}
