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

[CustomEditor(typeof(PathScript))]

public class PathEditor : Editor 
{
	public override void OnInspectorGUI() 
	{
		EditorGUIUtility.LookLikeControls();
		
		PathScript pathScript = (PathScript) target as PathScript;
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		Rect startButton = EditorGUILayout.BeginHorizontal();
		startButton.x = startButton.width / 2 - 100;
		startButton.width = 200;
		startButton.height = 18;
		
		if (GUI.Button(startButton, "New Path")) 
		{
			pathScript.NewPath();
			
			GUIUtility.ExitGUI();
		}
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		if (GUI.changed) 
		{
			EditorUtility.SetDirty(pathScript);
		}
	}
}