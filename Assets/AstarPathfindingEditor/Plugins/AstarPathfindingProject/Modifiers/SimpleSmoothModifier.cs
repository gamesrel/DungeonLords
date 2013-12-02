using UnityEngine;
using System.Collections;
using Pathfinding;

[AddComponentMenu ("Pathfinding/Modifiers/Simple Smooth")]
[System.Serializable]
/** Modifier which smooths the path. This modifier can smooth a path by either moving the points closer together (Simple) or using Bezier curves (Bezier).\n
 * Attach this component to the same GameObject as a Seeker component */
public class SimpleSmoothModifier : MonoModifier {
	
	public override ModifierData input {
		get { return ModifierData.All; }
	}
	
	public override ModifierData output {
		get {
			ModifierData result = ModifierData.VectorPath;
			if (iterations == 0 && smoothType == SimpleSmoothModifier.SmoothType.Simple && !uniformLength) {
				result |= ModifierData.StrictVectorPath;
			}
			return result;
		}
	}
	
	/** Type of smoothing to use */
	public SmoothType smoothType = SmoothType.Simple;
	
	/** Number of times to subdivide when not using a uniform length */
	public int subdivisions = 2;
	
	/** Number of times to apply smoothing */
	public int iterations = 2;
	
	/** The strength of the smoothing. A value from 0 to 1 is recommended, but values larger than 1 works too.*/
	public float strength = 0.5F;
	
	/** Toggle to divide all lines in equal length segments.
	  * \see #maxSegmentLength
	  */
	public bool uniformLength = true;
	
	/** The length of the segments when using #uniformLength */
	public float maxSegmentLength = 2F;
	
	/** Length factor of the bezier curves' tangents' */
	public float bezierTangentLength = 0.4F;
	
	public float offset = 0.2F;
	
	public enum SmoothType {
		Simple,
		Bezier,
		OffsetSimple
	}
	
	public override void Apply (Path p, ModifierData source) {
		
		//This should never trigger unless some other modifier has messed stuff up
		if (p.vectorPath == null) {
			return;
		}
		
		Vector3[] path = p.vectorPath;
		
		switch (smoothType) {
		case SmoothType.Simple:
				p.vectorPath = SmoothSimple (path); break;
		case SmoothType.Bezier:
				p.vectorPath = SmoothBezier (path); break;
		case SmoothType.OffsetSimple:
				p.vectorPath = SmoothOffsetSimple (path); break;
		}
	}
	
	public Vector3[] SmoothOffsetSimple (Vector3[] path) {
		
		if (path.Length <= 2 || iterations <= 0) {
			return path;
		}
		
		Vector3[] subdivided = new Vector3[(path.Length-2)*(int)Mathf.Pow(2,iterations)+2];
		Vector3[] subdivided2 = new Vector3[(path.Length-2)*(int)Mathf.Pow(2,iterations)+2];
		
		for (int i=0;i<path.Length;i++) {
			subdivided[i] = path[i];
		}
		
		
		for (int iteration=0;iteration < iterations; iteration++) {
			int currentPathLength = (path.Length-2)*(int)Mathf.Pow(2,iteration)+2;
			
			//Switch the arrays
			Vector3[] tmp = subdivided;
			subdivided = subdivided2;
			subdivided2 = tmp;
			
			float nextMultiplier = 1F;
			
			for (int i=0;i<currentPathLength-1;i++) {
				Vector3 current = subdivided2[i];
				Vector3 next = subdivided2[i+1];
				
				Vector3 normal = Vector3.Cross (next-current,Vector3.up);
				normal = normal.normalized;
				
				//This didn't work very well, made the path jaggy
				/*Vector3 dir = next-current;
				dir *= strength*0.5F;
				current += dir;
				next -= dir;*/
				
				bool firstRight = false;
				bool secondRight = false;
				bool setFirst = false;
				bool setSecond = false;
				if (i != 0 && !Polygon.IsColinear (current,next, subdivided2[i-1])) {
					setFirst = true;
					firstRight = Polygon.Left (current,next, subdivided2[i-1]);
				}
				if (i < currentPathLength-1 && !Polygon.IsColinear (current,next, subdivided2[i+2])) {
					setSecond = true;
					secondRight = Polygon.Left (current,next,subdivided2[i+2]);
				}
				
				if (setFirst) {
					subdivided[i*2] = current + (firstRight ? normal*offset*nextMultiplier : -normal*offset*nextMultiplier);
				} else {
					subdivided[i*2] = current;
				}
				
				//Didn't work very well
				/*if (setFirst && setSecond) {
					if (firstRight != secondRight) {
						nextMultiplier = 0.5F;
					} else {
						nextMultiplier = 1F;
					}
				}*/
				
				if (setSecond) {
					subdivided[i*2+1] = next  + (secondRight ? normal*offset*nextMultiplier : -normal*offset*nextMultiplier);
				} else {
					subdivided[i*2+1] = next;
				}
			}
			
			subdivided[(path.Length-2)*(int)Mathf.Pow(2,iteration+1)+2-1] = subdivided2[currentPathLength-1];
		}
		
		
		return subdivided;
	}
	
	public Vector3[] SmoothSimple (Vector3[] path) {
		
		if (path.Length <= 2) {
			return path;
		}
		
		if (uniformLength) {
			int numSegments = 0;
			maxSegmentLength = maxSegmentLength < 0.005F ? 0.005F : maxSegmentLength;
			
			for (int i=0;i<path.Length-1;i++) {
				float length = Vector3.Distance (path[i],path[i+1]);
				
				numSegments += Mathf.FloorToInt (length / maxSegmentLength);
			}
			
			Vector3[] subdivided = new Vector3[numSegments+1];
			
			int c = 0;
			
			for (int i=0;i<path.Length-1;i++) {
				
				float length = Vector3.Distance (path[i],path[i+1]);
				
				int numSegmentsForSegment = Mathf.FloorToInt (length / maxSegmentLength);
				
				//float t = 1F / numSegmentsForSegment;
				
				Vector3 dir = path[i+1] - path[i];
				
				for (int q=0;q<numSegmentsForSegment;q++) {
					//Debug.Log (q+" "+c+" "+numSegments+" "+length+" "+numSegmentsForSegment);
					subdivided[c] = dir*((float)q/numSegmentsForSegment) + path[i];
					c++;
				}
			}
			
			subdivided[c] = path[path.Length-1];
			
			
			for (int it = 0; it < iterations; it++) {
				Vector3 prev = subdivided[0];
				
				for (int i=1;i<subdivided.Length-1;i++) {
					
					Vector3 tmp = subdivided[i];
					
					subdivided[i] = Vector3.Lerp (tmp, (prev+subdivided[i+1])/2F,strength);
					
					prev = tmp;
				}
			}
			
			return subdivided;
		} else {
			Vector3[] subdivided = Polygon.Subdivide (path,subdivisions);
			
			for (int it = 0; it < iterations; it++) {
				Vector3 prev = subdivided[0];
				
				for (int i=1;i<subdivided.Length-1;i++) {
					
					Vector3 tmp = subdivided[i];
					
					subdivided[i] = Vector3.Lerp (tmp, (prev+subdivided[i+1])/2F,strength);
					
					prev = tmp;
				}
			}
			return subdivided;
		}
	}
	
	public Vector3[] SmoothBezier (Vector3[] path) {
		float subMult = Mathf.Pow (2,subdivisions);
		Vector3[] subdivided = new Vector3[(path.Length-1)*(int)subMult+1];
		
		
		for (int i=0;i<path.Length-1;i++) {
			
			Vector3 tangent1 = Vector3.zero;
			Vector3 tangent2 = Vector3.zero;
			if (i == 0) {
				tangent1 = path[i+1]-path[i];
			} else {
				tangent1 = path[i+1]-path[i-1];
			}
			
			if (i == path.Length-2) {
				tangent2 = path[i]-path[i+1];
			} else {
				tangent2 = path[i]-path[i+2];
			}
			
			tangent1 *= bezierTangentLength;
			tangent2 *= bezierTangentLength;
			
			Vector3 v1 = path[i];
			Vector3 v2 = v1+tangent1;
			Vector3 v4 = path[i+1];
			Vector3 v3 = v4+tangent2;
			
			Debug.DrawLine (v1,v4,Color.black);
			Debug.DrawLine (v1,v2,Color.red);
			Debug.DrawLine (v4,v3,Color.red);
			
			for (int j=0;j<(int)subMult;j++) {
				subdivided[i*(int)subMult+j] = Mathfx.CubicBezier (v1,v2,v3,v4, j/subMult);
				Debug.DrawLine (Vector3.Lerp (v1,v4,j/subMult),subdivided[i*(int)subMult+j],Color.blue);
			}
		}
		
		//Assign the last point
		subdivided[subdivided.Length-1] = path[path.Length-1];
		
		return subdivided;
	}
	
}