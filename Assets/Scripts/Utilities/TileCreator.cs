using UnityEngine;
using System.Collections;

public static class TileCreator {
    private static int goID = 0;
    /* Only used for tiles*/
    private static readonly int subdivXZ = 3;//number of subdivisions per tile
    private static readonly int subdivY = 6;//number of subdivisions for walls
    private static readonly int tileSize = 1;

    private static readonly float tileCenter = tileSize/2.0f;//to place the pivot in the center	
    private static readonly float displacement = (float)tileSize/(float)subdivXZ;
    private static readonly float displacementWallUV = 1.0f/(float)subdivY;
    private static readonly float displacementWallSize = (float) GLOBAL.WALLHEIGHT/(float)subdivY; 
    private static readonly float vertexRandomizationFactor = 0.05f;
    /* used for tile's meshes*/
    private static Vector3[] tileVertices = new Vector3[(subdivXZ+1)*(subdivXZ+1)];
    private static Vector3[] tileNormals = new Vector3[(subdivXZ+1)*(subdivXZ+1)];
    private static Vector4[] tileTangents = new Vector4[(subdivXZ+1)*(subdivXZ+1)];
    private static Vector2[] tileUvs = new Vector2[(subdivXZ+1)*(subdivXZ+1)];
    private static int[] tileTriangles = new int[subdivXZ*subdivXZ*6];

    /* used for Wall's meshes*/
    private static Vector3[] wallVertices = new Vector3[(subdivXZ+1)*(subdivY+1)];
    private static Vector3[] wallNormals = new Vector3[(subdivXZ+1)*(subdivY+1)];
    private static Vector4[] wallTangents = new Vector4[(subdivXZ+1)*(subdivY+1)];
    private static Vector2[] wallUvs = new Vector2[(subdivXZ+1)*(subdivY+1)];
    private static int[] wallTriangles = new int[subdivXZ*subdivY*6];

    private static int tmpV = 0;//temp vertex counter
    private static int tmpUV = 0;//temp uv counter
    private static int tmpT = 0;//temp triangle counter
    private static int optTile1; //optimization1
    private static int vertexSeed;//seed for calculating a random point in each vertex
    //private static Vector2 randomizedOffset;


    private static GameObject mainWorld;
    
    //Creates a tile of 1mx1m
    public static GameObject CreateTile(Vector3 position, Material m, string name = "") {
	
        GameObject floorObj = new GameObject((name == "") ? "Tile " + goID++ : name);
        floorObj.transform.position = position;	
        tmpV = tmpUV = tmpT = 0;
        for(int z = 0; z <= subdivXZ; z++) {
            for(int x = 0; x <= subdivXZ; x++) {
                tileNormals[tmpV] = Vector3.up;
                //tileTangents[tmpV] = new Vector4(0, 1, 0, -1);
                tileUvs[tmpUV++] = new Vector2(x*displacement,z*displacement);		
                //Random.seed = (int)(tmpObj.transform.TransformPoint(vertices[tmpV]).sqrMagnitude*10000.0f);
                //randomizedOffset = Random.insideUnitCircle * vertexRandomizationFactor;
                tileVertices[tmpV++] = new Vector3(x*displacement-tileCenter,// + randomizedOffset.x, 
                                                   0, 
                                                   z*displacement-tileCenter);// + randomizedOffset.y);
                //vertices[tmpV-1].y = 0;

                if(x == subdivXZ || z == subdivXZ)
                    continue;//goes to the next iteration of the for loop.
                //triangles
                optTile1 = (subdivXZ+1)*z+x;//optimization 1
                tileTriangles[tmpT++] = tileTriangles[tmpT+2] = optTile1;
                tileTriangles[tmpT++] = optTile1+(subdivXZ+1);
                tileTriangles[tmpT++] = tileTriangles[tmpT+1] = optTile1+(subdivXZ+1)+1;
                tmpT+=2;
                tileTriangles[tmpT++] = optTile1+1;
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = tileVertices;
        mesh.normals = tileNormals;
        mesh.uv = tileUvs;
        mesh.triangles = tileTriangles;
        mesh.tangents = tileTangents;

        floorObj.AddComponent<MeshFilter>().mesh = mesh;
        floorObj.AddComponent<MeshRenderer>().sharedMaterial = m;
        DistortionateVertexes(floorObj);
        floorObj.GetComponent<MeshFilter>().mesh.RecalculateNormals();


        if(!mainWorld)
            mainWorld = GameObject.Find("MainWorld");
        floorObj.transform.parent = mainWorld.transform;

        return floorObj;
    }
    
    public static GameObject CreateWall(WALL orientation, Vector3 pos, Material m, string wallName = "")
    {
        GameObject wall = new GameObject((wallName == "") ? "Wall " + goID++ : wallName);
        wall.transform.position = pos;
        tmpV = tmpUV = tmpT = 0;
        switch(orientation) {
        case WALL.UP:
            // WALL UP
            for(int y = 0; y <= subdivY; y++) {
                for(int x = 0; x <= subdivXZ; x++) {
                    wallNormals[tmpV] = -Vector3.forward;
                    wallUvs[tmpUV++] = new Vector2(x*displacement,y*displacementWallUV);		
                    wallVertices[tmpV++] = new Vector3(x*displacement-tileCenter, y*displacementWallSize, tileCenter);
		    
                    if(x == subdivXZ || y == subdivY)
                        continue;//goes to the next iteration of the for loop.
                    //triangles
                    optTile1 = (subdivXZ+1)*y+x;//optimization 1
                    wallTriangles[tmpT++] = wallTriangles[tmpT+2] = optTile1;
                    wallTriangles[tmpT++] = optTile1+(subdivXZ+1);
                    wallTriangles[tmpT++] = wallTriangles[tmpT+1] = optTile1+(subdivXZ+1)+1;
                    tmpT+=2;
                    wallTriangles[tmpT++] = optTile1+1;
                }
            }
            break;
        case WALL.DOWN:
            // WALL DOWN
            for(int y = 0; y <= subdivY; y++) {
                for(int x = 0; x <= subdivXZ; x++) {
                    wallNormals[tmpV] = Vector3.forward;
                    wallUvs[tmpUV++] = new Vector2(x*displacement,y*displacementWallUV);		
                    wallVertices[tmpV++] = new Vector3(x*displacement-tileCenter, y*displacementWallSize, -tileCenter);
		    
                    if(x == subdivXZ || y == subdivY)
                        continue;//goes to the next iteration of the for loop.
                    //triangles	
                    optTile1 = (subdivXZ+1)*y+x;//optimization 1
                    wallTriangles[tmpT++] = wallTriangles[tmpT+3] = optTile1+(subdivXZ+1)+1;
                    wallTriangles[tmpT++] = optTile1+(subdivXZ+1);
                    wallTriangles[tmpT++] = wallTriangles[tmpT+2] = optTile1;
                    wallTriangles[tmpT] = optTile1+1;
                    tmpT+=3;
                }
            }
            break;
        case WALL.LEFT:
            // WALL LEFT
            for(int y = 0; y <= subdivY; y++) {
                for(int z = 0; z <= subdivXZ; z++) {
                    wallNormals[tmpV] = Vector3.forward;
                    wallUvs[tmpUV++] = new Vector2(z*displacement,y*displacementWallUV);		
                    wallVertices[tmpV++] = new Vector3(-tileCenter, y*displacementWallSize, z*displacement-tileCenter);
		    
                    if(z == subdivXZ || y == subdivY)
                        continue;//goes to the next iteration of the for loop.
                    //triangles
                    optTile1 = (subdivXZ+1)*y+z;//optimization 1
                    wallTriangles[tmpT++] = wallTriangles[tmpT+2] = optTile1;
                    wallTriangles[tmpT++] = optTile1+(subdivXZ+1);
                    wallTriangles[tmpT++] = wallTriangles[tmpT+1] = optTile1+(subdivXZ+1)+1;
                    tmpT+=2;
                    wallTriangles[tmpT++] = optTile1+1;
                }
            }
            break;
        case WALL.RIGHT:
            //WALL RIGHT
            for(int y = 0; y <= subdivY; y++) {
                for(int z = 0; z <= subdivXZ; z++) {
                    wallNormals[tmpV] = -Vector3.right;
                    wallUvs[tmpUV++] = new Vector2(z*displacement,y*displacementWallUV);		
                    wallVertices[tmpV++] = new Vector3(tileCenter, y*displacementWallSize,z*displacement-tileCenter);
		    
                    if(z == subdivXZ || y == subdivY)
                        continue;//goes to the next iteration of the for loop.
                    //triangles
                    optTile1 = (subdivXZ+1)*y+z;//optimization 1
                    wallTriangles[tmpT++] = wallTriangles[tmpT+3] = optTile1+(subdivXZ+1)+1;
                    wallTriangles[tmpT++] = optTile1+(subdivXZ+1);
                    wallTriangles[tmpT++] = wallTriangles[tmpT+2] = optTile1;
                    wallTriangles[tmpT] = optTile1+1;
                    tmpT+=3;
                }
            }
            break;
        default:
            Debug.LogError("Not know wall orientation: " + orientation);
            break;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = wallVertices;
        mesh.normals = wallNormals;
        mesh.uv = wallUvs;
        mesh.triangles = wallTriangles;
        mesh.tangents = wallTangents;


        wall.AddComponent<MeshFilter>().mesh = mesh;
        wall.AddComponent<MeshRenderer>().sharedMaterial = m;
        DistortionateVertexes(wall);
        wall.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        if(!mainWorld)
            mainWorld = GameObject.Find("MainWorld");
        wall.transform.parent = mainWorld.transform;
	
        return wall;
    }


    // Used to randomize object's vertexes with the same random seed per vertex
    // so they match in the tiles.
    private static void DistortionateVertexes(GameObject obj) {
        Transform tr = obj.transform;
        Vector3[] vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
        Vector3 modifiedPos;
        //PUT THIS INSIDE THE CREATE TILE for loop FUNCTION!
        for(int k = 0; k < vertices.Length; k++) {

            Random.seed = (int)(tr.TransformPoint(vertices[k]).sqrMagnitude * 10000.0f);
            modifiedPos = Random.insideUnitSphere;
            vertices[k].x += modifiedPos.x*vertexRandomizationFactor;
            //vertices[k].y += newPosition.y * 0.2f;			
            vertices[k].z += modifiedPos.z*vertexRandomizationFactor;
        }
        obj.GetComponent<MeshFilter>().mesh.vertices = vertices;	
    }
}
