using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;

/// <summary>
/// Self Sortable Position Info Class
/// Ordered by Lap Number, then by Segment Number, 
/// then by Checkpoint Number and finally by Checkpoint Time
/// </summary>

[System.Serializable]
[XmlRoot]
public class PositionInfo : IComparable<PositionInfo> {
	// the name of the drone being tracked
	[XmlAttribute]
	public string droneName;
	// lap segment and position numbers. droneNumber represents drones array index
	[XmlAttribute]
	public int lapNumber, currentSegment, currentCheckpoint, droneNumber;
	// the time the drone passed the last checkpoint
	[XmlAttribute]
	public double checkpointTime; 
	
	// compares the data basedon criteria described in summary
	public int CompareTo(PositionInfo other)
	{
		if (this.lapNumber != other.lapNumber) {
			return other.lapNumber.CompareTo(this.lapNumber);
		}
		else if (this.currentSegment != other.currentSegment){
			return other.currentSegment.CompareTo(this.currentSegment);
		}
		else if (this.currentCheckpoint != other.currentCheckpoint) {
			return other.currentCheckpoint.CompareTo(this.currentCheckpoint);
		}
		else {
			return this.checkpointTime.CompareTo(other.checkpointTime);
		}
	}
}
