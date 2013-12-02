using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

namespace Pathfinding {
//Mem - 4+1+4+1+[4]+[4]+1+1+4+4+4+4+4+  12+12+12+12+12+12+4+4+4+4+4+1+1+(4)+4+4+4+4+4+4+4 ? 166 bytes

	/** Basic path, finds the shortest path from A to B.
	 * This is the most basic path object it will try to find the shortest path from A to B.\n
	 * All other path types inherit from this type.
	 * \see Seeker::StartPath
	 */
	public class Path {
		
		/** Callback to call when the path is complete.
		 * This is usually sent to the Seeker component which post processes the path and then calls a callback to the script which requested the path 
		*/
		public OnPathDelegate callback;
		
		[System.Obsolete]
		public OnPathDelegate preCallback;
		
		/** If the path failed, this is true.
		  * \see #errorLog */
		public bool error = false;
		
		/** Additional info on what went wrong.
		  * \see #error */
		public string errorLog;
		
		/** Is the path completed? */
		public bool foundEnd = false;
		
		/** Holds the path as a Node array. All nodes the path traverses. This might not be the same as all nodes the smoothed path traverses. */
		public Node[] path;
		
		/** Holds the (eventually smoothed) path as a Vector3 array */
		public Vector3[] vectorPath;
		
		public bool forceStartSnap;
		public bool forceEndSnap;
		
		/** The max number of milliseconds per iteration (frame) */
		protected float maxFrameTime;
		
		public Node startNode; /**< Start node of the path */
		public Node endNode;   /**< End node of the path */
		
		/** The node currently being processed */
		protected Node current;
		
		/** Hints can be set to enable faster Get Nearest Node queries. Only applies to some graph types */
		public Node startHint;
		/** Hints can be set to enable faster Get Nearest Node queries. Only applies to some graph types */
		public Node endHint;
		
		public Vector3 originalStartPoint;
		public Vector3 originalEndPoint;
		
		public Vector3 startPoint;
		public Vector3 endPoint;
		
		public Int3 startIntPoint;
		public Int3 hTarget; /**< Target to use for H score calculations. \see Pathfinding::Node::h */
		
		/** Reference to the open list. Shared between paths */
		public BinaryHeap open;
		
		/** Number of ms of computation time for this path */ 
		public float duration;
		public int searchIterations = 0;
		public int searchedNodes;
		
		/** When the call was made to start the pathfinding for this path */
		public System.DateTime callTime;
		
		/** True if the path has been calculated (even if it had an error). Used by the multithreaded pathfinder to signal that this path object is safe to return. */
		public bool processed = false;
		public bool multithreaded = false;
		
		/** Constraint for how to search for nodes */
		public NNConstraint nnConstraint = PathNNConstraint.Default;
		
		/** The next path to be searched.
		 * Linked list implementation. You should never change this if you do not know what you are doing */
		public Path next;
		
		
		//These are variables which different scripts and custom graphs can use to get a bit more info about What is searching
		//Not all are used in the standard graph types
		//These variables are put here because it is a lot faster to access fields than, for example make use of a lookup table (e.g dictionary)
		//Note: These variables needs to be filled in by an external script to be usable
		public int radius;
		/** A mask for defining what type of ground a unit can traverse, not used in any default standard graph */
		public int walkabilityMask = -1;
		public int height;
		public int turnRadius;
		public int speed;
		
		/** To store additional data. Note: this is SLOW. About 10-100 times slower than using the fields above */
		public Dictionary<string,int> customData = null;//new Dictionary<string,int>();
		
		/** Determines which heuristic to use */
		public Heuristic heuristic;
		/** Scale of the heuristic values */
		public float heuristicScale = 1F;
		
		/** ID of this path. Used to distinguish between different paths */
		public int pathID;
		
		public void LogError (string msg) {
			error = true;
			errorLog += msg;
			
			if (AstarPath.active.logPathResults != PathLog.None && AstarPath.active.logPathResults != PathLog.InGame) {
				Debug.LogWarning (msg);
			}
		}
		
		public Path () {}
		
		public Path (Vector3 start, Vector3 end, OnPathDelegate callbackDelegate) {
			Reset (start,end,callbackDelegate,false);
		}
		
		public virtual void UpdateStartEnd (Vector3 start, Vector3 end) {
			originalStartPoint = start;
			originalEndPoint = end;
			
			startPoint = start;
			endPoint = end;
			
			startIntPoint = (Int3)start;
			hTarget = (Int3)end;
		}
		
		public virtual void Reset (Vector3 start, Vector3 end, OnPathDelegate callbackDelegate, bool reset = true) {
			
			if (reset) {
				customData = null;
				processed = false;
				vectorPath = null;
				path = null;
				next = null;
				foundEnd = false;
				error = false;
				errorLog = "";
				callback = null;
				current = null;
				duration = 0;
				searchIterations = 0;
				searchedNodes = 0;
				
				startHint = null;
				endHint = null;
			}
			
			callTime = System.DateTime.Now;
			
			callback = callbackDelegate;
			
			if (AstarPath.active == null || AstarPath.active.graphs == null) {
				errorLog += "No NavGraphs have been calculated yet - Don't run any pathfinding calls in Awake";
				if (AstarPath.active.logPathResults != PathLog.None) {
					Debug.LogWarning (errorLog);
				}
				error = true;
				return;
			}
			
			pathID = AstarPath.active.GetNextPathID ();
			
			UpdateStartEnd (start,end);
			
			heuristic = AstarPath.active.heuristic;
			heuristicScale = AstarPath.active.heuristicScale;
			
		}
		
		public virtual void Prepare () {
			
			System.DateTime startTime = System.DateTime.Now;
			
			
			
			//@pathStartTime = startTime;
			
			maxFrameTime = AstarPath.active.maxFrameTime;
			
			//maxAngle = NmaxAngle;
			//angleCost = NangleCost;
			//stepByStep = NstepByStep;
			//unitRadius = 0;//BETA, Not used
			NNInfo startNNInfo 	= AstarPath.active.GetNearest (startPoint,nnConstraint, startHint);
			
			//Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			PathNNConstraint pathNNConstraint = nnConstraint as PathNNConstraint;
			if (pathNNConstraint != null) {
				pathNNConstraint.SetStart (startNNInfo.node);
			}
			
			NNInfo endNNInfo	= AstarPath.active.GetNearest (endPoint,nnConstraint, endHint);
			
			startPoint = startNNInfo.clampedPosition;
			endPoint = endNNInfo.clampedPosition;
			
			startIntPoint = (Int3)startPoint;
			hTarget = (Int3)endPoint;
			
			startNode = startNNInfo.node;
			endNode = endNNInfo.node;
			
			
			if (startNode == null || endNode == null) {
				LogError ("Couldn't find close nodes to either the start or the end (start = "+(startNode != null)+" end = "+(endNode != null)+")");
				duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
				return;
			}
			
			if (!startNode.walkable) {
				LogError ("The node closest to the start point is not walkable");
				
				duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
				return;
			}
			
			if (!endNode.walkable) {
				LogError ("The node closest to the start point is not walkable");
				
				duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
				return;
			}
			
			if (startNode.area != endNode.area) {
				LogError ("There is no valid path to the target (start area: "+startNode.area+", target area: "+endNode.area+")");
				//Debug.DrawLine (startNode.position,endNode.position,Color.cyan);
				duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
				return;
			}
			
			duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
			
		}
		
		/** Saved original costs for the end node. \see ResetCosts */
		int[] endNodeCosts;
		
		public virtual void Initialize () {
			
			System.DateTime startTime = System.DateTime.Now;
			
			//@float startTime = Time.realtimeSinceStartup;
			
			//Resets the binary heap, don't clear it because that takes an awful lot of time, instead we can just change the numberOfItems in it (which is just an int)
			//Binary heaps are just like a standard array but are always sorted so the node with the lowest F value can be retrieved faster
			
			open = AstarPath.active.binaryHeap;
			open.numberOfItems = 1;
			
			if (startNode == endNode) {
				endNode.parent = null;
				endNode.h = 0;
				endNode.g = 0;
				Trace (endNode);
				foundEnd = true;
				
				//When using multithreading, this signals that another function should call the callback function for this path		
				//sendCompleteCall = true;
				return;
			}
			
			//Adjust the costs for the end node
			endNodeCosts = endNode.InitialOpen (open,hTarget,(Int3)endPoint,this,false);
			callback += ResetCosts; /** \todo Might interfere with other paths since other paths might be calculated before #callback is called */
			
			Node.activePath = this;
			startNode.pathID = pathID;
			startNode.parent = null;
			startNode.cost = 0;
			startNode.g = startNode.penalty;
			startNode.UpdateH (hTarget,heuristic,heuristicScale);
			
			startNode.InitialOpen (open,hTarget,startIntPoint,this,true);
			searchedNodes++;
			
			//any nodes left to search?
			if (open.numberOfItems <= 1) {
				LogError ("No open points, the start node didn't open any nodes");
				
				duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
				return;
			}
			
			current = open.Remove ();
			
			duration += (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
			
		}
		
		/*public void CalculateStepMultiple (bool randomVar) {
			
			//@float startTime = Time.realtimeSinceStartup;
			//start.script = this;
			//start.parent = null;
			
			int searchedNodesThisFrame = 0;
			//Continue to search while there hasn't ocurred an error and the end hasn't been found
			while (!foundEnd && !error) {
				
				//Close the current node, if the current node is the target node then the path is finnished
				for (int i=0;i<endNodes.Length;i++) {
					if (current == endNodes[i]) {
						endsFound++;
						Trace (current);
						
						if (seekers[i] != null) {
							seekers[i].OnComplete (this);
						}
					}
					
					if (endsFound == endNodes.Length) {
						break;
					}
				}
				
				if (current == null) {
					Debug.LogWarning ("Current is Null");
					return;
				}
				
				//Debug.DrawRay ( current.position, Vector3.up*10,Color.cyan);
				
				//@Performance Just for debug info
				searchedNodes++;
				
				searchedNodesThisFrame++;
				//Loop through all walkable neighbours of the node
				
				current.Open (open, endNode,this);
				
				//any nodes left to search?
				if (open.numberOfItems <= 1) {
					Debug.LogWarning ("No open points, whole area searched");
					error = true;
					return;
				}
				
				//Select the node with the lowest F score and remove it from the open array
				current = open.Remove ();
				
				//Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
				//if (Time.realtimeSinceStartup-startTime >= maxFrameTime) {
				if (searchedNodesThisFrame > 1000) {
					//@duration += Time.realtimeSinceStartup-startTime;
					
					//Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
					return;
				}
			
			}
			
			if (foundEnd && !error) {
				Trace (endNode);
			}
			//@duration += Time.realtimeSinceStartup-startTime;
			
			//Send the computed path to the seeker
			if (seeker != null) {
				seeker.OnComplete (this);
			}
	
		}*/
		
		public virtual float CalculateStep (float remainingFrameTime) {
			
			System.DateTime startTime = System.DateTime.Now;
			
			System.Int64 maxTicks = (System.Int64)(remainingFrameTime*10000);
			
			
			//start.script = this;
			//start.parent = null;
			
			
			int counter = 0;
			
			//Continue to search while there hasn't ocurred an error and the end hasn't been found
			while (!foundEnd && !error) {
				
				//@Performance Just for debug info
				searchedNodes++;
				
				//Close the current node, if the current node is the target node then the path is finnished
				if (current == endNode) {
					foundEnd = true;
					break;
				}
				
				/*if (current == null) {
					Debug.LogWarning ("Current is Null");
					return;
				}*/
				
				//Debug.DrawRay ( current.position, Vector3.up*10,Color.cyan);
				
				
				//Loop through all walkable neighbours of the node
				
				current.Open (open, hTarget,this);
				
				//any nodes left to search?
				if (open.numberOfItems <= 1) {
					LogError ("No open points, whole area searched");
					
					float durationThisFrame = (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
					duration += durationThisFrame;
						
					return durationThisFrame;
				}
				
				//Select the node with the lowest F score and remove it from the open list
				current = open.Remove ();
				
				//Check for time every 500 nodes
				if (counter > 500) {
					
					//Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
					if ((System.DateTime.Now.Ticks-startTime.Ticks) > maxTicks) {//searchedNodesThisFrame > 20000) {
						
						
						float durationThisFrame = (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
						duration += durationThisFrame;
						
						//Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
						return durationThisFrame;
					}
					
					counter = 0;
				}
				
				counter++;
			
			}
			
			if (foundEnd && !error) {
				Trace (endNode);
			}
			
			float durationThisFrame2 = (System.DateTime.Now.Ticks-startTime.Ticks)*0.0001F;
			duration += durationThisFrame2;
			
			//When using multithreading, this signals that another function should call the callback function for this path		
			//sendCompleteCall = true;
			
			//Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
			return durationThisFrame2;
		}
		
		/** Resets End Node Costs. Costs are updated on the end node at the start of the search to better reflect the end point passed to the path, the previous ones are saved in #endNodeCosts and are reset in this function which is called after the path search is complete */
		public void ResetCosts (Path p) {
			endNode.ResetCosts (endNodeCosts);
		}
		
		/** Traces the calculated path from the end node to the start. This will build an array (Path.path) of the nodes this path will pass through.
		 */
		public virtual void Trace (Node from) {
			
			int count = 0;
			
			Node c = from;
			while (c != null) {
				c = c.parent;
				count++;
				if (count > 1024) {
					Debug.LogWarning ("Inifinity loop? >1024 node path");
					break;
				}
			}
			
			path = new Node[count];
			c = from;
			
			for (int i = 0;i<count;i++) {
				
				if (c == null) {
					Debug.LogError ("C == NULL");
				}
				
				path[count-1-i] = c;
				
				c = c.parent;
				
				
			}
			
			vectorPath = new Vector3[count];
			
			for (int i=0;i<count;i++) {
				if (path[i] == null) {
					Debug.LogError ("C == NULL "+i+" "+count);
				}
				
				vectorPath[i] = path[i].position;
			}
		}
		
		public static System.Text.StringBuilder debugStringBuilder = new System.Text.StringBuilder ();
			
		/** Returns a debug string for this path. \note This function should NOT be called simultaneously from multiple threads since that could mess up the static StringBuilder used */
		public virtual string DebugString (PathLog logMode) {
			
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}
			
			debugStringBuilder.Length = 0;
			
			System.Text.StringBuilder text = debugStringBuilder;
			
			//if (active.logPathResults == PathLog.Normal || logPathResults == PathLog.OnlyErrors) {
				text.Append (error ? "Path Failed : " : "Path Completed : ");
				text.Append ("Computation Time ");
				
				text.Append ((duration).ToString (logMode == PathLog.Heavy ? "0.000" : "0.00"));
				text.Append (" ms Searched Nodes ");
				text.Append (searchedNodes);
				
				if (!error) {
					text.Append (" Path Length ");
					text.Append (path == null ? "Null" : path.Length.ToString ());
				
					if (logMode == PathLog.Heavy) {
						text.Append ("\nSearch Iterations "+searchIterations);
						text.Append ("\nEnd Node\n	G: ");
						text.Append (endNode.g);
						text.Append ("\n	H: ");
						text.Append (endNode.h);
						text.Append ("\n	F: ");
						text.Append (endNode.f);
						text.Append ("\n	Point: ");
						text.Append (((Vector3)endPoint).ToString ());
						text.Append ("\n	Graph: ");
						text.Append (endNode.graphIndex);
						
						text.Append ("\nStart Node");
						text.Append ("\n	Point: ");
						text.Append (((Vector3)endPoint).ToString ());
						text.Append ("\n	Graph: ");
						text.Append (startNode.graphIndex);
						text.Append ("\nBinary Heap size at completion: ");
						text.Append (open == null ? "Null" : (open.numberOfItems-2).ToString ());// -2 because numberOfItems includes the next item to be added and item zero is not used
					}
					
					/*"\nEnd node\n	G = "+p.endNode.g+"\n	H = "+p.endNode.h+"\n	F = "+p.endNode.f+"\n	Point	"+p.endPoint
					+"\nStart Point = "+p.startPoint+"\n"+"Start Node graph: "+p.startNode.graphIndex+" End Node graph: "+p.endNode.graphIndex+
					"\nBinary Heap size at completion: "+(p.open == null ? "Null" : p.open.numberOfItems.ToString ())*/
				}
				
				if (error) {
					text.Append ("\nError: ");
					text.Append (errorLog);
				}
				
				text.Append ("\nPath Number ");
				text.Append (pathID);
				
				return text.ToString ();
				/*if (p.error) {
					debug = "Path Failed : Computation Time: "+(p.duration).ToString ("0.00")+" ms Searched Nodes "+p.searchedNodes+"\nPath number: "+PathsCompleted+"\nError: "+p.errorLog;
				} else {
					debug = "Path Completed : Computation Time: "+(p.duration).ToString ("0.00")+" ms Path Length "+(p.path == null ? "Null" : p.path.Length.ToString ()) + " Searched Nodes "+p.searchedNodes+"\nSmoothed path length "+(p.vectorPath == null ? "Null" : p.vectorPath.Length.ToString ())+"\nPath number: "+p.pathID;
				}*/
				
			/*} else if (logPathResults == PathLog.Heavy || logPathResults == PathLog.InGame || logPathResults == PathLog.OnlyErrors) {
				
				if (p.error) {
					debug = "Path Failed : Computation Time: "+(p.duration).ToString ("0.000")+" ms Searched Nodes "+p.searchedNodes+"\nPath number: "+PathsCompleted+"\nError: "+p.errorLog;
				} else {
					debug = "Path Completed : Computation Time: "+(p.duration).ToString ("0.000")+" ms\nPath Length "+(p.path == null ? "Null" : p.path.Length.ToString ()) + "\nSearched Nodes "+p.searchedNodes+"\nSearch Iterations (frames) "+p.searchIterations+"\nSmoothed path length "+(p.vectorPath == null ? "Null" : p.vectorPath.Length.ToString ())+"\nEnd node\n	G = "+p.endNode.g+"\n	H = "+p.endNode.h+"\n	F = "+p.endNode.f+"\n	Point	"+p.endPoint
					+"\nStart Point = "+p.startPoint+"\n"+"Start Node graph: "+p.startNode.graphIndex+" End Node graph: "+p.endNode.graphIndex+"\nBinary Heap size at completion: "+(p.open == null ? "Null" : p.open.numberOfItems.ToString ())+"\nPath number: "+p.pathID;
				}
				
				/*if (active.logPathResults == PathLog.Heavy) {
					Debug.Log (debug);
				} else {
					inGameDebugPath = debug;
				}*
			}
			
			if (logPathResults == PathLog.Normal || logPathResults == PathLog.Heavy || (logPathResults == PathLog.OnlyErrors && p.error)) {
				Debug.Log (debug);
			} else if (logPathResults == PathLog.InGame) {
				inGameDebugPath = debug;
			}*/
		}
		
		/** Calls callback to return the calculated path. \see #callback */
		public virtual void ReturnPath () {
			if (callback != null) {
				callback (this);
			}
		}
		
		//Movement stuff
		
		public Vector3 GetMovementVector (Vector3 point) {
			
			if (vectorPath == null || vectorPath.Length < 2) {
				return Vector3.zero;
			}
			
			float minDist = float.PositiveInfinity;//Mathf.Infinity;
			int minSegment = 0;
			
			for (int i=0;i<vectorPath.Length-1;i++) {
				
				Vector3 closest = Mathfx.NearestPointStrict (vectorPath[i],vectorPath[i+1],point);
				float dist = (closest-point).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					minSegment = i;
				}
			}
			
			return vectorPath[minSegment+1]-point;
		}
		
	}
}