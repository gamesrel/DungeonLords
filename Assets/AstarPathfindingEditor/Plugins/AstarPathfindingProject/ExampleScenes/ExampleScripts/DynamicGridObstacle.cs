using UnityEngine;
using System.Collections;
using Pathfinding;

/** Attach this script to any obstacle with a collider to enable dynamic updates of the grid around it.
 * When the object has moved a certain distance (or actually when it's bounding box has changed by a certain amount) defined by #updateError
 * it will call UpdateGraphs and update the graph around it.
 * \note This will not work very good when using erosion on the grid since erosion is done in a post-processing pass after scanning
 * and is very difficult to update without rescanning the whole grid
 * \see AstarPath::UpdateGraphs
 */
public class DynamicGridObstacle : MonoBehaviour {
	
	Collider col;
	public float updateError = 1; /**< The minimum change along one of the axis of the bounding box of collider to trigger a graph update */
	public float checkTime = 0.2F; /**< Time in seconds between bounding box checks */
	public bool simple = false; /**< Does not use physics to rasterize the object to the grid.
		Use only if there are no static obstacles in the grid and the object is axis aligned */
	
	/** Use this for initialization */
	void Start () {
		col = collider;
		if (collider == null) {
			Debug.LogError ("A collider must be attached to the GameObject for DynamicGridObstacle to work");
		}
		StartCoroutine (UpdateGraphs ());
	}
	
	Bounds prevBounds;
	bool isWaitingForUpdate = false;
	
	/** Coroutine which checks for changes in the collider's bounding box */
	IEnumerator UpdateGraphs () {
		
		if (col == null || AstarPath.active == null) {
			Debug.LogWarning ("No collider is attached to the GameObject. Canceling check");
			yield break;
		}
		
		while (true) {
			
			while (isWaitingForUpdate) {
				yield return new WaitForSeconds (checkTime);
			}
			
			Bounds newBounds = col.bounds;
			
			Bounds merged = newBounds;
			merged.Encapsulate (prevBounds);
			
			Vector3 minDiff = merged.min - newBounds.min;
			Vector3 maxDiff = merged.max - newBounds.max;
			
			if (Mathf.Abs (minDiff.x) > updateError || Mathf.Abs (minDiff.y) > updateError || Mathf.Abs (minDiff.z) > updateError ||
			   	Mathf.Abs (maxDiff.x) > updateError || Mathf.Abs (maxDiff.y) > updateError || Mathf.Abs (maxDiff.z) > updateError) {
				
				isWaitingForUpdate = true;
				AstarPath.active.RegisterCanUpdateGraphs (DoUpdateGraphs, DoUpdateGraphs2);
				
			}
			
			yield return new WaitForSeconds (checkTime);
		}
	}
	
	public void DoUpdateGraphs () {
		isWaitingForUpdate = false;
		Bounds newBounds = col.bounds;
		
		if (!simple) {
			Bounds merged = newBounds;
			merged.Encapsulate (prevBounds);
			
			if (BoundsVolume (merged) < BoundsVolume (newBounds)+BoundsVolume(prevBounds)) {
				AstarPath.active.UpdateGraphs (merged);
			} else {
				AstarPath.active.UpdateGraphs (prevBounds);
				AstarPath.active.UpdateGraphs (newBounds);
			}
		} else {
			GraphUpdateObject guo = new GraphUpdateObject (prevBounds);
			guo.updatePhysics = false;
			guo.modifyWalkability = true;
			guo.setWalkability = true;
			
			AstarPath.active.UpdateGraphs (guo);
		}
		
		Debug.DrawLine (prevBounds.min,prevBounds.max,Color.yellow);
		Debug.DrawLine (newBounds.min,newBounds.max,Color.red);
		
		prevBounds = newBounds;
	}
	
	public void DoUpdateGraphs2 () {
		if (simple) {
			GraphUpdateObject guo = new GraphUpdateObject (col.bounds);
			guo.updatePhysics = false;
			guo.modifyWalkability = true;
			guo.setWalkability = false;
			AstarPath.active.UpdateGraphs (guo);
		}
	}
	
	/* Returns a new Bounds object which contains both \a b1 and \a b2 */
	/*public Rect ExpandToContain (Bounds b1, Bounds b2) {
		Vector3 min = Vector3.Min (b1.min,b2.min);
		Vector3 max = Vector3.Max (b1.max,b2.max);
		
		return new Bounds ((max+min)*0.5F,max-min);
	}*/
	
	/** Returns the volume of a Bounds object. X*Y*Z */
	public float BoundsVolume (Bounds b) {
		return System.Math.Abs (b.size.x * b.size.y * b.size.z);
	}
}
