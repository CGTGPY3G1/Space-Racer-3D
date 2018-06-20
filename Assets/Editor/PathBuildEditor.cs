using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PathBuilder))]
public class PathBuildEditor : Editor {
	private static GUILayoutOption smallButtonWidth = GUILayout.Width(30), smallButtonHeight = GUILayout.Height(20);
	public override void OnInspectorGUI()
	{		
		DrawDefaultInspector();
		if(!target)
			return;
		PathBuilder pb = (PathBuilder)target;
		if(!pb)
			return;
		bool showExtra = (pb.waypoints != null && pb.waypoints.Count>0);
		bool showSelectedSegment = (pb.segments != null && pb.segments.Count>0);

		EditorGUILayout.Separator();
		EditorGUILayout.PrefixLabel("Path Builder");
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
		if(showExtra) {
			if(GUILayout.Button("Add Colliders")) {
				pb.BuildColliders();
			}
			if(GUILayout.Button("Destroy Path")) {
				pb.DestroyPath();
			}
			SceneView.RepaintAll();
			
		}
		if(GUILayout.Button("Build Path")) {
			pb.BuildPoints();
			SceneView.RepaintAll();
		}
		if(showExtra) {

			if(showSelectedSegment) {
				EditorGUILayout.Separator();
				EditorGUILayout.PrefixLabel("Selected Segment");
				GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
				EditorGUILayout.LabelField("Number");
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("-", EditorStyles.miniButtonLeft, smallButtonWidth, smallButtonHeight)) {
					pb.SelectedSegment--;
				}
				pb.SelectedSegment = EditorGUILayout.IntSlider(pb.SelectedSegment, 1, pb.getNumOfSegments());
				if(GUILayout.Button("+", EditorStyles.miniButtonRight, smallButtonWidth, smallButtonHeight)) {
					pb.SelectedSegment++;
				}
				EditorGUILayout.EndHorizontal();
				
				
				EditorGUILayout.LabelField("Position");
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("-", EditorStyles.miniButtonLeft, smallButtonWidth, smallButtonHeight)) {
					pb.Segments[pb.SelectedSegment-1].start--;
				}
				pb.Segments[pb.SelectedSegment-1].start = (EditorGUILayout.IntSlider(pb.Segments[pb.SelectedSegment-1].start, 0, pb.waypoints.Count-1));
				if(GUILayout.Button("+", EditorStyles.miniButtonRight, smallButtonWidth, smallButtonHeight)) {
					pb.Segments[pb.SelectedSegment-1].start++;
				}
				EditorGUILayout.EndHorizontal();
				
				
				EditorGUILayout.LabelField("Target");
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("-", EditorStyles.miniButtonLeft, smallButtonWidth, smallButtonHeight)) {
					pb.Segments[pb.SelectedSegment-1].targetPoint--;
				}
				pb.Segments[pb.SelectedSegment-1].targetPoint = (EditorGUILayout.IntSlider(pb.Segments[pb.SelectedSegment-1].targetPoint, -2, 2));
				if(GUILayout.Button("+", EditorStyles.miniButtonRight, smallButtonWidth, smallButtonHeight)) {
					pb.Segments[pb.SelectedSegment-1].targetPoint++;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
			}
			EditorGUILayout.PrefixLabel("Race Line Creation");
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
			if(GUILayout.Button("Add Segment")) {
				if(pb.CanAddSegment()) {
					pb.addSeg();
				}
				
			}
//			if(pb.RemovedSegments > 0) {
//				if(GUILayout.Button("UndoRemove")) {
//					if(pb.CanAddSegment()) {
//						pb.undoDestroySeg();
//					}
//				}
//			}
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
			if(showSelectedSegment) {
				if(GUILayout.Button("Destroy Selected Segment")) {
					pb.destroySeg();
				}
				if(GUILayout.Button("Destroy All Segments")) {
					pb.destroyAllSegs();
				}
				GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
				if(GUILayout.Button("Place Extra Segment")) {
					pb.placeExtraSeg();
				}
				GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
				if(GUILayout.Button("Complete Track")) {
					while(pb.CanAddSegment()) {
						pb.addSeg();
					}
				}
				GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
				if(GUILayout.Button("Initialize Segments")) {
					pb.InitializeSegments();
				}
				if(GUILayout.Button("Decorate Segments")) {
					pb.DecorateSegments();
				} 
			}
			
		}
		if(GUI.changed)
			EditorUtility.SetDirty(pb); 
	}
	
	public void OnSceneGUI() {
		if(!target)
			return;
		Handles.color = Color.cyan;
		PathBuilder pb = (PathBuilder)target;
		if(pb && pb.getNumOfSegments() > 0) {
			for(int i = 0; i < pb.getNumOfSegments(); i++) {
				Handles.Label(pb.getSegLabelPosition(i, 120), pb.getSegID(i));
			}
		}
	}
	
}
