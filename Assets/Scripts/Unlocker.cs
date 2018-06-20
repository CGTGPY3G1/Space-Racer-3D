using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Used to lock/unlock vehicles and tracks
/// </summary>
public class Unlocker : MonoBehaviour {
	// used to store locking data
	[System.Serializable]
	public class Unlockables {
		public bool[] tracks, vehicles;
	
		public Unlockables() {
			tracks = new bool[] { true, false };
			vehicles = new bool[] { true, false, false, false, false, false };
		}
	}
	
	// the names of each vehicle in alphabetical order
	private static string[] vehicleNames = { "Blue Falcon", "Feisar", "Hover9K", "HoverCar", "SX1", "X Fighter" };
	// the loction and name of the file to be used
	private static string file;
	// the unlocking data
	private static Unlockables unlockables;
	
	// unlocks a random unlockable item
	public static string UnlockNew() {
		file = Application.dataPath+"/ul";
		string unlocked = "Nothing";
		List<int> possibleT = new List<int>(), possibleV = new List<int>();
		LoadUnlocked();
		bool availableTrack = canUnlockTrack;
		if(availableTrack) {
			for(int i = 0; i < unlockables.tracks.Length; i++) {
				if(unlockables.tracks[i] == false)
					possibleT.Add(i);
			}
		}
		bool availableVehicle = canUnlockVehicle;
		if(availableVehicle){
			for(int i = 0; i < unlockables.vehicles.Length; i++) {
				if(unlockables.vehicles[i] == false)
					possibleV.Add(i);
			}
		}
		if(availableTrack) {
			int index;
			if(Random.value < 0.5f) {
				index = possibleT[Random.Range(0, possibleT.Count)];
				unlockables.tracks[index] = true;
				unlocked = "Track " + (index+1);
			}
			else {
				index = possibleV[Random.Range(0, possibleV.Count)];
				unlockables.vehicles[index] = true;
				unlocked = vehicleNames[index];
			}
		}
		else if(availableVehicle) {
			int index = possibleV[Random.Range(0, possibleV.Count)];
			unlockables.vehicles[index] = true;
			unlocked = vehicleNames[index];
		}
		SaveUnlocked();
		Debug.Log(unlocked + " Unlocked");
		return unlocked;
	}
	
	// returns a tracks locking lock status
	public static bool IsTrackUnLocked(int trackNumber) {
		file = Application.dataPath+"/ul";
		LoadUnlocked();
		return unlockables.tracks[trackNumber];
	}
	
	// returns a vehicles lock status
	public static bool IsVehicleUnLocked(int vehicleNumber) {
		file = Application.dataPath+"/ul";
		LoadUnlocked();
		return unlockables.vehicles[vehicleNumber];
	}
	
	// returns true if there are items left to be unlocked
	public static bool canUnlock() {
		file = Application.dataPath+"/ul";
		LoadUnlocked();
		if(canUnlockTrack)
			return true;
		if(canUnlockVehicle)
			return true;
		return false;
	}
	
	// returns true if there is a track left to be unlocked
	private static bool canUnlockTrack{
		get{ 
			for(int i = 0; i < unlockables.tracks.Length; i++) {
				if(unlockables.tracks[i] == false)
					return true;
			}
			return false;
		}
	}
	
	// returns true if there are vehicles left to be unlocked
	private static bool canUnlockVehicle{
		get{ 
			for(int i = 0; i < unlockables.vehicles.Length; i++) {
				if(unlockables.vehicles[i] == false)
					return true;
			}
			return false;
		}
	}
	
	// Use this for initialization
	void Start () {
		file = Application.dataPath+"/ul";
		LoadUnlocked();
	}
	
	// loads the locking data
	private static void LoadUnlocked() {
		if(File.Exists(file)) {
			Stream s = File.Open(file, FileMode.Open);
			try {
				unlockables = (Unlockables)new BinaryFormatter().Deserialize(s);
				Debug.Log("Loaded Unlockable Data");
			}
			catch (SerializationException e) {
				Debug.Log("Loading Unlockable Data Failed: " + e.Message);
				throw;
			}
			finally {
				s.Close();
			}
		}
		else {
			unlockables = new Unlockables();
			SaveUnlocked();
		}
	}
	
	// saves the locking data
	private static void SaveUnlocked() {
		Stream s = File.Open(file, FileMode.Create);
		BinaryFormatter bf = new BinaryFormatter();
		try {
			bf.Serialize(s, unlockables);
		}
		catch (SerializationException e) {
			Debug.Log("Saving Unlockable Data Failed: " + e.Message);
			throw;
		}
		finally {
			s.Close();
		}
		Debug.Log("Saved Unlockable Data");
	}
}
