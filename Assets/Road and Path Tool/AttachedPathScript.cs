/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]

public class PathNodeObjects 
{
	public Vector3 position;
	public float width;
}

public struct TerrainPathCell
{
	public float heightAtCell;
	public Vector2 position;
	public bool isAdded;
};

public class AttachedPathScript : MonoBehaviour 
{
	// Array of terrain cells for convenience 
	public TerrainPathCell[] terrainCells;
	
	public bool addNodeMode;
	public bool isRoad;
	public bool isFinalized;
	
	public PathNodeObjects[] nodeObjects;
	public Vector3[] nodeObjectVerts; // keeps vertice positions for handles
	
	public GameObject pathMesh;
	public MeshCollider pathCollider;
	
	// central terrian cells
	public ArrayList pathCells;
	public ArrayList totalPathVerts;
	public ArrayList innerPathVerts;
	
	// GUI variables
	public int pathWidth;
	public int pathTexture;
	public bool pathUniform;
	public bool pathFlat;
	public bool showHandles;
	public float pathWear;
	public int pathSmooth;
	
	// name of the terrain that created the path
	public GameObject parentTerrain;
	
	public GameObject terrainObj;
	public Terrain terComponent;
	public TerrainData terData;
	public TerrainCollider terrainCollider;
	public float[,] terrainHeights;
	
	public void Awake()
	{
		terComponent = (Terrain) parentTerrain.GetComponent(typeof(Terrain));
		if(terComponent == null)
			Debug.LogError("This script must be attached to a terrain object - Null reference will be thrown");	
	}
	
	public void NewPath()
	{
		nodeObjects = new PathNodeObjects[0];
		pathCollider = (MeshCollider)pathMesh.AddComponent(typeof(MeshCollider));
		
		terrainObj = parentTerrain;
		terComponent = (Terrain) terrainObj.GetComponent(typeof(Terrain));
		
		if(terComponent == null)
			Debug.LogError("This script must be attached to a terrain object - Null reference will be thrown");
			
		terData = terComponent.terrainData;
		terrainHeights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		terrainCollider = (TerrainCollider)terrainObj.GetComponent(typeof(TerrainCollider));
	}
	
	public void CreatePathNode(TerrainPathCell nodeCell)
	{
		Vector3 pathPosition = new Vector3((nodeCell.position.x/terData.heightmapResolution) * terData.size.x, nodeCell.heightAtCell * terData.size.y , (nodeCell.position.y/terData.heightmapResolution) * terData.size.z);
		
		AddNode(pathPosition, pathWidth);
		
		if(pathFlat || isRoad)
			CreatePath(pathSmooth, true, false);
		
		else
			CreatePath(pathSmooth, false, false);
	}
	
	public void AddNode(Vector3 position, float width)
	{
		PathNodeObjects newPathNodeObject = new PathNodeObjects();
		int nNodes;
		
		if(nodeObjects == null)
		{
			nodeObjects = new PathNodeObjects[0];
			nNodes = 1;
			newPathNodeObject.position = position;
		}
		
		else
		{
			nNodes = nodeObjects.Length + 1;
			newPathNodeObject.position = position;
		}

		PathNodeObjects[] newNodeObjects = new PathNodeObjects[nNodes];
		newPathNodeObject.width = width;
		
		int n = newNodeObjects.Length;
		
		for (int i = 0; i < n; i++) 
		{
			if (i != n - 1) 
			{
				newNodeObjects[i] = nodeObjects[i];
			} 
			
			else 
			{
				newNodeObjects[i] = newPathNodeObject;
			}
		}
		
		nodeObjects = newNodeObjects;
	}
	
	public void CreatePath(int smoothingLevel, bool flatten, bool road)
	{
		MeshFilter meshFilter = (MeshFilter)pathMesh.GetComponent(typeof(MeshFilter));
		
		if(meshFilter == null)
			return;
		
		Mesh newMesh = meshFilter.sharedMesh;
		terrainHeights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		
		pathCells = new ArrayList();
	 
		if (newMesh == null) 
		{
			newMesh = new Mesh();
			newMesh.name = "Generated Path Mesh";
			meshFilter.sharedMesh = newMesh;
		} 
	  
		else 
			newMesh.Clear();

		
		if (nodeObjects == null || nodeObjects.Length < 2) 
		{
			return;
		}
		
		int n = nodeObjects.Length;

		int verticesPerNode = 2 * (smoothingLevel + 1) * 2;
		int trianglesPerNode = 6 * (smoothingLevel + 1);
		Vector2[] uvs = new Vector2[(verticesPerNode * (n - 1))];
		Vector3[] newVertices = new Vector3[(verticesPerNode * (n - 1))];
		int[] newTriangles = new int[(trianglesPerNode * (n - 1))];
		nodeObjectVerts = new Vector3[(verticesPerNode * (n - 1))];
		int nextVertex  = 0;
		int nextTriangle = 0;
		int nextUV = 0;
		
		// variables for splines and perpendicular extruded points
		float[] cubicX = new float[n];
		float[] cubicZ = new float[n];
		Vector3 handle1Tween = new Vector3();
		Vector3[] g1 = new Vector3[smoothingLevel+1];
		Vector3[] g2 = new Vector3[smoothingLevel+1];
		Vector3[] g3 = new Vector3[smoothingLevel+1];
		Vector3 oldG2 = new Vector3();
		Vector3 extrudedPointL = new Vector3();
		Vector3 extrudedPointR = new Vector3();
		
		for(int i = 0; i < n; i++)
		{
			cubicX[i] = nodeObjects[i].position.x;
			cubicZ[i] = nodeObjects[i].position.z;
		}
		
		for (int i = 0; i < n; i++) 
		{
			g1 = new Vector3[smoothingLevel+1];
			g2 = new Vector3[smoothingLevel+1];
			g3 = new Vector3[smoothingLevel+1];
			
			extrudedPointL = new Vector3();
			extrudedPointR = new Vector3();
			
			if (i == 0)
			{
				newVertices[nextVertex] = nodeObjects[0].position;
				nextVertex++;
				uvs[0] = new Vector2(0f, 1f);
				nextUV++;
				newVertices[nextVertex] = nodeObjects[0].position;
				nextVertex++;
				uvs[1] = new Vector2(1f, 1f);
				nextUV++;
				
				continue;
			}
			
			float _widthAtNode = pathWidth;		
			
			// Interpolate points along the path using splines for direction and bezier curves for heights
			for (int j = 0; j < smoothingLevel + 1; j++) 
			{
				// clone the vertex for uvs
				if(i == 1)
				{
					if(j != 0)
					{
						newVertices[nextVertex] = newVertices[nextVertex-2];
						nextVertex++;
						
						newVertices[nextVertex] = newVertices[nextVertex-2];
						nextVertex++;
						
						uvs[nextUV] = new Vector2(0f, 1f);
						nextUV++;
						uvs[nextUV] = new Vector2(1f, 1f);
						nextUV++;
					}
					
					else
						oldG2 = nodeObjects[0].position;
				}
				
				else
				{
					newVertices[nextVertex] = newVertices[nextVertex-2];
					nextVertex++;
					
					newVertices[nextVertex] =newVertices[nextVertex-2];
					nextVertex++;
					
					uvs[nextUV] = new Vector2(0f, 1f);
					nextUV++;
					uvs[nextUV] = new Vector2(1f, 1f);
					nextUV++;
				}
				
				float u = (float)j/(float)(smoothingLevel+1f);
				
				Cubic[] X = calcNaturalCubic(n-1, cubicX);
				Cubic[] Z = calcNaturalCubic(n-1, cubicZ);
				
				Vector3 tweenPoint = new Vector3(X[i-1].eval(u), 0f, Z[i-1].eval(u));
				
				// Add the current tweenpoint as a path cell
				TerrainPathCell tC = new TerrainPathCell();
				tC.position.x = ((tweenPoint.x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution;
				tC.position.y = ((tweenPoint.z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution;
				tC.heightAtCell = (tweenPoint.y - parentTerrain.transform.position.y)/terData.size.y;
				pathCells.Add(tC);
				
				// update tweened points
				g2[j] = tweenPoint;
				g1[j] = oldG2;
				g3[j] = g2[j] - g1[j];
				oldG2 = g2[j];
				
				// Create perpendicular points for vertices
				extrudedPointL = new Vector3(-g3[j].z, 0, g3[j].x);
				extrudedPointR = new Vector3(g3[j].z, 0, -g3[j].x);
				extrudedPointL.Normalize();
				extrudedPointR.Normalize();
				extrudedPointL *= _widthAtNode;
				extrudedPointR *= _widthAtNode;
				
				// Height at the terrain
				tweenPoint.y = terrainHeights[(int)(((float)(tweenPoint.z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(tweenPoint.x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;

				// create vertices at the perpendicular points
				newVertices[nextVertex] = tweenPoint + extrudedPointR;
				if(!road)
					newVertices[nextVertex].y = (((float)terrainHeights[(int)(((float)(newVertices[nextVertex].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[nextVertex].x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y) + newVertices[nextVertex-2].y)/2f;
				else
					newVertices[nextVertex].y = (float)terrainHeights[(int)(((float)(newVertices[nextVertex].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[nextVertex].x- parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;
				nodeObjectVerts[nextVertex] = newVertices[nextVertex];
				nextVertex++;
				
				newVertices[nextVertex] = tweenPoint + extrudedPointL;
				if(!road)
					newVertices[nextVertex].y = (((float)terrainHeights[(int)(((float)(newVertices[nextVertex].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[nextVertex].x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y) + newVertices[nextVertex-2].y)/2f;
				else
					newVertices[nextVertex].y = (float)terrainHeights[(int)(((float)(newVertices[nextVertex].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[nextVertex].x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;
				nodeObjectVerts[nextVertex] = newVertices[nextVertex];
				nextVertex++;

				uvs[nextUV] = new Vector2(0f, 0f);
				nextUV++;
				uvs[nextUV] = new Vector2(1f, 0f);
				nextUV++;
				
				// used later to update the handles
				if(i == 1)
					if(j == 0)
						handle1Tween = tweenPoint;

				// flatten mesh
				if(flatten && !road)
				{
					if(newVertices[nextVertex-1].y < (newVertices[nextVertex-2].y-0.0f))
					{
						extrudedPointL *= 1.5f;
						extrudedPointR *= 1.2f;
						newVertices[nextVertex-1] = tweenPoint + extrudedPointL;
						newVertices[nextVertex-2] = tweenPoint + extrudedPointR;
						
						newVertices[nextVertex-1].y = newVertices[nextVertex-2].y;
					}
				
					else if(newVertices[nextVertex-1].y > (newVertices[nextVertex-2].y-0.0f))
					{
						extrudedPointR *= 1.5f;
						extrudedPointL *= 1.2f;
						newVertices[nextVertex-2] = tweenPoint + extrudedPointR;
						newVertices[nextVertex-1] = tweenPoint + extrudedPointL;
						
						newVertices[nextVertex-2].y = newVertices[nextVertex-1].y;		
					}
				}

				// Create triangles...
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j); // 0
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 1; // 1
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 2; // 2
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 1; // 1
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 3; // 3
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 2; // 2
				nextTriangle++;
			}
		}
		
		// update handles
		g2[0] = handle1Tween;
		g1[0] = nodeObjects[0].position;
		g3[0] = g2[0] - g1[0];
	
		extrudedPointL = new Vector3(-g3[0].z, 0, g3[0].x);
		extrudedPointR = new Vector3(g3[0].z, 0, -g3[0].x);
		
		extrudedPointL.Normalize();
		extrudedPointR.Normalize();
		extrudedPointL *= nodeObjects[0].width;
		extrudedPointR *= nodeObjects[0].width;
		
		newVertices[0] = nodeObjects[0].position + extrudedPointR;
		newVertices[0].y = terrainHeights[(int)(((float)(newVertices[0].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[0].x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;
		newVertices[1] = nodeObjects[0].position + extrudedPointL;
		newVertices[1].y = terrainHeights[(int)(((float)(newVertices[1].z - parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(newVertices[1].x - parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;
		
		if(road)
		{
			for(int i = 0; i < newVertices.Length; i++)
			{
				newVertices[i].y = terComponent.SampleHeight(newVertices[i]) + (0.05f) + parentTerrain.transform.position.y;
			}
		}
		
		newMesh.vertices = newVertices;
		newMesh.triangles = newTriangles;
		
		newMesh.uv =  uvs;
		
		Vector3[] myNormals = new Vector3[newMesh.vertexCount];
		for(int p = 0; p < newMesh.vertexCount; p++)
		{
			myNormals[p] = Vector3.up;
		}

		newMesh.normals = myNormals;
		
		TangentSolver(newMesh);

//		newMesh.RecalculateNormals();
		pathCollider.sharedMesh = meshFilter.sharedMesh;
		pathCollider.smoothSphereCollisions = true;
		
		// we don't want to see the mesh
		if(!road)
			pathMesh.renderer.enabled = false;
		else
			pathMesh.renderer.enabled = true;
		
		transform.localScale = new Vector3(1,1,1);
	}
	
	public bool FinalizePath()
	{
		transform.localScale = new Vector3(1,1,1);
		
		if(terData.alphamapLayers > pathTexture || isRoad)
		{
			float[,] tempLRheightmap = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
			
			float[,,] alphamap = terData.GetAlphamaps(0, 0, terData.alphamapWidth, terData.alphamapHeight);
			float[,,] preWornCells = terData.GetAlphamaps(0, 0, terData.alphamapWidth, terData.alphamapHeight);
			
			innerPathVerts = new ArrayList();
			totalPathVerts = new ArrayList();
			ArrayList roadVerts = new ArrayList();
			ArrayList wearCells = new ArrayList();
			Vector3 returnCollision = new Vector3();
			
			pathMesh.transform.Translate(0f, -150f, 0f);
			
			pathWidth +=  6;
			
			if(pathFlat || isRoad)
				CreatePath(30, true, false);
			else
				CreatePath(30, false, false);
			
			foreach(TerrainPathCell tC in terrainCells)
			{
				RaycastHit raycastHit = new RaycastHit();
				Ray pathRay = new Ray(new Vector3((float)((tC.position.x * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.x, (float)tC.heightAtCell * terData.size.y + parentTerrain.transform.position.y, (float)((tC.position.y* terData.size.z)/terData.heightmapResolution) + parentTerrain.transform.position.z), -Vector3.up);
				
				if(pathCollider.Raycast(pathRay, out raycastHit, Mathf.Infinity))
				{
					innerPathVerts.Add(tC);
				}
			}
			
//			int detailRadius = (int)((float)terData.detailResolution/(terData.heightmapResolution-1));
			
			/* Removed because Unity 3.0 doesn't allow it :((
			
			// Remove trees and details from path
			foreach(TerrainPathCell tC in innerPathVerts)
			{
				for(int i = 0; i < terData.detailPrototypes.Length; i++)
				{
					int[,] detailLayer = terData.GetDetailLayer(0, 0, terData.detailResolution, terData.detailResolution, i);

					for(int j = -detailRadius+1; j < detailRadius; j++)
					{
						for(int k = -detailRadius+1; k < detailRadius; k++)
						{
							detailLayer[(int)(((float)tC.position.y/terData.heightmapResolution) * terData.detailResolution) + j, (int)(((float)tC.position.x/terData.heightmapResolution) * terData.detailResolution) + k] = 0;
						}
					}
					terData.SetDetailLayer(0, 0, i, detailLayer);
				}
				
				for(int i = 0; i < terData.treePrototypes.Length; i++)
				{
					terComponent.RemoveTrees(new Vector3(((float)tC.position.x/terData.heightmapResolution), ((float)tC.position.y/terData.heightmapResolution)), ((float)5f/terData.size.x), i);
				}
			}

			terData.RecalculateTreePositions();
			*/
			
			if(isRoad)
			{
				pathWidth -=  7;
				
				if(pathFlat || isRoad)
					CreatePath(30, true, false);
				else
					CreatePath(30, false, false);
				
				foreach(TerrainPathCell tC in terrainCells)
				{
					RaycastHit raycastHit = new RaycastHit();
					Ray pathRay = new Ray(new Vector3((float)((tC.position.x * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.x, (float)tC.heightAtCell * terData.size.y + parentTerrain.transform.position.y, (float)((tC.position.y * terData.size.z)/terData.heightmapResolution) + parentTerrain.transform.position.z), -Vector3.up);
					
					if(pathCollider.Raycast(pathRay, out raycastHit, Mathf.Infinity))
					{
						roadVerts.Add(tC);
					}
				}
				
				pathWidth += 7;
			}
			
			pathWidth +=  8;
			
			if(pathFlat || isRoad)
				CreatePath(30, true, false);
			else
				CreatePath(30, false, false);
		
			foreach(TerrainPathCell tC in terrainCells)
			{
				RaycastHit raycastHit = new RaycastHit();
				Ray pathRay = new Ray(new Vector3((float)((tC.position.x * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.x, (float)tC.heightAtCell * terData.size.y + parentTerrain.transform.position.y, (float)((tC.position.y * terData.size.z)/terData.heightmapResolution) + parentTerrain.transform.position.z), -Vector3.up);
				
				if(pathCollider.Raycast(pathRay, out raycastHit, Mathf.Infinity))
				{
					returnCollision = raycastHit.point;
					returnCollision.y = (returnCollision.y - parentTerrain.transform.position.y)/terData.size.y;
					
					if(pathFlat || isRoad)
					{
						tempLRheightmap[(int)(tC.position.y), (int)(tC.position.x)] = returnCollision.y + ((float)(150.00f/terData.size.y));
					}
					
					totalPathVerts.Add(tC);
				}
			}
			
			if(isRoad)
				pathWidth -= 12;
			else
				pathWidth -= 8;
			
			if(pathFlat || isRoad)
				CreatePath(100, true, false);
			else
				CreatePath(100, false, false);
			
			ArrayList totalPathVerts2 = new ArrayList();
			
			if(!isRoad)
			{
				// After getting the terrain cells for terrain deformation, now get the ones for texturing
				foreach(TerrainPathCell tC in terrainCells)
				{
					RaycastHit raycastHit = new RaycastHit();
					Ray pathRay = new Ray(new Vector3((float)((tC.position.x * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.x, (float)tC.heightAtCell * terData.size.y + parentTerrain.transform.position.y, (float)((tC.position.y * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.z), -Vector3.up);
					
					if(pathCollider.Raycast(pathRay, out raycastHit, Mathf.Infinity))
					{
						totalPathVerts2.Add(tC);
					}
				}
				
				pathMesh.transform.Translate(0f, 150f, 0f);
				
				foreach(TerrainPathCell tC in totalPathVerts2)
				{
					float closestDistance = 100f;
					float distanceFromPath = 0;

					foreach(TerrainPathCell pV in pathCells)
					{
						distanceFromPath = (float)Mathf.Sqrt((float)Mathf.Pow((pV.position.x - tC.position.x), 2f) + Mathf.Pow((pV.position.y - tC.position.y), 2f));
						
						if(distanceFromPath < closestDistance)
						{
							closestDistance = distanceFromPath;
						}
					}
					
					// draw actual path
					float blendAmount;
					float randomBlendAdd = UnityEngine.Random.Range(0.0f, 0.5f);
					
					blendAmount = (float)closestDistance/(pathWidth/2f);
					
					if(blendAmount>1.0f)
						blendAmount = 1.0f;
					
					if(blendAmount<0.0f)
						blendAmount = 0.0f;
					
					if(!pathUniform)
					{
						blendAmount += randomBlendAdd;
						
						if(blendAmount>1.0f)
						blendAmount = 1.0f;
						
						if(blendAmount<0.0f)
							blendAmount = 0.0f;
					}
					
					for(int i = 0; i < terData.alphamapLayers; i++)
					{
						if(alphamap[Convert.ToInt32((tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), Convert.ToInt32((tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), i] > 0.0f)
						{
							Vector3 wearCell = new Vector3(Convert.ToInt32((tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), Convert.ToInt32((tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), 0f);
							
							wearCells.Add(wearCell);

							alphamap[Convert.ToInt32((tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), Convert.ToInt32((tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), i]  *= blendAmount;
						}
					}

					alphamap[Convert.ToInt32((tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), Convert.ToInt32((tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), pathTexture] += (1f- blendAmount);
				}
				
				if(!pathUniform)
				{
					foreach(Vector3 wc in wearCells)
					{
						float newValue = 0.0f;
						float value = 0.0f;
						
						for(int i = 0; i < terData.alphamapLayers; i++)
						{
							if(i == pathTexture)
							{
								value = alphamap[(int)wc.x, (int)wc.y, i] ;
								alphamap[(int)wc.x, (int)wc.y, i]  -= (1.0f - pathWear);
								
								if(alphamap[(int)wc.x, (int)wc.y, i] < preWornCells[(int)wc.x, (int)wc.y, i])
								{
									alphamap[(int)wc.x, (int)wc.y, i] = preWornCells[(int)wc.x, (int)wc.y, i];
								}
								
								if(alphamap[(int)wc.x, (int)wc.y, i]  < 0f)
									alphamap[(int)wc.x, (int)wc.y, i]  = 0.0f;
								
								newValue = alphamap[(int)wc.x, (int)wc.y, i] ;
							}
						}
						
						for(int i = 0; i < terData.alphamapLayers; i++)
						{
							if(i != pathTexture)
							{
								alphamap[(int)wc.x, (int)wc.y, i]  = alphamap[(int)wc.x, (int)wc.y, i]  + ((value - newValue) * (alphamap[(int)wc.x, (int)wc.y, i]  / (1.0f - value)));
								
								if(alphamap[(int)wc.x, (int)wc.y, i]  > 1.0f)
									alphamap[(int)wc.x, (int)wc.y, i]  = 1.0f;
							}
							
							if(alphamap[(int)wc.x, (int)wc.y, i]  < 0f)
									alphamap[(int)wc.x, (int)wc.y, i]  = 0.0f;
							if(alphamap[(int)wc.x, (int)wc.y, i]  > 1.0f)
									alphamap[(int)wc.x, (int)wc.y, i]  = 1.0f;
						}
					}
				}
				
				terData.SetAlphamaps(0, 0, alphamap);
			}
			
			else
				pathMesh.transform.Translate(0f, 150f, 0f);
			
			terData.SetHeights(0, 0, tempLRheightmap);
			
			if(!isRoad)
			{
				if(pathFlat)
					CreatePath(pathSmooth, true, false);
				else
					CreatePath(pathSmooth, false, false);
			}
			
			else
			{
				// Perhaps a bit of an aggressive smooth routine ;)
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
			
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(totalPathVerts,  1.0f, true);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(totalPathVerts,  1.0f, true);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
					
				float[,] newTerrainHeights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
				foreach(TerrainPathCell pv in roadVerts)
				{
					if(pathWidth < 10)
						newTerrainHeights[(int)pv.position.y, (int)pv.position.x] -= 0.007f;
					
					else
						newTerrainHeights[(int)pv.position.y, (int)pv.position.x] -= 0.002f;
				}
				terData.SetHeights(0, 0, newTerrainHeights);
				
				
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				AreaSmooth(totalPathVerts,  1.0f, true);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
					
				AreaSmooth(totalPathVerts,  1.0f, true);
				
				foreach(TerrainPathCell tv in totalPathVerts)
					terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				
				if(pathWidth > 10)
				{
					AreaSmooth(innerPathVerts,  1.0f, false);
				
					foreach(TerrainPathCell tv in totalPathVerts)
						terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
						
						AreaSmooth(innerPathVerts,  1.0f, false);
					
					foreach(TerrainPathCell tv in totalPathVerts)
						terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
						
						AreaSmooth(innerPathVerts,  1.0f, false);
					
					foreach(TerrainPathCell tv in totalPathVerts)
						terrainCells[Convert.ToInt32((tv.position.y) + ((tv.position.x) * (terData.heightmapResolution)))].isAdded = false;
				}
				
				CreatePath(pathSmooth, true, true);
			}
			
			terData.SetAlphamaps(0, 0, alphamap);
			
			isFinalized = true;
			Debug.Log("Now that you have finalized your road, it is advised that you delete the 'Attached Path Script' component of this game object to avoid corrupting it in the future");
			return true;
		}
		
		else
		{
			Debug.LogError("Invalid texture prototype - please correct and re-finalize");
			return false;
		}
	}
	
	public void AreaSmooth(ArrayList terrainList, float blendAmount, bool exclusion)
	{
		TerrainPathCell tc;
		TerrainPathCell lh;
		
		float[,] blendLRheightmap = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		
		if(exclusion)
			foreach(TerrainPathCell tC in innerPathVerts)
			{		
				terrainCells[Convert.ToInt32((tC.position.y) + ((tC.position.x) * (terData.heightmapResolution)))].isAdded = true;	
			}
		
		for(int i = 0; i < terrainList.Count; i++)
		{
			tc = (TerrainPathCell)terrainList[i];
			ArrayList locals = new ArrayList();
			
			if(exclusion)
				for(int x = 2; x > -3; x--)
				{
					for(int y =  2; y > -3; y--)
					{				
						if(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded == false)
						{
							locals.Add(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))]);
							terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded = true;
						}
					}
				}
			
			else
				for(int x = 0; x > -1; x--)
				{
					for(int y =  0; y > -1; y--)
					{				
						if(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded == false)
						{
							locals.Add(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))]);
							terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded = true;
						}
					}
				}
				
			for(int p = 0; p < locals.Count; p++)
			{
				lh = (TerrainPathCell)locals[p];
				ArrayList localHeights = new ArrayList();
				float cumulativeHeights = 0f;
				
				// Get all immediate neighbors 
				for(int x = 1; x > -2; x--)
				{
					for(int y =  1; y > -2; y--)
					{
						localHeights.Add(terrainCells[Convert.ToInt32((lh.position.y + y) + ((lh.position.x+x) * (terData.heightmapResolution)))]);
					}
				}
				
				foreach(TerrainPathCell lH in localHeights)
				{
					cumulativeHeights += blendLRheightmap[(int)lH.position.y, (int)lH.position.x];
				}
				
				blendLRheightmap[(int)(lh.position.y), (int)(lh.position.x)] = (terrainHeights[(int)lh.position.y, (int)lh.position.x] * (1f-blendAmount)) + ( ((float)cumulativeHeights/((Mathf.Pow(((float)1f*2f + 1f), 2f)) - 0f)) * blendAmount);
			}
		}
		
		terData.SetHeights(0, 0, blendLRheightmap);
		
		if(!isRoad)
		{
			if(pathFlat)
				CreatePath(pathSmooth, true, false);
			else
				CreatePath(pathSmooth, false, false);
		}
		
		else if (isRoad && isFinalized)
			CreatePath(pathSmooth, true, true);
		
		else if (isRoad && !isFinalized)
			CreatePath(pathSmooth, true, false);
	}
	
	public void OnDrawGizmos()
	{
		if(showHandles)
		{
			if(nodeObjectVerts != null)
				if (nodeObjectVerts.Length > 0) 
				{
					int n = nodeObjectVerts.Length;
					for (int i = 0; i < n; i++) 
					{
						// Handles...
						Gizmos.color = Color.white;
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(-0.5f, 0, 0)), transform.TransformPoint(nodeObjectVerts[i]  + new Vector3(0.5f, 0, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i]  + new Vector3(0, -0.5f, 0)), transform.TransformPoint(nodeObjectVerts[i]  + new Vector3(0, 0.5f, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i]  + new Vector3(0, 0, -0.5f)), transform.TransformPoint(nodeObjectVerts[i]  + new Vector3(0, 0, 0.5f)));
					}
				}
		}
	}
	
	public void TangentSolver(Mesh theMesh)
    {
        int vertexCount = theMesh.vertexCount;
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;
        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length/3;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
        int tri = 0;
		
		int i1, i2, i3;
		Vector3 v1, v2, v3, w1, w2, w3, sdir, tdir;
		float x1, x2, y1, y2, z1, z2, s1, s2, t1, t2, r;
        for (int i = 0; i < (triangleCount); i++)
        {
            i1 = triangles[tri];
            i2 = triangles[tri+1];
            i3 = triangles[tri+2];

            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];

            w1 = texcoords[i1];
            w2 = texcoords[i2];
            w3 = texcoords[i3];

            x1 = v2.x - v1.x;
            x2 = v3.x - v1.x;
            y1 = v2.y - v1.y;
            y2 = v3.y - v1.y;
            z1 = v2.z - v1.z;
            z2 = v3.z - v1.z;

            s1 = w2.x - w1.x;
            s2 = w3.x - w1.x;
            t1 = w2.y - w1.y;
            t2 = w3.y - w1.y;

            r = 1.0f / (s1 * t2 - s2 * t1);
            sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;

            tri += 3;
        }
		
        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            // Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize(ref n, ref t);

            tangents[i].x  = t.x;
            tangents[i].y  = t.y;
            tangents[i].z  = t.z;

            // Calculate handedness
            tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ) ? -1.0f : 1.0f;
        }       
		
        theMesh.tangents = tangents;
    }
	
	public Cubic[] calcNaturalCubic(int n, float[] x) 
	{
		float[] gamma = new float[n+1];
		float[] delta = new float[n+1];
		float[] D = new float[n+1];
		int i;
	
		gamma[0] = 1.0f/2.0f;
		
		for ( i = 1; i < n; i++) 
		{
		  gamma[i] = 1/(4-gamma[i-1]);
		}
		
		gamma[n] = 1/(2-gamma[n-1]);
		
		delta[0] = 3*(x[1]-x[0])*gamma[0];
		
		for ( i = 1; i < n; i++) 
		{
		  delta[i] = (3*(x[i+1]-x[i-1])-delta[i-1])*gamma[i];
		}
		
		delta[n] = (3*(x[n]-x[n-1])-delta[n-1])*gamma[n];
		
		D[n] = delta[n];
		
		for ( i = n-1; i >= 0; i--) 
		{
		  D[i] = delta[i] - gamma[i]*D[i+1];
		}
		
		Cubic[] C = new Cubic[n+1];
		for ( i = 0; i < n; i++) {
		  C[i] = new Cubic((float)x[i], D[i], 3*(x[i+1] - x[i]) - 2*D[i] - D[i+1],
				   2*(x[i] - x[i+1]) + D[i] + D[i+1]);
		}
			
		return C;
	}
}

public class Cubic
{
  float a,b,c,d;        

  public Cubic(float a, float b, float c, float d){
    this.a = a;
    this.b = b;
    this.c = c;
    this.d = d;
  }
  
  public float eval(float u) 
  {
    return (((d*u) + c)*u + b)*u + a;
  }
}

	
