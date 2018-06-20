using UnityEngine;
using System.Collections;

/// <summary>
/// Used for position tracking/waypoint/target point/race line generation
/// </summary>

[System.Serializable]
public class CheckPointScript : MonoBehaviour {
	public Transform checkPointTransform;
	
	// the number (array index) of the segment this checkpoint exists within
	public int segment;
	public int Segment {
		get { return segment; }
		set { segment = value; }
	}
	
	// the number (array index) of this checkpoint
	public int number;
	public int Number{ 
		get { return number; }
		set { number = value; }
	}

	// The location of each target point
	public Vector3[] points;
	// Set up each point with the outer points positioned maxDistance from the centre point
	// along the X axis and the inner points poitioned between the outer and centre points
	public void SetPoints(float maxDistance) {
		checkPointTransform = transform;
		Vector3 pointLocation = checkPointTransform.right*maxDistance;
		points = new Vector3[5];
		points[0] = checkPointTransform.position-pointLocation;
		points[1] = checkPointTransform.position-(pointLocation*0.5f);
		points[2] = checkPointTransform.position;
		points[3] = checkPointTransform.position+(pointLocation*0.5f);
		points[4] = checkPointTransform.position+pointLocation;
	}
	
	// Gets the Target Point for the PathBuilder (uses values between (-2 and 2)) 
	public Vector3 getPoint(int point) {
		return points[Mathf.Clamp(point+2, 0, points.Length-1)];
	}
	
	// Used by Drones to determine which part of the track they crossed
	public int GetClosestPoint(Vector3 dronePosition){
		int shortest = -1;
		float bestLength = float.PositiveInfinity;
		for(int i = 0; i < points.Length; i++) {
			float newLength = Vector3.SqrMagnitude(points[i]-dronePosition);
			if(newLength < bestLength) {
				bestLength = newLength;
				shortest = i;
			}
		}
		return shortest-2;
	}
}
