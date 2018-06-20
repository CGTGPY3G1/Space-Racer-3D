using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine.UI;

/// <summary>
/// Dave the Driver Analysis and Velocity Evaluation bot.
/// Dave spawns and tracks a number of vehicle drones.
/// </summary>

[System.Serializable]
[XmlRoot]
public class Dave : MonoBehaviour {

	// used to store drone Identification and Instantiation data
	[System.Serializable]
	public struct DroneDef {
		public string name;
		public GameObject dronePrefab;
	}
	
	// Data retrieved from drones about lap performance
	[XmlArray]
	[XmlArrayItem]
	public static List <LapData> savedLapData;
	
	// A self sortable list of drones
	[XmlArray]
	[XmlArrayItem]
	public List <PositionInfo> racerList;
	// Cached drone scripts
	[XmlArray]
	[XmlArrayItem]
	public Drone[] drones;
	
	// Caches drone scripts
	void AddDrone(Drone d){
		Drone[]temp = new Drone[drones.Length+1];
		for(int i = 0; i < drones.Length; i++) {
			temp[i] = drones[i];
		}
		temp[temp.Length-1] = d; 
		drones = temp;
	}
	
	// For posting race results
	public ResultScript resultScript;
	
	// The Selected player vehicle
	Drone playerVehicle;
	// List of drone names and Prefabs
	public DroneDef[] dronePrefabs;
	// Possible Spawn Points
	public Transform[] spawnPoints;
	// UI Text components for displaying race position/lap count/lap time
	public Text positionText, currentLapText, lapTimer, startText;
	// Cached Camera Script
	public CamScript cameraScript;
	// Cached Speedometer Script
	public SpeedometerScript speedometer;
	// Available Music
	public List<AudioClip> music;
	// Cached Player Data (Scores/Positions etc)
	private PlayerData playerData;
	// the selected Race Type
	RaceType raceType;
	// number of drones that have been disabled (for elimination races)
	int disabledDrones;
	// time until the race starts
	float raceStartTimer = 10000;
	// the beep counter
	int countStart;
	// Used to determine is dave is set up, if race has started and if race has ended
	private bool isSetUp, raceOver, hideStartText;
	public bool IsSetUp {
		get{ return isSetUp; }
	}
	// Sets uo the race based on the choices made in the menu selection system
	public void SetUpRace(PlayerData playerData) {
		this.playerData = playerData;
		raceType = (RaceType)playerData.selectedRaceType;
		Init();
		isSetUp = true;
	}
	
	/// <summary>
	/// Randomises the drones positions.
	/// </summary>
	void RandomiseDrones() {
		DroneDef[] newDrones = new DroneDef[6];
		List<int> possibleIndices = new List<int>() { 0, 1, 2, 3, 4, 5 };
		for(int i = 0; i < 6; i++) {
			int rand = Random.Range(0, possibleIndices.Count);
			int index = possibleIndices[rand];
			newDrones[i] = dronePrefabs[index];
			possibleIndices.RemoveAt(rand);
		}
		dronePrefabs = newDrones;
	}
	public RaceStartsScript raceStartsScript;
	void Start () {
		isSetUp = false;
		raceOver = false;
		raceType = RaceType.None;
		disabledDrones = 0;
		raceStartTimer = 8;
		countStart = 3;
		hideStartText = false;
	}
	
	// Initializes Dave with the relevant race parameters
	void Init() {
		savedLapData = new List<LapData>();
		racerList = new List<PositionInfo>();
		RandomiseDrones();
		SpawnRacers();
		for(int i = 0; i < drones.Length; i++) {
			drones[i].Init();
			drones[i].positionInfo.droneNumber = i+1;
		}
		raceOver = false;
		disabledDrones = 0;
		raceStartTimer = 8;
		countStart = 3;
		hideStartText = false;
		// Set a random music track
		AudioSource aS = GetComponent<AudioSource>();
		aS.clip = music[Random.Range(0, music.Count)];
		aS.Play();
		for(int i = 0; i < drones.Length; i++) {
			racerList.Add(drones[i].positionInfo);
		}
	}
	
	// used to delay Switching Back to the Menu
	float mainMenuTimer;
	// Update is called once per frame
	void Update () {
		if(raceStartTimer < -2) {
			if(playerVehicle.IsFacingForward) {
				if(startText.text != "")
					startText.text = "";
			}
			else {
				if(startText.text != "Wrong\nWay")
					startText.text = "Wrong\nWay";
			}
		}
		// if the race is over, count down the menu timer before loading the menu
		if(raceOver) {
			if(mainMenuTimer > 0) {
				mainMenuTimer -= Time.deltaTime;
			}
			else {
				GameObject.Find("GameManager").GetComponent<GameManager>().SetRaceEnd();
				mainMenuTimer = 1000;
			}
		}
	}
	
	// called every physics time-step
	void FixedUpdate () {	
		CheckRaceConditions();
	}
	
	// called after every frame
	void LateUpdate () {
		// Hide the word Go after 2 seconds
		if(raceStartTimer <= 0) {
			if(!hideStartText) {
				if(raceStartTimer > -2) {
					raceStartTimer -= Time.deltaTime;
				}
				else {
					startText.text = "";
					hideStartText = true;
				}
			}
			// sort drones(see PositionInfo class for sorting criteria)
			racerList.Sort();
			// Display the sorted list with ordinal positions 
			if(positionText && raceType != RaceType.TimeTrial) {
				string toPrint = "";
				for(int i = 0; i < racerList.Count; i++) {
					if(i == 1)
						toPrint += racerList[i].droneName + "  " + GetOrdinal(i+1)+"\n";
					else 
						toPrint += racerList[i].droneName + "   " + GetOrdinal(i+1)+"\n";
				}
				positionText.text = toPrint;
			}
		}
	}	
	
	/// <summary>
	/// Counts down th given timer.
	/// </summary>
	/// <param name="type">the timer to count down.</param>
	/// <param name="delta">the frame time.</param>
	void CountDownTimer(TimerType type, float delta) {
		if(type == TimerType.Eliminate) {
			if (playerData.eliminationTime > 0) {
				playerData.eliminationTime -= delta;
				lapTimer.text =  playerData.eliminationTime.ToString("0.##");if(countStart > 0) {
					if(playerData.eliminationTime <= countStart) {
						raceStartsScript.PlayClip(countStart);
						countStart--;
					}
				}
			}
			else {
				raceStartsScript.PlayClip(countStart);
				countStart = 3;
				// Disable the racer in last position racer
				EliminateRacer();
			}
		}
		else if (type == TimerType.Start) {
			raceStartTimer -= delta;
			if(countStart > 0) {
				if(raceStartTimer <= countStart) {
					startText.text = countStart.ToString();
					raceStartsScript.PlayClip(countStart);
					countStart--;
				}
			}
			else if(raceStartTimer <= 0) {
				raceStartsScript.PlayClip(countStart);
				countStart = 3;
				for(int i = 0; i < drones.Length; i++) {
					drones[i].IsEnabled = true;
				}
				startText.text = "Go";
			}
		}
	}
	
	// Evaluates race conditions to check for winners and losers
	void CheckRaceConditions() {
		float fixedDelta = Time.fixedDeltaTime;
		// if Dave is set up and the race has not ended
		if(isSetUp && !raceOver) {
			// if the race has started
			if(raceStartTimer < 0) {
				// Evaluate the selected race type
				switch(raceType) { 
				case RaceType.Basic:
					// if the player has completed the 3rd lap
					if(playerVehicle.LapReached > 3) {
						string result;
						//switch to AI control
						playerVehicle.aiControlled = true;
						// Sort the drones
						racerList.Sort();
						// get the players final position
						playerData.finalPosition = GetPlayerPosition();
						// if the player is in position 1 they have won, if not they have lost
						if(playerData.finalPosition == 1) {
							result = "You Win\n\n";
						}
						else {
							result = "You Lose\n\n";
						}
						// set race as over
						raceOver = true;
						// set the menu countdown time
						mainMenuTimer = 8;
						result += "you placed\n"+ GetOrdinal(playerData.finalPosition);
						resultScript.ShowResult(result);
					}
					break;
				case RaceType.TimeTrial:
					// if the player has completed the first lap 
					// !raceOver is only relevant when mainMenuTimer is more than the time 
					// it takes to complete a lap (as the attempt time would be overwritten)
					if(playerVehicle.LapReached > 1 && !raceOver) {
						string result;
						//switch to AI control
						playerVehicle.aiControlled = true;
						// stor the end time and check it againt the target time for this lap
						playerData.bestTime = playerVehicle.lastLapTime;
						// if the players time is less than the target they have won, if not they have lost.
						// Matching the target time will not result in a win
						if(playerData.bestTime < playerData.GetBestLapTime()) {
							result = "You Win\n\n";
						}
						else {
							result = "You Lose\n\n";
						}
						// set race as over
						raceOver = true;
						// set the menu countdown time
						mainMenuTimer = 8;
						result += "Lap Time\n"+ playerData.bestTime.ToString("0.00") + " seconds";
						resultScript.ShowResult(result);
					}
					break;
				case RaceType.Elimination:
					// if the player is disabled or all other racers are disabled
					if(!playerVehicle.IsEnabled || disabledDrones == 5) {
						string result;
						// if the players is enabled they have won, if not they have lost. 
						if(playerVehicle.IsEnabled)
							result = "You Win\n\n";
						else
							result = "You Lose\n\n";
						// set race as over
						raceOver = true;
						// set the menu countdown time
						mainMenuTimer = 8;
						result += "you placed\n"+ GetOrdinal(playerData.finalPosition);
						resultScript.ShowResult(result);
					}
					else {
						CountDownTimer(TimerType.Eliminate, fixedDelta); 
					}	
					break;
				}
			}
			else {
				// count down the race timer then activate the Drones
				CountDownTimer(TimerType.Start, fixedDelta);
			}
		}
	}
	
	// Eliminate the racer in last position
	void EliminateRacer() {
		string name = racerList[racerList.Count-1].droneName;
		foreach(Drone d in drones) {
			if(name == d.positionInfo.droneName) {
				currentLapText.text = d.positionInfo.droneName + "\nEliminated!";
				if(name == playerVehicle.positionInfo.droneName) {
					playerData.finalPosition = GetPlayerPosition();
					d.DeactivateDrone(100);
					break;
				}
				else {
					if(racerList.Count <= 2)
						playerData.finalPosition = GetPlayerPosition();
					d.DeactivateDrone(5);
					break;
				}		
			}
		}
		racerList.RemoveAt(racerList.Count-1);
		// increment the disabled drone count
		disabledDrones++;
		playerData.IncrementEliminationTime();
	}
	
	// returns the players race position
	int GetPlayerPosition() {
		racerList.Sort();
		for(int i = 0; i < racerList.Count; i++) {
			if(playerVehicle.positionInfo.droneName == racerList[i].droneName){
				return i+1;
			}
		}
		return 1;
	}
	
	// spawn racer/racers based on selected race type
	void SpawnRacers() {
		// Set up time trial
		if(raceType == RaceType.TimeTrial) {
			SpawnDrone(playerData.selectedVehicle, 6);
			positionText.text = "Time To Beat"+"\n<<<<<< "+playerData.GetBestLapTime().ToString("0.00")+" >>>>>>";
		}
		else {
			// Set up basic/elimination races
			for(int i = 0; i < dronePrefabs.Length; i++) {
				SpawnDrone(dronePrefabs[i].name, i);
			}
		}
		// Set the player vehicle
		SetPlayer(playerData.selectedVehicle);
	}
	
	// Sets up player with relevant UI components while linking
	// the player controlled drone to the camera and speedometer
	// Sets all non-player drones as AI controlled
	void SetPlayer(string droneName) {
		foreach(Drone d in drones){
			if(d.transform.name == droneName) {
				d.aiControlled = false;
				if(speedometer)
					speedometer.SetDrone(d);
				if(currentLapText && raceType == RaceType.Basic) {
					d.currentLapText = currentLapText;
				}
				else {
					currentLapText.fontSize = 30;
					currentLapText.alignment = TextAnchor.UpperCenter;
				}
				if(lapTimer && raceType != RaceType.Elimination)
					d.lapTimer = lapTimer;
				cameraScript.target = FindLookTarget(d);
				playerVehicle = d;
			}
			else {
				d.aiControlled = true;
			}
		}
	}
	
	// Returns the selected drones Look Targer (for the camera to use)
	Transform FindLookTarget(Drone drone) {
		// checks 3 layers of the vehicles transform hierarchy of the cameras Look Target
		foreach(Transform t in drone.transform) { 
			if(t.name == "LookTarget") {
				return t;
			}
			else {
				foreach(Transform u in t) {
					// LookTarget should be found somewhere on this level of the transforms hierarchy
					if(u.name == "LookTarget") {
						return u;
					}
					else {
						// This is an extra check
						foreach(Transform v in u) {
							if(v.name == "LookTarget") {
								return v;
							}
						}
					}
				}
			}
		}
		// retuen null if the target was not found
		Debug.Log("Look Target Not Found");
		return null;
	}
	
	// spawn the drone defined by droneName in the position defined by spawnPoint
	void SpawnDrone(string droneName, int spawnPoint) {
		// used to determine is the drone exists
		bool droneExists = false;
		// search for the selected drone
		foreach(DroneDef d in dronePrefabs){
			
			if(d.name == droneName) {
				// Instantiate the drone in the relevant position and orientation
				GameObject toSpawn = (GameObject)Instantiate(d.dronePrefab, spawnPoints[spawnPoint].position, spawnPoints[spawnPoint].rotation);
				// set the drone name to remove the (clone) postfix added by unitys Instantiate method
				toSpawn.transform.name = droneName;
				// Add the drone to the drone list
				AddDrone(toSpawn.GetComponent<Drone>());
				// notify of drones existance and break from foreach loop
				droneExists = true;
				break;	
			}
		}
		// Print relevant Debug data
		if(droneExists){
			Debug.Log(droneName + " Spawned!");
		}
		else {
			Debug.Log(droneName + " Not Found!");
		}
	}
	
	// cached reference to drone reports folder postfix
	private static string reportsFolder = "/Reports/";
	// Saves reposts sent from drones
	public static void SaveReport(LapData lapData, string droneName) {
		// set initial folder path
		string filePath = Application.dataPath + reportsFolder+droneName+"/";
		// create the folder if the folder doesn't exist 
		if(!Directory.Exists(filePath)) { 
			Directory.CreateDirectory(filePath); 
		}
		
		// used to number the reports
		int reportNumber = 0;
		string fileName;
		do {
			// increment the report number until an unused nummber is found
			reportNumber++;
			fileName = "Race Data "+ reportNumber + ".txt";
		}while(File.Exists(filePath+fileName));
		// tab escape sequence
		string tab = "\t";
		// create a new list of strings to compile the report
		List <string> report = new List<string>();
		report.Add("Report Number " + reportNumber + ", Recorded on " + System.DateTime.Now.ToLongDateString()+ " at "+System.DateTime.Now.ToShortTimeString() + ".");
		report.Add("");
		report.Add("Lap ID: "+lapData.LapID+".  Total Time : " + lapData.totalTime);
		report.Add("Average Speed: "+lapData.averageSpeed+".  Collision Count: " + lapData.noOfCollisions);
		report.Add("");
		for(int i = 0; i < lapData.segmentData.Length; i++) {
			SegmentData sd = lapData.segmentData[i];
			report.Add(tab+"Segment: "+(i)+".  Total Time : " + sd.totalTime);
			report.Add(tab+"Average Speed: "+sd.averageSpeed+".  Collision Count: " + sd.noOfCollisions);
			report.Add(tab+"Number of Waypoints : " + sd.noOfWaypoints);	
			report.Add(tab+"Sensor Data");
			report.Add(tab+"Angles");
			report.Add(tab+tab+"Back Angle: " +sd.sensorData.backAngle);		
			report.Add(tab+"Forces");
			report.Add(tab+tab+"Back Sensor Avoidance: "+sd.sensorData.backRedirectForce); //+"  -  Front Sensor Avoidance: " + sd.sensorData.frontRedirectForce);	
			report.Add(tab+tab+"Front Ray Scale: "+sd.sensorData.frontRayScale);//+".  Front Steer Scale: " + sd.sensorData.steerScaler);	
			report.Add(tab+tab+"Back Ray Length: " + sd.sensorData.backRayLength);//+".   SteerRay Length: " + sd.sensorData.steerRayLength);
			report.Add("");
			for(int j = sd.startPoint; j < sd.startPoint+sd.noOfWaypoints; j++) {
				WaypointData wd = lapData.waypointsData[j];
				report.Add(tab+tab+tab+"Waypoint: "+(j+1)+".  Number: "+ wd.number+".  Time Passed: " + wd.time);
				report.Add(tab+tab+tab+"New Target: "+wd.targetWaypoint+".  Speed : " + wd.speed);
				report.Add(tab+tab+tab+"Point Crossed: "+wd.pointCrossed.ToString()+".  Target Point : " + wd.targetPoint.ToString());	
				report.Add("");
			}
			report.Add("");
		}
		// save the report to a text file
		using (StreamWriter file = new StreamWriter(filePath+fileName)) {
			foreach (string line in report) {
				file.WriteLine(line);
			}
		}
		// sve a loadable version of the lap data in XML
		SerializeLapData(lapData, droneName, reportNumber);
		// add yo the list of complede laps
		savedLapData.Add(lapData);
	}
	
	// cached reference to drone data folder postfix
	private static string droneDataFolder = "Drone Data/";
	// uses XML serialization to save lap Data
	public static void SerializeLapData(LapData lapData, string droneName, int reportNumber) {
		string filePath = Application.dataPath + reportsFolder+droneName+"/";;
		if(!Directory.Exists(filePath + droneDataFolder)) { Directory.CreateDirectory(filePath + droneDataFolder); }
		using (Stream fileStream = new FileStream(filePath + droneDataFolder + "LapData " + reportNumber + ".dddf", FileMode.Create, FileAccess.Write, FileShare.None)) {
			XmlSerializer serializer = new XmlSerializer(typeof(LapData));
			serializer.Serialize(fileStream, lapData);
		}
	}
	
	// uses XML de-serialization to load lap Data
	private void LoadLapData(string droneName) {
		string filePath = Application.dataPath + reportsFolder+droneName+"/";;
		if(!Directory.Exists(filePath + droneDataFolder)) { Directory.CreateDirectory(filePath + droneDataFolder); }
		for(int i = 1; i < int.MaxValue; i++) {
			string fileName = filePath + droneDataFolder + "LapData " + i + ".dddf";
			if(!File.Exists(fileName)) 
				return;
			using (Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				XmlSerializer deserializer = new XmlSerializer(typeof(LapData));
				savedLapData.Add((LapData)deserializer.Deserialize(fileStream));
			}
		}
	}
	
	// get the ordinal value (1st, 2nd, 3rd etc) of a integer value
	public static string GetOrdinal(int toConvert) {
		// Will return the the correct ordinal of any number greater than 0 and less tham 11
		// will only be sent values from 1 to 6   
		switch(toConvert) {
		case 1:
			return toConvert + "st";
		case 2:
			return toConvert + "nd";
		case 3:
			return toConvert + "rd";
		default:
			return toConvert + "th";
		}
	}
}