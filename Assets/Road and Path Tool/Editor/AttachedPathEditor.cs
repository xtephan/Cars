/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttachedPathScript))]

public class AttachedPathEditor : Editor 
{
	public void Awake()
	{
		
	}
	
	public void OnSceneGUI()
	{
		AttachedPathScript pathScript = (AttachedPathScript) target as AttachedPathScript;
		
		Event currentEvent = Event.current;
		
		if(pathScript.addNodeMode == true)
		{
			// If P is pressed, than create a node at selected point (window has to have focus)
			if(currentEvent.isKey && currentEvent.character == 'p')
			{
				Vector3 pathNode = GetTerrainCollisionInEditor(currentEvent);
				
				TerrainPathCell pathNodeCell = new TerrainPathCell();
				pathNodeCell.position.x = pathNode.x;
				pathNodeCell.position.y = pathNode.z;
				pathNodeCell.heightAtCell = pathNode.y;
				
				pathScript.CreatePathNode(pathNodeCell);
				pathScript.addNodeMode = false;
			}
		}
		
		if (pathScript.nodeObjects != null && pathScript.nodeObjects.Length != 0) 
		{
			int n = pathScript.nodeObjects.Length;
			for (int i = 0; i < n; i++) 
			{
				PathNodeObjects node = pathScript.nodeObjects[i];
				node.position = Handles.PositionHandle(node.position, Quaternion.identity);
			}
		}
		
		if (GUI.changed) 
		{
			EditorUtility.SetDirty(pathScript);
			
			if((pathScript.pathFlat || pathScript.isRoad) && (pathScript.isFinalized == false))
				pathScript.CreatePath(pathScript.pathSmooth, true, false);
			
			else if(!pathScript.pathFlat && !pathScript.isRoad) 
				pathScript.CreatePath(pathScript.pathSmooth, false, false);
			
			else if(pathScript.isFinalized)
				pathScript.CreatePath(pathScript.pathSmooth, true, true);
		}
	}
	
	public override void OnInspectorGUI() 
	{
		EditorGUIUtility.LookLikeControls();
		
		AttachedPathScript pathScript = (AttachedPathScript) target as AttachedPathScript;
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.PrefixLabel("Show handles");
		pathScript.showHandles = EditorGUILayout.Toggle(pathScript.showHandles);
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		
		if(!pathScript.isFinalized)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.PrefixLabel("Road");
			pathScript.isRoad = EditorGUILayout.Toggle(pathScript.isRoad);
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			
			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal();
			pathScript.pathWidth = (int) EditorGUILayout.IntSlider("Path Width", pathScript.pathWidth, 3, 20);
			EditorGUILayout.EndHorizontal();
			
			if(!pathScript.isRoad)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				pathScript.pathTexture = (int) EditorGUILayout.IntSlider("Texture Prototype", pathScript.pathTexture, 0, 30);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				pathScript.pathUniform = (bool) EditorGUILayout.Toggle("Uniform texture?", pathScript.pathUniform);
				EditorGUILayout.EndHorizontal();
				
				if(!pathScript.pathUniform)
				{
					EditorGUILayout.Separator();
					EditorGUILayout.BeginHorizontal();
					pathScript.pathWear = (float) EditorGUILayout.Slider("Wear", pathScript.pathWear, 0.5f, 1.0f);
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				pathScript.pathFlat = (bool) EditorGUILayout.Toggle("Flat?", pathScript.pathFlat);
				EditorGUILayout.EndHorizontal();
			}
		}
			
			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal();
			pathScript.pathSmooth = (int) EditorGUILayout.IntSlider("Mesh Smoothing", pathScript.pathSmooth, 5, 60);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

		if(!pathScript.isFinalized)
		{
			EditorGUILayout.Separator();
			Rect startButton = EditorGUILayout.BeginHorizontal();
			startButton.x = startButton.width / 2 - 100;
			startButton.width = 200;
			startButton.height = 18;
			
			if (GUI.Button(startButton, "Add path node")) 
			{
				pathScript.addNodeMode = true;
				
				GUIUtility.ExitGUI();
			}
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			Rect endButton = EditorGUILayout.BeginHorizontal();
			endButton.x = endButton.width / 2 - 100;
			endButton.width = 200;
			endButton.height = 18;
			
			if (GUI.Button(endButton, "Finalize Path")) 
			{
				if(pathScript.nodeObjects.Length > 1)
				{	
					// define terrain cells
					pathScript.terrainCells = new TerrainPathCell[pathScript.terData.heightmapResolution * pathScript.terData.heightmapResolution];

					for(int x = 0; x < pathScript.terData.heightmapResolution; x++)
					{
						for(int y = 0; y < pathScript.terData.heightmapResolution; y++)
						{
								pathScript.terrainCells[ (y) + (x*pathScript.terData.heightmapResolution)].position.y = y;
								pathScript.terrainCells[ (y) + (x*pathScript.terData.heightmapResolution)].position.x = x;
								pathScript.terrainCells[ (y) + (x*pathScript.terData.heightmapResolution)].heightAtCell = pathScript.terrainHeights[y, x]; 
								pathScript.terrainCells[ (y) + (x*pathScript.terData.heightmapResolution)].isAdded = false;
						}
					}
					
					// finalize path
					Undo.RegisterUndo(pathScript.terData, "Undo finalize path");
					bool success = pathScript.FinalizePath();
					
					if(success)
					{
						if(pathScript.isRoad)
						{
							pathScript.pathMesh.renderer.enabled = true;
						}
					
						else
						{
							MeshFilter meshFilter = (MeshFilter)pathScript.pathMesh.GetComponent(typeof(MeshFilter));
							Mesh destroyMesh = meshFilter.sharedMesh;
						
							DestroyImmediate(destroyMesh);
							DestroyImmediate(meshFilter);
						}
					}
				}
				
				else
					Debug.Log("Not enough nodes to finalize");
			}
			
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		if(pathScript.isFinalized)
		{
			Rect smoothPathButton = EditorGUILayout.BeginHorizontal();
			smoothPathButton.x = smoothPathButton.width / 2 - 100;
			smoothPathButton.width = 200;
			smoothPathButton.height = 18;
			
			if (GUI.Button(smoothPathButton, "Smooth Path")) 
			{
				Undo.RegisterUndo(pathScript.terData, "Undo smooth path");
				pathScript.AreaSmooth(pathScript.innerPathVerts,  1.0f, false);
				
				foreach(TerrainPathCell tc in pathScript.totalPathVerts)
					pathScript.terrainCells[Convert.ToInt32((tc.position.y) + ((tc.position.x) * (pathScript.terData.heightmapResolution)))].isAdded = false;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			
			Rect smoothPathSlopeButton = EditorGUILayout.BeginHorizontal();
			smoothPathSlopeButton.x = smoothPathSlopeButton.width / 2 - 100;
			smoothPathSlopeButton.width = 200;
			smoothPathSlopeButton.height = 18;
			
			if (GUI.Button(smoothPathSlopeButton, "Smooth Path Slope")) 
			{
				Undo.RegisterUndo(pathScript.terData, "Undo smooth path slope");
				pathScript.AreaSmooth(pathScript.totalPathVerts,  1.0f, true);
				
				foreach(TerrainPathCell tc in pathScript.totalPathVerts)
					pathScript.terrainCells[Convert.ToInt32((tc.position.y) + ((tc.position.x) * (pathScript.terData.heightmapResolution)))].isAdded = false;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
		}
		
		if (GUI.changed) 
		{
			if(pathScript.isFinalized && !pathScript.isRoad) { }
				// do nothing
			
			else
			{
				EditorUtility.SetDirty(pathScript);
				
				if((pathScript.pathFlat || pathScript.isRoad) && (!pathScript.isFinalized))
					pathScript.CreatePath(pathScript.pathSmooth, true, false);
				
				else if (!pathScript.pathFlat && !pathScript.isRoad)
					pathScript.CreatePath(pathScript.pathSmooth, false, false);
				
				else if (pathScript.isRoad && pathScript.isFinalized)
					pathScript.CreatePath(pathScript.pathSmooth, true, true);
			}
		}
	}
	

	public Vector3 GetTerrainCollisionInEditor(Event currentEvent)
	{
		Vector3 returnCollision = new Vector3();
		
		AttachedPathScript pathScript = (AttachedPathScript) target as AttachedPathScript;
		
		Camera SceneCameraReceptor = new Camera();
		
		GameObject terrain = pathScript.parentTerrain;
		Terrain terComponent = (Terrain) terrain.GetComponent(typeof(Terrain));
		TerrainCollider terCollider = (TerrainCollider) terrain.GetComponent(typeof(TerrainCollider));
		TerrainData terData = terComponent.terrainData;
		
		if(Camera.current != null)
		{
			SceneCameraReceptor = Camera.current;
		
			RaycastHit raycastHit = new RaycastHit();
			
			Vector2 newMousePosition = new Vector2(currentEvent.mousePosition.x, Screen.height - (currentEvent.mousePosition.y + 25));
			
			Ray terrainRay = SceneCameraReceptor.ScreenPointToRay(newMousePosition);
			
			if(terCollider.Raycast(terrainRay, out raycastHit, Mathf.Infinity))
			{
				returnCollision = raycastHit.point;
				
				returnCollision.x = Mathf.RoundToInt((returnCollision.x/terData.size.x) * terData.heightmapResolution);
				returnCollision.y = returnCollision.y/terData.size.y;
				returnCollision.z = Mathf.RoundToInt((returnCollision.z/terData.size.z) * terData.heightmapResolution);
			}
			
			else
				Debug.LogError("Error: No collision with terrain to create node");
			
		}
		
		return returnCollision;
	}
}
