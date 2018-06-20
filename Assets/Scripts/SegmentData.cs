using UnityEngine;
using System;
using System.Collections;
using System.Xml.Serialization;

/// <summary>
/// Used to track drone performance while travelling through a segment
/// </summary>
[System.Serializable]
[XmlRoot]
public class SegmentData : ScriptableObject{
	[XmlAttribute]
	public int startPoint, endPoint, noOfCollisions, noOfWaypoints;
	[XmlAttribute]
	public float totalTime, startTime, endTime, startSpeed, averageSpeed;
	[XmlAttribute]
	public SensorData sensorData;
	
	/// <summary>
	/// Initialize a new Segment with the specified startPoint, endPoint, startTime, startSpeed and sensorData.
	/// </summary>
	/// <param name="startPoint">Start point.</param>
	/// <param name="endPoint">End point.</param>
	/// <param name="startTime">Start time.</param>
	/// <param name="startSpeed">Start speed.</param>
	/// <param name="sensorData">Sensor data.</param>
	public void Initialize(int startPoint, int endPoint, float startTime, float startSpeed, SensorData sensorData) {
		this.startPoint = startPoint; this.startTime = startTime; this.endPoint = endPoint;
		noOfWaypoints = endPoint-startPoint; this.startSpeed = startSpeed; this.sensorData = sensorData;
	}
	
	/// <summary>
	/// Finalize the segment data collection.
	/// </summary>
	/// <param name="wd">the waypoints contained within the segment.</param>
	public void Finalize(WaypointData[] wd) {
		float totalSpeed = 0;
		for(int i = startPoint; i < endPoint; i++) {
			totalSpeed += wd[i].speed;
		}
		this.averageSpeed = totalSpeed/noOfWaypoints;
	}
	
	/// <summary>
	/// Sets the end values.
	/// </summary>
	/// <param name="noOfCollisions">No of collisions.</param>
	/// <param name="endTime">End time.</param>
	public void SetEndValues(int noOfCollisions, float endTime) {
		this.noOfCollisions = noOfCollisions; this.endTime = endTime;
		totalTime = endTime - startTime;
	}
}
