using UnityEngine;
using System.Collections;
using Pathfinding;

[System.Serializable]
public class StartEndModifier : Modifier {
	
	public override ModifierData input {
		get { return ModifierData.Vector; }
	}
	
	public override ModifierData output {
		get { return (addPoints ? ModifierData.None : ModifierData.StrictVectorPath) | ModifierData.VectorPath; }
	}
	
	/** Add points to the path instead of replacing. */
	public bool addPoints = false;
	public bool exactStartPoint = true;
	public bool exactEndPoint = true;
	
	public bool useRaycasting = false;
	public LayerMask mask = -1;
	
	public bool useGraphRaycasting = false;
	
	/*public override void ApplyOriginal (Path p) {
		
		if (exactStartPoint) {
			pStart = GetClampedPoint (p.path[0].position, p.originalStartPoint, p.path[0]);
			
			if (!addPoints) {
				p.startPoint = pStart;
			}
		}
		
		if (exactEndPoint) {
			pEnd = GetClampedPoint (p.path[p.path.Length-1].position, p.originalEndPoint, p.path[p.path.Length-1]);
			
			if (!addPoints) {
				p.endPoint = pEnd;
			}
		}
	}*/
	
	public override void Apply (Path p, ModifierData source) {
		
		Vector3 pStart = Vector3.zero,
		pEnd = Vector3.zero;
		
		if (exactStartPoint) { pStart = GetClampedPoint (p.path[0].position, p.originalStartPoint, p.path[0]); } else { pStart = p.path[0].position; }
		if (exactEndPoint)   { pEnd   = GetClampedPoint (p.path[p.path.Length-1].position, p.originalEndPoint, p.path[p.path.Length-1]); } else { pEnd = p.path[p.path.Length-1].position; }
		
		if (!addPoints) {
			//p.vectorPath[0] = p.startPoint;
			//p.vectorPath[p.vectorPath.Length-1] = p.endPoint;
			//Debug.DrawLine (p.vectorPath[0],pStart,Color.green);
			//Debug.DrawLine (p.vectorPath[p.vectorPath.Length-1],pEnd,Color.green);
			p.vectorPath[0] = pStart;
			p.vectorPath[p.vectorPath.Length-1] = pEnd;
			
			
		} else {
			
			Vector3[] newPath = new Vector3[p.vectorPath.Length+(exactStartPoint ? 1 : 0) + (exactEndPoint ? 1 : 0)];
			
			if (exactStartPoint) {
				newPath[0] = pStart;
			}
			
			if (exactEndPoint) {
				newPath[newPath.Length-1] = pEnd;
			}
			
			int offset = exactStartPoint ? 1 : 0;
			for (int i=0;i<p.vectorPath.Length;i++) {
				newPath[i+offset] = p.vectorPath[i];
			}
			p.vectorPath = newPath;
		}
	}
	
	public Vector3 GetClampedPoint (Vector3 from, Vector3 to, Node hint) {
		
		//float minDistance = Mathf.Infinity;
		Vector3 minPoint = to;
		
		if (useRaycasting) {
			RaycastHit hit;
			if (Physics.Linecast (from,to,out hit,mask)) {
				minPoint = hit.point;
				//minDistance = hit.distance;
			}
		}
		
		if (useGraphRaycasting && hint != null) {
			
			NavGraph graph = AstarData.GetGraph (hint);
			
			if (graph != null) {
				IRaycastableGraph rayGraph = graph as IRaycastableGraph;
				
				if (rayGraph != null) {
					GraphHitInfo hit;
					
					if (rayGraph.Linecast (from,minPoint, hint, out hit)) {
						
						//if ((hit.point-from).magnitude < minDistance) {
							minPoint = hit.point;
						//}
					}
				}
			}
		}
		
		return minPoint;
	}
	
}
