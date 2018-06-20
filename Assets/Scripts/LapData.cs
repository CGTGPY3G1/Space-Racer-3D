using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[System.Serializable] 
[XmlRoot]
public class LapData : ScriptableObject {
	[XmlAttribute]
	public string LapID;
	[XmlAttribute]
	public float averageSpeed, totalTime, startTime, endTime;
	[XmlAttribute]
	public int noOfCollisions;
	[XmlArray]
	[XmlArrayItem]
	public SegmentData[] segmentData;
	[XmlArray]
	[XmlArrayItem]
	public WaypointData[] waypointsData;
	
	/// <summary>
	/// Initialize the new lap with the specified LapID, noOfSegments, sd, noOfWaypoints, wd and startTime.
	/// </summary>
	/// <param name="LapID">Lap I.</param>
	/// <param name="noOfSegments">No of segments.</param>
	/// <param name="sd">The first segment of the laps data.</param>
	/// <param name="noOfWaypoints">No of waypoints.</param>
	/// <param name="wd">The first waypoint of the laps data.</param>
	/// <param name="startTime">Start time.</param>
	public void Initialize(string LapID, int noOfSegments, SegmentData sd, int noOfWaypoints, WaypointData wd, float startTime) {
		this.LapID = LapID; segmentData = new SegmentData[noOfSegments]; segmentData[0] = sd; waypointsData = new WaypointData[noOfWaypoints]; 
		waypointsData[0] = wd; this.startTime = startTime;
	}
	
	/// <summary>
	/// Finalize the lap data collection
	/// </summary>
	/// <param name="endTime">End time.</param>
	public void Finalize(float endTime) {
		this.endTime = endTime;
		CalculateResults();
	}
	
	/// <summary>
	/// Calculates the lap data.
	/// </summary>
	public void CalculateResults() {
		foreach(SegmentData sd in segmentData) {
			sd.Finalize(waypointsData);
		}
		float totalSpeed = 0;
		int totalCollisions = 0;
		foreach(SegmentData sd in segmentData) {
			totalSpeed += sd.averageSpeed;
			totalCollisions += sd.noOfCollisions;
		}
		noOfCollisions = totalCollisions;
		averageSpeed = totalSpeed/segmentData.Length;
		totalTime = endTime-startTime;
	}
}
