using UnityEditor;
using UnityEngine;


/// <summary>
/// Used to assign references through the Unity Inspector
/// see Dave for full description
/// </summary>
[CustomEditor(typeof(Dave))]
public class DaveEditor : Editor {

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("raceStartsScript"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("positionText"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLapText"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lapTimer"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startText"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("drones"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("dronePrefabs"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPoints"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraScript"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("speedometer"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("music"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("resultScript"), true);
		serializedObject.ApplyModifiedProperties();
	}
}
