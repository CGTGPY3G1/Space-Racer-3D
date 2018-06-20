using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

/// <summary>
/// Used to compare and display scores
/// </summary>
public class HighScores : MonoBehaviour {
	// used to track the scores for the 3 race types
	[System.Serializable]
	public class ScoreTable {
		// best race results
		public int basicRacePosition, eliminationPosition;
		public float timeTrialResult;
		// used to prevent beating the same track with
		// the same vehicle from unlcking a new asset
		private bool beatenBestTime;
		public bool BestTimeBeat {
			get { return beatenBestTime; }
			set { beatenBestTime = value; }
		}
		
		// Score table constructor
		public ScoreTable() {
			basicRacePosition = 1000; eliminationPosition = 1000;
			timeTrialResult = 10000000;
			beatenBestTime = false;
		}
	}
	
	// used to store and manipulate Score Tables
	[System.Serializable]
	public class ScoreBoard {
		public string[] vehicleNames = { "Blue Falcon", "Feisar", "Hover9K", "HoverCar", "SX1", "X Fighter" };
		public Dictionary<int, Dictionary<string, ScoreTable>> scores;
		
		// Initialize a complete scoreboard with default values
		public ScoreBoard() {
			scores = new Dictionary<int, Dictionary<string, ScoreTable>>();
			SetDefaultScores();
		}
		
		// get the score table in the position defined by level and vehicle
		public ScoreTable getScoreTable(int level, string vehicle) {
			return scores[level][vehicle];
		}
		
		// flag the time trial event as complete
		public void SetTimeBeaten(int level, string vehicle) {
			scores[level][vehicle].BestTimeBeat = true;
		}
		
		// store a score table in the position defined by level and vehicle
		public void setScoreTable(int level, string vehicle, ScoreTable table) {
			scores[level][vehicle] = table;
		}
		
		// Reset all scores
		public void SetDefaultScores() {
			for(int i = 1; i <= 2; i++) {
				scores[i] = new Dictionary<string, ScoreTable>();
				for(int j = 0; j < vehicleNames.Length; j++) {
					scores[i][vehicleNames[j]] = new ScoreTable();
				}
			}
		}
	}
	
	// Used to check the best time before beginning a time trial  
	public static float GetBestTime(string vehicle, int track) {
		LoadScores();
		return scoreBoard.getScoreTable(track, vehicle).timeTrialResult;
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	private static void LoadScores() {
		string file = Application.dataPath+"/hs";
		if(File.Exists(file)) {
			Stream s = File.Open(file, FileMode.Open);
			try {
				scoreBoard = (ScoreBoard)new BinaryFormatter().Deserialize(s);
			}
			catch (SerializationException e) {
				Debug.Log("Loading Failed: " + e.Message);
				throw;
			}
			finally {
				s.Close();
			}
		}
		else {
			scoreBoard = new ScoreBoard();
			SaveScores();
		}
		loaded = true;
		Debug.Log("Loaded");
	}
	
	/// <summary>
	/// Saves the scores.
	/// </summary>
	private static void SaveScores() {
		string file = Application.dataPath+"/hs";
		Stream s = File.Open(file, FileMode.Create);
		BinaryFormatter bf = new BinaryFormatter();
		try {
			bf.Serialize(s, scoreBoard);
		}
		catch (SerializationException e) {
			Debug.Log("Saving Failed: " + e.Message);
			throw;
		}
		finally {
			s.Close();
		}
		Debug.Log("Saved");
	}
	
	const string NOT_SET = "N/A";
	public Text trackNameText, vehicleNameText, basicRaceScoreText, timeTrialScoreText, eliminationScoreText, unlockText;
	public GameObject newBasic, newTimeTrial, newElimination, starBasic, starTimeTrial, starElimination;
	static ScoreBoard scoreBoard;
	bool newHighScore, newScore;
	int selectedTrack, selectedVehicle, selectedOption;
	public Animator menuAnimator, leftButton, rightButton;
	public static bool loaded = false;
	
	// Use this for initialization
	void Start () {
		loaded = false;
		newScore = false;
		newHighScore = false;
		
		selectedTrack = 1; selectedVehicle = 0;
		scoreBoard = new ScoreBoard();
		LoadScores();
		ShowScores();
	}
	
	/// <summary>
	/// Switches the selection horizontally.
	/// </summary>
	/// <param name="right">If set to <c>true</c> switch selection right.</param>
	public void SwitchSelectionH(bool right) {
		if(right) { 
			rightButton.Play("Activate");
			if(selectedOption == 0) {
				selectedTrack = WrapIndex(selectedTrack+1, 1, 2);
			}
			else {
				selectedVehicle = WrapIndex(selectedVehicle+1, 0, scoreBoard.vehicleNames.Length-1);
			}
		}
		else {
			leftButton.Play("Activate");
			if(selectedOption == 0) {
				selectedTrack = WrapIndex(selectedTrack-1, 1, 2);
			}
			else {
				selectedVehicle = WrapIndex(selectedVehicle-1, 0, scoreBoard.vehicleNames.Length-1);
			}
		}
		if(newScore) {
			newBasic.SetActive(false);
			newTimeTrial.SetActive(false);
			newElimination.SetActive(false);
		}
		if(newHighScore) {
			unlockText.gameObject.SetActive(false);
			newHighScore = false;
		}
		ShowScores();
	}
	
	/// <summary>
	/// Wraps the given value within a range.
	/// </summary>
	/// <returns>The index.</returns>
	/// <param name="value">the value to wrap.</param>
	/// <param name="min">Minimum range.</param>
	/// <param name="max">Maximum range.</param>
	int WrapIndex(int value, int min, int max) {
		if(value > max)
			return min;
		else if (value < min) 
			return max;
		return value;
	}
	
	/// <summary>
	/// Switchs the selection vertically.
	/// </summary>
	/// <param name="down">If set to <c>true</c> switch selection down.</param>
	public void SwitchSelectionV(bool down) {
		if(down) {
			selectedOption = WrapIndex(selectedOption-1, 0, 1);
		}
		else {
			selectedOption = WrapIndex(selectedOption+1, 0, 1);
		}
		if(selectedOption == 0) 
			menuAnimator.Play("MoveToTrackSelect");
		else 
			menuAnimator.Play("MoveToVehicleSelect");
			
	}
	
	/// <summary>
	/// Checks the scores.
	/// </summary>
	/// <param name="playerData">th player data to check.</param>
	public void CheckScores(PlayerData playerData) {
		selectedTrack = playerData.selectedLevel;
		for(int i = 0; i < scoreBoard.vehicleNames.Length; i++) {
			if(scoreBoard.vehicleNames[i] == playerData.selectedVehicle) {
				selectedVehicle = i;
			}
		}
		newScore = false;
		ScoreTable toCheck = scoreBoard.getScoreTable(playerData.selectedLevel, playerData.selectedVehicle);
		
		switch((RaceType)playerData.selectedRaceType) {
		case RaceType.Basic:
			if(playerData.finalPosition < toCheck.basicRacePosition) {
				toCheck.basicRacePosition = playerData.finalPosition;
				newScore = true;
				newBasic.SetActive(true);
				if(playerData.finalPosition == 1) {
					newHighScore = true;
				}
			}
			break;
		case RaceType.TimeTrial:
			if(playerData.bestTime < toCheck.timeTrialResult) {
				toCheck.timeTrialResult = playerData.bestTime;
				newScore = true;
				newTimeTrial.SetActive(true);
				if(playerData.bestTime < playerData.GetBestLapTime()) {
					ScoreTable st = scoreBoard.getScoreTable(playerData.selectedLevel, playerData.selectedVehicle);
					if(!st.BestTimeBeat) {
						newHighScore = true;
						scoreBoard.SetTimeBeaten(playerData.selectedLevel, playerData.selectedVehicle);
					}
				}
			}
			break;
		case RaceType.Elimination:
			if(playerData.finalPosition < toCheck.eliminationPosition) {
				toCheck.eliminationPosition = playerData.finalPosition;
				newScore = true;
				newElimination.SetActive(true);
				if(playerData.finalPosition == 1) {
					newHighScore = true;
				}
			}
			break;
		}
		if(newScore) {
			scoreBoard.setScoreTable(playerData.selectedLevel, playerData.selectedVehicle, toCheck);
			SaveScores();	
		}
		ShowScores();
		if(newHighScore && Unlocker.canUnlock()) {
			unlockText.gameObject.SetActive(true);
			unlockText.text = Unlocker.UnlockNew() + "\nUnlocked";
		}
	}
	
	/// <summary>
	/// Shows the scores.
	/// </summary>
	void ShowScores() {
		trackNameText.text = "Track " +selectedTrack;
		vehicleNameText.text = scoreBoard.vehicleNames[selectedVehicle];
		ScoreTable table = scoreBoard.getScoreTable(selectedTrack, scoreBoard.vehicleNames[selectedVehicle]);
		if(table.basicRacePosition < 7) 
			basicRaceScoreText.text = Dave.GetOrdinal(table.basicRacePosition);
		else
			basicRaceScoreText.text = NOT_SET;
		if(table.timeTrialResult < 10000000) 
			timeTrialScoreText.text = table.timeTrialResult.ToString("0.##");
		else
			timeTrialScoreText.text = NOT_SET;
		if(table.eliminationPosition < 7) 
			eliminationScoreText.text = Dave.GetOrdinal(table.eliminationPosition);
		else
			eliminationScoreText.text = NOT_SET;
		starBasic.SetActive(table.basicRacePosition == 1);	
		starTimeTrial.SetActive(table.BestTimeBeat);
		starElimination.SetActive(table.eliminationPosition == 1);
	}
}
