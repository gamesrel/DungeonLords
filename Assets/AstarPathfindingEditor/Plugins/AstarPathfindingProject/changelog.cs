/** \file
 * This file contains the changelog for the A* Pathfinding Project.
 */

/** \page changelog Changelog

This is the changelog, I will try to keep it updated.

- 3.0.6
	- Added support for a C++ version of Recast which means faster scanning times and more features (though almost no are available at the moment since I haven't added support for them yet).
	- Removed the overload AstarData.AddGraph (string type, NavGraph graph) since it was obsolete. AstarData::AddGraph (Pathfinding::NavGraph) should be used now.
	- Fixed a few bugs in the FunnelModifier which could cause it to return invalid paths
	- A reference image can now be generated for the Use Texture option for Grid Graphs
	- Fixed an editor bug with graphs which had no editors
	- Graphs with no editors now show up in the Add New Graph list to show that they have been found, but they cannot be used
	- Deleted the \a graphIndex parameter in the Pathfinding::NavGraph::Scan function. If you need to use it in your graph's Scan function, get it using Pathfinding::AstarData::GetGraphIndex
	- Javascript support! At last you can use Js code with the A* Pathfinding Project! Go to A* Inspector-->Settings-->Editor-->Enable Js Support to enable it
	- The Dark Skin is now automatically used if the rest of Unity uses the dark skin(hopefully)
- 3.0.5
	- \link Pathfinding::ListGraph List Graphs\endlink now support UpdateGraphs. This means that they for example can be used with the DynamicObstacle script.
	- List Graphs can now gather nodes based on GameObject tags instead of all nodes as childs of a specific GameObject.
	- List Graphs can now search recursively for childs to the 'root' GameObject instead of just searching through the top-level children.
	- Added custom area colors which can be edited in the inspector (A* inspector --> Settings --> Color Settings --> Custom Area Colors)
	- Fixed a NullReference bug which could ocurr when loading a Unity Reference with the AstarSerializer.
	- Fixed some bugs with the FleePath and RandomPath which could cause the StartEndModifier to assign the wrong endpoint to the path.
	- Documentation is now more clear on what is A* Pathfinding Project Pro only features.
	- Pathfinding::NNConstraint now has a variable to constrain which graphs to search (A* Pro only).\n
	  This is also available for Pathfinding::GraphUpdateObject which now have a field for an NNConstraint where it can constrain which graphs to update.
	- StartPath calls on the Seeker can now take a parameter specifying which graphs to search for close nodes on (A* Pro only)
	- Added the delegate AstarPath::OnAwakeSettings which is called as the first thing in the Awake function, can be used to set up settings.
	- Pathfinding::UserConnection::doOverrideCost is now serialized correctly. This represents the toggle to the right of the "Cost" field when editing a link.
	- Fixed some bugs with the RecastGraph when spans were partially out-of-bounds, this could generate seemingly random holes in the mesh
- 3.0.4 (only pro version affected)
	- Added a Dark Skin for Unity Pro users (though it is available to Unity Free users too, even though it doesn't look very good).
	  It can be enabled through A* Inspector --> Settings --> Editor Settings --> Use Dark Skin
	- Added option to include or not include out of bounds voxels (Y axis below the graph only) for Recast graphs.
- 3.0.3 (only pro version affected)
	- Fixed a NullReferenceException caused by Voxelize.cs which could surface if there were MeshFilters with no Renderers on GameObjects (Only Pro version affected)
- 3.0.2
	- Textures can now be used to add penalty, height or change walkability of a Grid Graph (A* Pro only)
	- Slope can now be used to add penalty to nodes
	- Height (Y position) can now be usd to add penalty to nodes
	- Prioritized graphs can be used to enable prioritizing some graphs before others when they are overlapping
	- Several bug fixes
	- Included a new DynamicGridObstacle.cs script which can be attached to any obstacle with a collider and it will update grids around it to account for changed position
- 3.0.1
	- Fixed Unity 3.3 compability
- 3.0
	- Rewrote the system from scratch
	- Funnel modifier
	- Easier to extend the system
 */