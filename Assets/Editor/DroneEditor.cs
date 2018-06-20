using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Drone)), CanEditMultipleObjects]
public class DroneEditor : Editor {

	public override void OnInspectorGUI()
	{
		Drone drone = (Drone)target;
		if(GUILayout.Button("Set Sensors")) {
			drone.SetUpSensors();
		}
		DrawDefaultInspector();
	}
}
