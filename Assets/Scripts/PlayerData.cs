using UnityEngine;
using System.Collections;

/// <summary>
/// Used to track the race configuration amd score
/// </summary>
public class PlayerData {
	const float ELIMINATION_INCREMENT = 20, INITIAL_ELIMINATION_TIME = 30;
	public float[] lapTimes = new float[] { 64.25f, 65.50f  };
	// array accessor gets the best lap time
	public float GetBestLapTime() {
		return lapTimes[selectedLevel-1];
	}
	// array modifier sets the best lap time
	public void SetBestLapTime(float time) {
		lapTimes[selectedLevel-1] = time;
	}
	
	// race configuration amd score variables
	public string selectedVehicle;
	public int selectedLevel;
	public int selectedRaceType;
	public int finalPosition, noOfLaps;
	public float bestTime, startTime, eliminationTime;
	public PlayerData() {
		selectedVehicle = "";
		selectedLevel = 1;
		selectedRaceType = 0;
	}
	
	// sets the relevant end conditions for races
	public void SetRaceConditions(RaceType type) {
		selectedRaceType = (int)type;
		if(type == RaceType.Basic) {
			noOfLaps = 3;
		}
		else if(type == RaceType.TimeTrial) {
			noOfLaps = 1;
		}
		else if(type == RaceType.Elimination) {
			eliminationTime = INITIAL_ELIMINATION_TIME;
		}
	}
	
	// Increment the elimination timer
	public void IncrementEliminationTime() {
		eliminationTime += ELIMINATION_INCREMENT;
	}
}
