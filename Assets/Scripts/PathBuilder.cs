using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PathBuilder : MonoBehaviour {
	
	[System.Serializable]
	public class SegmentDef{
		public string segmentID;
		public int start;
		public int end;
		public int length;
		public int targetPoint = 0;
	}
	public bool reversed;
	public List<Transform> trackSegments;
	public List<Transform> waypoints;
	public GameObject checkpointPrefab, track, startLine, segmentLine;
	public float height, width, length, pointWidth, startLineHeight, segmentLineHeight;
	public int startSegment = 0;

#region Segment Manipulation
	public List<SegmentDef> removedSegments = new List<SegmentDef>();
	public int RemovedSegments {
		get{ return removedSegments.Count; }
	}
	
	public List<SegmentDef> segments;
	public List<SegmentDef> Segments {
		get { return segments; }
		set { segments = value; }
	}
	public int defaultSegmentLength;
	private int selectedSegment;
	public int SelectedSegment {
		get{ return selectedSegment; }
		set{ selectedSegment = value; }
	}
	/// <summary>
	/// Updates the selected segments start.
	/// </summary>
	/// <param name="start">the new start position.</param>
	public void UpdateSegStart(int start) {
		segments[selectedSegment-1].start = start;
	}
	/// <summary>
	/// Updates the selected segments target.
	/// </summary>
	/// <param name="targetP">the new target point.</param>
	public void UpdateSegTarget(int targetP) {
		segments[selectedSegment-1].targetPoint = targetP;
	}
	/// <summary>
	/// Adds a segment
	/// </summary>
	public void addSeg() {
		int oldSize = segments.Count;
		SegmentDef newSeg = new SegmentDef();
		if(oldSize > 0) {
			int newSegStart =segments[oldSize-1].start+defaultSegmentLength;
			newSeg.targetPoint = segments[oldSize-1].targetPoint;
			newSeg.start = newSegStart;
		}
		else {
			newSeg.start = 0;
			newSeg.targetPoint = 0;
		}
		newSeg.segmentID = "Segment "+ (oldSize+1);
		segments.Add(newSeg);
		selectedSegment = oldSize+1;
		InitializeSegments();
	}
	/// <summary>
	/// Destroies all segments.
	/// </summary>
	public void destroyAllSegs() {
		segments = new List<SegmentDef>();
	}
	
	/// <summary>
	/// Destroies the selected segment.
	/// </summary>
	public void destroySeg() {
		selectedSegment--;
		removedSegments.Add(segments[selectedSegment]);
		segments.RemoveAt(selectedSegment);
		segments.TrimExcess();
		InitializeSegments();
	}
	/// <summary>
	/// Places an extra segment.
	/// </summary>
	public void placeExtraSeg() {
		int newSegStart =segments[selectedSegment-1].start+1;
		int nextSegmentStart = segments[getNextSegment(selectedSegment-1)].start;
		if(nextSegmentStart == 0) {
			nextSegmentStart = waypoints.Count-1;
		}
		if(newSegStart >= nextSegmentStart) {
			Debug.Log("Not enough room.  Move next segment!");
			return;	
		}
		SegmentDef newSeg = new SegmentDef();
		newSeg.start = newSegStart;
		newSeg.targetPoint = 0;
		segments.Insert(selectedSegment, newSeg);
		selectedSegment++;
		InitializeSegments();
	}
	
	/// <summary>
	/// Determines whether or not a new segment can be added.
	/// </summary>
	/// <returns><c>true</c> if a segment can be added; otherwise, <c>false</c>.</returns>
	public bool CanAddSegment() {
		if(segments.Count == 0 || segments[segments.Count-1].start+defaultSegmentLength < waypoints.Count)
			return true;
		 
		return false;
	}
	/// <summary>
	/// Gets the ID of the given segment
	/// </summary>
	/// <returns>The segment ID.</returns>
	/// <param name="num">the segment number.</param>
	public string getSegID(int num) {
		return segments[num].segmentID;
	}
	/// <summary>
	/// Gets the segment labels position.
	/// </summary>
	/// <returns>The segment labels position.</returns>
	/// <param name="num">the segment number.</param>
	/// <param name="posHeight">the height of the label.</param>
	public Vector3 getSegLabelPosition(int num, float posHeight) {
		int index = segments[num].start;
		return waypoints[index].position+(waypoints[index].up*posHeight);
	}
	
	/// <summary>
	/// Gets the number of segments.
	/// </summary>
	/// <returns>The number of segments.</returns>
	public int getNumOfSegments() {
		if(segments == null)
			return 0;
		return segments.Count;	
	}
	/// <summary>
	/// Gets the next segment.
	/// </summary>
	/// <returns>The next segment.</returns>
	/// <param name="current">the current segment.</param>
	public int getNextSegment(int current) { 
		int toReturn = current+1;
		if (toReturn >= segments.Count)
			toReturn -= segments.Count;
		return toReturn;
	}
	/// <summary>
	/// Gets the previous segment.
	/// </summary>
	/// <returns>The previous segment.</returns>
	/// <param name="current">the current segment.</param>
	public int getPreviousSegment(int current) { 
		int toReturn = current-1;
		if (toReturn <= -1)
			toReturn = segments.Count-1;
		return toReturn;
	}
#endregion
	
	// get the offset track index
	public int getTrackRef(int selected) { 
		return WrapValue(selected+(startSegment), 0, trackSegments.Count-1);
	}
	
	// Run the script without having to press play
	[ExecuteInEditMode]
	void OnDrawGizmos() {
		// draws the path
		if(waypoints != null) {
			for(int i = 0; i < waypoints.Count; i++) {
				if(!waypoints[i])
					return;
				CheckPointScript cp = waypoints[i].GetComponent<CheckPointScript>();
				
				float lineLength = 5, sphereRadius = 0.5f;
				Vector3 lineForward = waypoints[i].forward*lineLength, lineRight = (waypoints[i].right*lineLength), lineUp = waypoints[i].up*lineLength;
				
				for(int j = 0; j < cp.points.Length; j++) {
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(cp.points[j], cp.points[j]+lineForward);
					Gizmos.color = Color.green;
					Gizmos.DrawLine(cp.points[j], cp.points[j]+lineUp);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(cp.points[j], cp.points[j]+lineRight);
					Gizmos.color = Color.white;
					Gizmos.DrawWireSphere(cp.points[j], sphereRadius);
				}
			}
		}
		if(segments != null) {
			if(segments.Count > 0) {
				Gizmos.color = Color.magenta;
				for(int i = 0; i < segments.Count; i++) { 
					if(i == selectedSegment-1)
						Gizmos.color = Color.white;
					else 
						Gizmos.color = Color.magenta;
					int index = segments[i].start;
					Vector3 spherePos = waypoints[index].position+(waypoints[index].up*80);
					Gizmos.DrawSphere(spherePos, 20);
					Gizmos.DrawLine(waypoints[index].position, spherePos);
				}
				Gizmos.color = Color.yellow;
				int currentPoint = segments[segments.Count-1].targetPoint;
				for(int i = 0; i < segments.Count; i++) {
					CheckPointScript cps = waypoints[segments[i].start].GetComponent<CheckPointScript>();
					int newPoint = segments[i].targetPoint;
					Gizmos.DrawLine(cps.getPoint(currentPoint), waypoints[segments[getNextSegment(i)].start].GetComponent<CheckPointScript>().getPoint(newPoint));
					currentPoint = newPoint;
				}
			}
		}
	}
	
	void Start () {
		BuildPoints();
		BuildColliders();
		InitializeSegments();
		DecorateSegments();
	}
	
	// Creates and initialises a checkpoint for each track segment  
	public void BuildPoints() {
		// Destroy any old paths
		DestroyPath();
		trackSegments = new List< Transform>();
		waypoints = new List< Transform>();
		// get all children of the Tracks Transform
		foreach(Transform child in track.transform) {
			trackSegments.Add(child);
		}
		// switch the tracks direction (if required)
		if(reversed)
			trackSegments.Reverse();
			
		for(int i = 0; i < trackSegments.Count; i++) {
			// get the offset array index for the current segment
			int nextI = getTrackRef(i);
			// Set the name, tag and collision layer of the CheckPoint
			GameObject newPoint = Instantiate(checkpointPrefab) as GameObject;
			newPoint.name = "Waypoint "+(i+1);
			newPoint.tag = "Waypoint";
			newPoint.layer = 11;
			newPoint.GetComponent<CheckPointScript>().Number = i;
			waypoints.Add(newPoint.transform); 
			// Position the checkpoint in the offset position and set it as a child of this PathBuilder
			waypoints[i].position = trackSegments[nextI].position+(trackSegments[nextI].up*height);
			waypoints[i].parent = transform;
		}
		
		for(int i = 0; i < waypoints.Count; i++) {
			// Face the checkpoints towards the next checkpoint in the array
			waypoints[i].forward = (waypoints[WrapValue(i+1, 0, waypoints.Count-1)].position-waypoints[i].position);
			// Initialise the checkpoints target points
			waypoints[i].GetComponent<CheckPointScript>().SetPoints(pointWidth);
		} 
	}
	
	// Creates colliders for use by the 
	// Drone position tracking system
	public void BuildColliders() {
		Vector3 b = new Vector3(width, height*2, length);
		for(int i = 0; i < waypoints.Count; i++){
			BoxCollider bc = waypoints[i].gameObject.AddComponent<BoxCollider>();
			bc.size = b;
			bc.isTrigger = true;
		} 
	}
	
	// Destory the old path (if it exists)
	public void DestroyPath() {
		if(waypoints.Count > 0) {
			foreach(Transform obj in waypoints) {
				if(obj)
					DestroyImmediate(obj.gameObject);
			}
		}
	}
	
	/// <summary>
	/// Initializes the segments.
	/// </summary>
	public void InitializeSegments() {
		for(int i = 0; i < segments.Count; i++) {
			segments[i].segmentID = "Segment "+(i+1);
			int start = segments[i].start;
			int nextStart = segments[getNextSegment(i)].start;
			if(nextStart == 0) {
				nextStart = waypoints.Count-1;
			}
			segments[i].end = nextStart;
			segments[i].length = nextStart-start;
			for(int j = start; j < nextStart; j++) {
				waypoints[j].GetComponent<CheckPointScript>().Segment = i;
			}
		}
	}
	
	/// <summary>
	/// Decorates the segments.
	/// </summary>
	public void DecorateSegments() {
		int trackRef = getTrackRef(0);
		// Place Start Line
		Transform parentTransform = waypoints[segments[0].start];
		GameObject toAdd;
		toAdd = Instantiate(startLine) as GameObject;
		toAdd.transform.position = trackSegments[trackRef].position+(parentTransform.up*startLineHeight)-(parentTransform.forward*(length/2));
		toAdd.name = "Start Line";
		toAdd.transform.forward = parentTransform.forward;
		toAdd.transform.parent = parentTransform;
	}
	
//	Also Places Checkpoints (not used)
//	public void DecorateSegments() {
//		for(int i = 0; i < segments.Count; i++) {
//			int trackRef = getTrackRef(segments[i].start);
//			if(i == 0) {
//				// Place Start Line
//				GameObject toAdd;
//				toAdd = Instantiate(startLine) as GameObject;
//				toAdd.transform.position = trackSegments[trackRef].position+(waypoints[segments[i].start].up*startLineHeight)-(waypoints[segments[i].start].forward*(length/2));
//				toAdd.name = "Start Line";
//				toAdd.transform.forward = waypoints[segments[i].start].forward;
//				toAdd.transform.parent = waypoints[segments[i].start];
//			}
//			else {
//				// Place Checkpoint
//				GameObject toAdd;
//				toAdd = Instantiate(segmentLine) as GameObject;
//				toAdd.transform.position = trackSegments[trackRef].position+(waypoints[segments[i].start].up*segmentLineHeight);
//				toAdd.name = "Checkpoint "+ i;
//				toAdd.transform.forward = waypoints[segments[i].start].forward;
//				toAdd.transform.parent = waypoints[segments[i].start];
//			}
//		}
//	}

	/// <summary>
	/// Wraps the given value within a range.
	/// </summary>
	/// <returns>The index.</returns>
	/// <param name="value">the value to wrap.</param>
	/// <param name="min">Minimum range.</param>
	/// <param name="max">Maximum range.</param>
	public static int WrapValue(int value, int min, int max) {
		if (value >= min && value <= max) 
			return value;
		
		int difference = Mathf.Abs(max-min)+1;
		while(value<min){
			value += difference;
		}
		while(value>max){
			value -= difference;
		}
		return value;
	}
}
