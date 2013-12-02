//#define DEBUGGING
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	//To be able to serialize modifiers, we store it in a holder which can contain any modifier type
	/*[System.Serializable]
	public class ModifierHolder {
		public int activeModifier;
		
		public Modifier defaultMod;
		public FunnelModifier funnelMod;
		
		public SimpleSmoothModifier simpleSmoothMod;
		
		public Modifier GetModifier () {
			
			switch (activeModifier) {
				case 0:
					return defaultMod;
				case 1:
					return funnelMod;
				case 2:
					return simpleSmoothMod;
			}
			return defaultMod;
		}
		
		public Vector3[] Apply (Node[] path, Vector3 start, Vector3 end, int startIndex, int endIndex, NavGraph graph) {
			
			return GetModifier ().Apply (path,start,end, startIndex, endIndex, graph);
		}
		
		public Vector3[] Apply (Vector3[] path, Vector3 start, Vector3 end) {
			
			return GetModifier ().Apply (path, start, end);
		}
	}*/
	
	[System.Flags]
	public enum ModifierData {
		All					= -1,		/**< All bits set to 1 */
		StrictNodePath 		= 1 << 0,	/**< Node array with original length */
		NodePath			= 1 << 1,	/**< Node array */
		StrictVectorPath	= 1 << 2,	/**< Vector path with original length (same as node path array length). Think of it as: the node positions have changed */
		VectorPath			= 1 << 3,	/**< Vector path */
		Original			= 1 << 4,	/**< Used when the modifier requires to be the first in the list (or after a modifier outputting #All) */
		None				= 0,		/**< Zero (no bits true) */
		Nodes				= NodePath | StrictNodePath, /**< Combine of NodePath and StrictNodePath */
		Vector				= VectorPath | StrictVectorPath /**< Combine of VectorPath and StrictVectorPath */
	}
	
	/** Base for all path modifiers.
	 * \see MonoModifier
	 * Modifier */
	public interface IPathModifier {
		int Priority {
			get;
			set;
		}
		
		ModifierData input { get; }
		ModifierData output { get; }
		
		void ApplyOriginal (Path p);
		void Apply (Path p, ModifierData source);
		void PreProcess (Path p);
	}
	
	/** Base class for path modifiers which are not attached to GameObjects.
	 * \see MonoModifier */
	[System.Serializable]
	public abstract class Modifier : IPathModifier {
		
		/** Higher priority modifiers are executed first */
		public int priority = 0;
		
		[System.NonSerialized]
		public Seeker seeker;
		
		public abstract ModifierData input { get; }
		public abstract ModifierData output { get; }
		
		public int Priority {
			get {
				return priority;
			}
			set {
				priority = value;
			}
		}
		
		public void Awake (Seeker s) {
			seeker = s;
			if (s != null) {
				s.RegisterModifier (this);
			}
		}
		
		public void OnDestroy (Seeker s) {
			if (s != null) {
				s.DeregisterModifier (this);
			}
		}
		
		public virtual void ApplyOriginal (Path p) {
		}
		
		public abstract void Apply (Path p, ModifierData source);
		
		public virtual void PreProcess (Path p) {
		}
	}
	
	/** Base class for path modifiers which can be attached to GameObjects.
	  * \see Modifier */
	[System.Serializable]
	public abstract class MonoModifier : MonoBehaviour, IPathModifier {
		
		[System.NonSerialized]
		public Seeker seeker;
		
		/** Higher priority modifiers are executed first */
		public int priority = 0;
		
		public int Priority {
			get {
				return priority;
			}
			set {
				priority = value;
			}
		}
		
		public abstract ModifierData input { get; }
		public abstract ModifierData output { get; }
		
		public void Awake () {
			seeker = GetComponent<Seeker>();
			
			if (seeker != null) {
				seeker.RegisterModifier (this);
				//seeker.postProcessOriginalPath += new OnPathDelegate (ApplyOriginal);
				//seeker.postProcessPath += new OnPathDelegate (Apply);
				//seeker.getNextTarget += new GetNextTargetDelegate (GetNextTarget);
			}
		}
		
		public void OnDestroy () {
			
			if (seeker != null) {
				seeker.DeregisterModifier (this);
				//seeker.postProcessOriginalPath -= new OnPathDelegate (ApplyOriginal);
				//seeker.postProcessPath -= new OnPathDelegate (Apply);
				//seeker.getNextTarget -= new GetNextTargetDelegate (GetNextTarget);
			}
		}
		
		public virtual void ApplyOriginal (Path p) {
			//Debug.Log ("Base call");
		}
		
		public abstract void Apply (Path p, ModifierData source);
		
		public virtual void PreProcess (Path p) {
		}
		
		public virtual Vector3[] GetNextTarget (Path p, Vector3 currentPosition) {
			return p.vectorPath;
			//Debug.Log ("Base call");
		}
		
		//This is for the first pass of original data modifiers
		public virtual Vector3[] Apply (Node[] path, Vector3 start, Vector3 end, int startIndex, int endIndex, NavGraph graph) {
			
			Vector3[] p = new Vector3[endIndex-startIndex];
			
			for (int i=startIndex;i< endIndex;i++) {
				p[i-startIndex] = path[i].position;
			}
			
			return p;
		}
		
		//This is for all other position only modifiers (mostly smoothers)
		public virtual Vector3[] Apply (Vector3[] path, Vector3 start, Vector3 end) {
			return path;
		}
	}
	
	public class ModifierConverter {
		
		/** Returns If \a a has all bits that \a b has set to true, set to true */
		public static bool AllBits (ModifierData a, ModifierData b) {
			return (a & b) == b;
		}
		
		/** Returns If \a a and \a b has any bits in common */
		public static bool AnyBits (ModifierData a, ModifierData b) {
			return (a & b) != 0;
		}
		
		/** Converts a path from \a input to \a output */
		public static ModifierData Convert (Path p, ModifierData input, ModifierData output) {
			
			//"Input" can not be converted to "output", log error
			if (!CanConvert (input,output)) {
				Debug.LogError ("Can't convert "+input+" to "+output);
				return ModifierData.None;
			}
			
			//"Output" can take "input" with no change, return
			if (AnyBits (input,output)) {
				return input;
			}
			
			//If input is a node path, and output wants a vector array, convert the node array to a vector array
			if (AnyBits (input,ModifierData.Nodes) && AnyBits (output, ModifierData.Vector)) {
				p.vectorPath = new Vector3[p.path.Length];
				for (int i=0;i<p.vectorPath.Length;i++) {
					p.vectorPath[i] = p.path[i].position;
				}
				
				//Return VectorPath and also StrictVectorPath if input has StrictNodePath set
				return ModifierData.VectorPath | (AnyBits (input, ModifierData.StrictNodePath) ? ModifierData.StrictVectorPath : ModifierData.None);
			}
			
			Debug.LogError ("This part should not be reached - Error in ModifierConverted\nInput: "+input+" ("+(int)input+")\nOutput: "+output+" ("+(int)output+")");
			return ModifierData.None;
		}
		
		/** Returns If \a input can be converted to \a output */
		public static bool CanConvert (ModifierData input, ModifierData output) {
			ModifierData convert = CanConvertTo (input);
			return AnyBits (output,convert);
		}
		
		/** Returns All data types \a a can be converted to */
		public static ModifierData CanConvertTo (ModifierData a) {
			
			if (a == ModifierData.All) {
				return ModifierData.All;
			}
			
			ModifierData result = a;
			
			if (AnyBits (a,ModifierData.Nodes)) {
				result |= ModifierData.VectorPath;
			}
			
			if (AnyBits (a,ModifierData.StrictNodePath)) {
				result |= ModifierData.StrictVectorPath;
			}
			
			if (AnyBits (a,ModifierData.StrictVectorPath)) {
				result |= ModifierData.VectorPath;
			}
			return result;
		}
	}
}