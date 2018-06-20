using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	// used to disable menus that are no longer being viewed
	public class MenuDisabler {
		// time until the object is disabled
		public float lifeTime;
		// indicates whether or not the GmaeObject is enabled
		public bool isEnabled;
		// The GameObject to disable.
		GameObject toDisable;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GameManager+MenuDisabler"/> class.
		/// </summary>
		/// <param name="toDisable"> The GameObject to disable.</param>
		/// <param name="lifeTime"> The time to wait before disabling the GameObject.</param>
		public MenuDisabler(GameObject toDisable, float lifeTime) {
			this.lifeTime = lifeTime; this.toDisable = toDisable;
			isEnabled = true;
		}
		
		/// <summary>
		/// Counts down the GameObjects remaining life.
		/// </summary>
		/// <param name="delta"> The frame time.</param>
		public void CountDown(float delta) {
			lifeTime -= delta;
			if(lifeTime <= 0)
				Disable();
		}
		
		/// <summary>
		/// Disable this instance.
		/// </summary>
		void Disable() {
			toDisable.SetActive(false);
			isEnabled = false;
		}
	}
	
	// The success and failure sounds.
	public AudioClip success, failure;
	// The track preview image, and the locked overlay.
	public Image trackPreviewImage, trackLockImage;
	// The track preview sprites.
	public Sprite[] trackPreviews;
	int trackBeingViewed;
	/// <summary>
	/// Changes the selected track
	/// </summary>
	/// <param name="selectNext">If set to <c>true</c> select next.</param>
	void ChangeTrackSelection(bool selectNext) {
		if(selectNext) {
			trackBeingViewed++;
			if(trackBeingViewed >= trackPreviews.Length) 
				trackBeingViewed -= trackPreviews.Length;
		}
		else {
			trackBeingViewed--;
			if(trackBeingViewed < 0) 
				trackBeingViewed += trackPreviews.Length;
		}
		trackPreviewImage.sprite = trackPreviews[trackBeingViewed];
		trackLockImage.gameObject.SetActive(!Unlocker.IsTrackUnLocked(trackBeingViewed));
	}
	
	// The race type image.
	public Image raceTypeImage;
	// The race type sprites.
	public Sprite[] raceTypeSprites;
	/// The type of race selected.
	int selectedRaceType;
	/// <summary>
	/// Changes the selected race tyoe.
	/// </summary>
	/// <param name="selectNext">If set to <c>true</c> select next.</param>
	void ChangeRaceType(bool selectNext) {
		if(selectNext) {
			selectedRaceType++;
			if(selectedRaceType >= raceTypeSprites.Length) 
				selectedRaceType -= raceTypeSprites.Length;
		}
		else {
			selectedRaceType--;
			if(selectedRaceType < 0) 
				selectedRaceType += raceTypeSprites.Length;
		}
		raceTypeImage.sprite = raceTypeSprites[selectedRaceType];
	}
	
	// used to display the selected vehicles stats
	public Image vehicleInfoImage, vehicleLockImage;
	// The vehicle info sprites.
	public Sprite[] vehicleInfo;
	// The vehicle being viewed.
	int vehicleBeingViewed;
	/// <summary>
	/// Changes the selected vehicle.
	/// </summary>
	/// <param name="selectNext">If set to <c>true</c> select next.</param>
	void ChangeVehicleSelection(bool selectNext) {
		ShowRoomScript showRoomScript = vehicles[vehicleBeingViewed].GetComponent<ShowRoomScript>();
		showRoomScript.IsSelected = false;
		Vector3 targetPosition;
		// move the old vehicle out of the way
		if(selectNext) {
			vehicleBeingViewed++;
			showRoomScript.MoveToPosition(vehiclePositionRight.position, 1);
			if(vehicleBeingViewed >= vehicles.Length) 
				vehicleBeingViewed -= vehicles.Length;
			targetPosition = vehiclePositionLeft.position;
		}
		else {
			vehicleBeingViewed--;
			showRoomScript.MoveToPosition(vehiclePositionLeft.position, 1);
			if(vehicleBeingViewed < 0) 
				vehicleBeingViewed += vehicles.Length;
			targetPosition = vehiclePositionRight.position;
		}
		// move the new vehicle on screen
		showRoomScript = vehicles[vehicleBeingViewed].GetComponent<ShowRoomScript>();
		if(vehicles[vehicleBeingViewed].position != targetPosition)
			showRoomScript.TeleportToPosition(targetPosition, vehiclePositionCentre.rotation);
		showRoomScript.MoveToPosition(vehiclePositionCentre.position, 3);
		showRoomScript.IsSelected = true;
		vehicleInfoImage.sprite = vehicleInfo[vehicleBeingViewed];
		vehicleLockImage.gameObject.SetActive(!Unlocker.IsVehicleUnLocked(vehicleBeingViewed));
	}
	
	// The vehicle models.
	public Transform[] vehicles;
	// used for vehicle placement during menu scenes
	public Transform vehiclePositionLeft, vehiclePositionCentre, vehiclePositionRight;
	// The current game state.
	GameState state;
	// The current and new menu levels
	public MenuLevel currentLevel, newLevel;
	// animators for controlling the transitions between menus
	public Animator MainMenuPanelAnim, TitleAnim, vehicleSelectTitleAnim, vehicleInfoAnim, trackSelectAnim, raceTypeAnim, previewAnim;
	// used to manipulate the displayed info abound pressing the back button
	public BackButtonContextScript backButtonContext;
	// A list of menu items to be disabled
	List<MenuDisabler> toDisable = new List<MenuDisabler>();
	// used to determine wether input should be controlled by the editor or
	// controlled by this script.
	bool scriptedInput;
	// should the race type overlay be visable
	bool selectingRaceType;
	// used to determine if the players score should be 
	// checked when entering the Player Data screen
	bool checkScore;
	// used to prevent rapid key presses
	float inputTimer;
	
	PlayerData playerData;
	/// <summary>
	/// ends the race.
	/// </summary>
	/// <param name="pd">Pd.</param>
	public void SetRaceEnd() { 
		checkScore = true;
		ShowPlayerDataScreen();
	}
	public GameObject eventSystem;
	bool eventSystemActive;
	public static GameManager singletonReference;
	// Use this for initialization
	void Start () {
		checkScore = false;
		DontDestroyOnLoad(this);
		if(singletonReference)
			DestroyImmediate(singletonReference);
		singletonReference = this;
		playerData = new PlayerData();
		Cursor.visible = false; 
		Reset();	
	}
	
	void Reset() {
		inputTimer = 0;
		selectingRaceType = false;
		currentLevel = MenuLevel.None;
		newLevel = MenuLevel.MainMenu;
		state = GameState.BrowsingMenu;
		dave = null;
		playAnimation();
	}
	
	Dave dave;
	HighScores highScores;
	// Update is called once per frame
	void Update () {
		float delta = Time.deltaTime;
		if(state == GameState.BrowsingMenu) {
			if(toDisable.Count > 0) {
				bool clearList = true;
				foreach(MenuDisabler md in toDisable) {
					if(md.isEnabled) {
						md.CountDown(delta);
						clearList = false;
					}
				}
				if(clearList) 
					toDisable.Clear();
			}
			
			if(currentLevel == MenuLevel.PlayerData) {
				if(!highScores) {
					highScores = FindScoreBoard();
				}
				else if(HighScores.loaded && checkScore) {
					highScores.CheckScores(playerData);
					checkScore = false;
					Debug.Log("score checked");
				}
			}
			else if (!eventSystemActive && currentLevel == MenuLevel.MainMenu && inputTimer <= 0) {
				eventSystemActive = true;
				eventSystem.SetActive(eventSystemActive);
			}
			if(inputTimer <= 0) {
				if(scriptedInput) 
					ProcessMenuInput(delta);
				if(Input.GetButtonDown("Cancel"))
					HitBackButton();
			}
			else {
				inputTimer -= delta;
			}
		}
		else {
			if(!dave) {
				dave = FindDave();
			}
			else {
				if(!dave.IsSetUp) {
					Debug.Log("Found Dave");
					dave.SetUpRace(playerData);
				}
			}
		}
		
		
	}
	
	/// <summary>
	/// Finds the dave.
	/// </summary>
	/// <returns>The dave.</returns>
	Dave FindDave() {
		GameObject bot = GameObject.Find("Bot");
		if(!bot) {
			Debug.Log("Couldn't Find Bot");
			return null;
		}
		else {
			Debug.Log("Found Bot");
			Dave fDave = bot.GetComponent<Dave>() as Dave;
			if(fDave)
				Debug.Log("Found Dave");
			else 
				Debug.Log("Couldn't Find Dave");
				
			return bot.GetComponent<Dave>() as Dave;
		}
	}
	
	/// <summary>
	/// Finds the score board.
	/// </summary>
	/// <returns>The score board.</returns>
	HighScores FindScoreBoard() {
		GameObject tracker = GameObject.Find("ScoreTracker");
		if(!tracker) {
			Debug.Log("Couldn't Find ScoreTracker");
			return null;
		}
		else {
			Debug.Log("Found ScoreTracker");
			HighScores scores = tracker.GetComponent<HighScores>() as HighScores;
			if(scores)
				Debug.Log("Found HighScores");
			else 
				Debug.Log("Couldn't Find HighScores");
			return scores;
		}
	}
	
	/// <summary>
	/// Processes the menu input.
	/// </summary>
	/// <param name="delta">the frame time.</param>
	void ProcessMenuInput(float delta) {
		if(currentLevel == MenuLevel.VehicleSelect) {
			float scrollValue = Input.GetAxis("Horizontal");
			if(scrollValue > 0.2f) {
				ChangeVehicleSelection(true);
				inputTimer = 1;
			}
			else if(scrollValue < -0.2f) {
				ChangeVehicleSelection(false);
				inputTimer = 1;
			}
			else if(Input.GetButtonDown("Submit")) {
				if(!Unlocker.IsVehicleUnLocked(vehicleBeingViewed)) {
					GetComponent<AudioSource>().PlayOneShot(failure);
				}	
				else {
					GetComponent<AudioSource>().PlayOneShot(success);
					ShowTrackSelect();
				}
			}
		}
		else if(currentLevel == MenuLevel.TrackSelect) {
			float scrollValue = Input.GetAxis("Horizontal");
			if(scrollValue > 0.2f) {
				if(selectingRaceType)
					ChangeRaceType(true);
				else
					ChangeTrackSelection(true);
				inputTimer = 0.2f;
			}
			else if(scrollValue < -0.2f) {
				if(selectingRaceType)
					ChangeRaceType(false);
				else 
					ChangeTrackSelection(false);
				inputTimer = 0.2f;
			}
			else if(Input.GetButtonDown("Submit")) {
				if(selectingRaceType){
					GetComponent<AudioSource>().PlayOneShot(success);
					playerData.SetRaceConditions((RaceType)selectedRaceType);
					if((RaceType)selectedRaceType == RaceType.TimeTrial) {
						float highestScore = HighScores.GetBestTime(playerData.selectedVehicle, playerData.selectedLevel);
						float bestTime = playerData.GetBestLapTime();
						if(bestTime > highestScore)
							playerData.SetBestLapTime(highestScore);
					}
					state = GameState.PlayingGame;
					newLevel = MenuLevel.None;
					playAnimation();
					LoadLevel(playerData.selectedLevel);
				}
				else  {
					if(!Unlocker.IsTrackUnLocked(trackBeingViewed)) {
						GetComponent<AudioSource>().PlayOneShot(failure);
					}	
					else {
						GetComponent<AudioSource>().PlayOneShot(success);
						ShowRaceTypeSelection(true);
					}
				}
				inputTimer = 0.6f;
			}
		}
		else if(currentLevel == MenuLevel.PlayerData && highScores) {
			float scrollValueH = Input.GetAxis("Horizontal");
			if(scrollValueH > 0.2f) {
				highScores.SwitchSelectionH(true);
				inputTimer = 0.25f;
			}
			else if(scrollValueH < -0.2f) {
				highScores.SwitchSelectionH(false);
				inputTimer = 0.25f;
			}
			float scrollValueV = Input.GetAxis("MenuVert");
			if(scrollValueV > 0.2f) {
				highScores.SwitchSelectionV(false);
				inputTimer = 0.25f;
			}
			else if(scrollValueV < -0.2f) {
				highScores.SwitchSelectionV(true);
				inputTimer = 0.25f;
			}
		}
	}
	
	/// <summary>
	/// Hit the back button.
	/// </summary>
	void HitBackButton() {
		GetComponent<AudioSource>().PlayOneShot(failure);
		switch(currentLevel) {
		case MenuLevel.MainMenu:
			Application.Quit();
			break;
		case MenuLevel.VehicleSelect:
			ShowMainMenu();
			break;
		case MenuLevel.TrackSelect:
			if(selectingRaceType)
				ShowRaceTypeSelection(false);
			else 
				ShowVehicleSelect(false);
			inputTimer = 1;
			break;
		case MenuLevel.Options:
			GameManager.LoadLevel(0);
			break;
		case MenuLevel.PlayerData:
			highScores = null;
			GameManager.LoadLevel(0);
			break;
		}
	}
	
	/// <summary>
	/// Shows the vehicle select menu.
	/// </summary>
	/// <param name="fromMenu"> set to <c>true</c> if coming from main menu.</param>
	public void ShowVehicleSelect(bool fromMenu) {
		inputTimer = 1;
		if(fromMenu)
			GetComponent<AudioSource>().PlayOneShot(success);
		else
			GetComponent<AudioSource>().PlayOneShot(failure);	
		newLevel = MenuLevel.VehicleSelect;
		playAnimation();
	} 
	
	/// <summary>
	/// Shows the track select menu.
	/// </summary>
	public void ShowTrackSelect() {
		inputTimer = 1;
		playerData.selectedVehicle = vehicles[vehicleBeingViewed].name;
		newLevel = MenuLevel.TrackSelect;
		playAnimation();
	}
	
	/// <summary>
	/// Shows the player data screen.
	/// </summary>
	public void ShowPlayerDataScreen() {
		GetComponent<AudioSource>().PlayOneShot(success);
		newLevel = MenuLevel.PlayerData;
		scriptedInput = true;
		currentLevel = newLevel;
		state = GameState.BrowsingMenu;
		LoadLevel(3);
	}
	
	/// <summary>
	/// Shows the main menu.
	/// </summary>
	public void ShowMainMenu() {
		inputTimer = 1;
		newLevel = MenuLevel.MainMenu;
		playAnimation();
	}
	
	/// <summary>
	/// Shows the options/credits.
	/// </summary>
	public void ShowOptions() {
		currentLevel = MenuLevel.Options;
		LoadLevel(4);
	}
	
	/// <summary>
	/// Shows the race type selection.
	/// </summary>
	/// <param name="show">If set to <c>true</c> show.</param>
	public void ShowRaceTypeSelection(bool show) {
		playerData.selectedLevel = trackBeingViewed+1;
		selectingRaceType = show;
		if (show) {
			selectedRaceType = 0;
			raceTypeImage.sprite = raceTypeSprites[selectedRaceType];
			raceTypeAnim.Play("RaceTypeReveal");
			previewAnim.Play("PreviewFade");
		}
		else {
			raceTypeAnim.Play("RaceTypeFade");
			previewAnim.Play("PreviewReveal");
		}
	}
	
	/// <summary>
	/// Plaies the selected animation.
	/// </summary>
	void playAnimation() {
		if(state == GameState.BrowsingMenu) {
			// Clean up old Screen
			switch(currentLevel) {
			case MenuLevel.MainMenu:
				MainMenuPanelAnim.Play("MainMenuExit"); 
				TitleAnim.Play("MainTitleExit");
				toDisable.Add(new MenuDisabler(MainMenuPanelAnim.gameObject, 0.8f));
				toDisable.Add(new MenuDisabler(TitleAnim.gameObject, 0.8f));
				eventSystemActive = false;
				eventSystem.SetActive(eventSystemActive);
				break;
			case MenuLevel.VehicleSelect:
				vehicleSelectTitleAnim.Play("VehicleSelectExit");
				toDisable.Add(new MenuDisabler(vehicleSelectTitleAnim.gameObject, 0.8f));
				vehicleInfoAnim.Play("VehicleInfoExit");
				toDisable.Add(new MenuDisabler(vehicleInfoAnim.gameObject, 0.8f));
				ShowRoomScript showRoomScript = vehicles[vehicleBeingViewed].GetComponent<ShowRoomScript>();
				showRoomScript.MoveToPosition(vehiclePositionRight.position, 1);
				showRoomScript.IsSelected = false;
				break;
			case MenuLevel.TrackSelect:
				trackSelectAnim.Play("TrackSelectExit");
				toDisable.Add(new MenuDisabler(trackSelectAnim.gameObject, 0.8f));
				break;
			}
			
			// Activate new Screen
			switch(newLevel) {
			case MenuLevel.MainMenu:
				MainMenuPanelAnim.gameObject.SetActive(true);
				MainMenuPanelAnim.Play("MainMenuReturn"); 
				TitleAnim.gameObject.SetActive(true);
				TitleAnim.Play("MainTitleEnter");
				scriptedInput = false;
				backButtonContext.CanReturn = false;
				
				break;
			case MenuLevel.VehicleSelect:
				vehicleSelectTitleAnim.gameObject.SetActive(true);
				vehicleSelectTitleAnim.Play("VehicleSelectEnter");
				vehicleInfoAnim.gameObject.SetActive(true);
				vehicleInfoAnim.Play("VehicleInfoEnter");
				backButtonContext.CanReturn = true;
				vehicleBeingViewed = 0;
				vehicleLockImage.gameObject.SetActive(!Unlocker.IsVehicleUnLocked(vehicleBeingViewed));
				scriptedInput = true;
				for(int i = 0; i < vehicles.Length; i++) {
					ShowRoomScript showRoomScript = vehicles[i].GetComponent<ShowRoomScript>();
					showRoomScript.TeleportToPosition(vehiclePositionLeft.position, vehiclePositionLeft.rotation);
					if(i == vehicleBeingViewed) {
						showRoomScript.MoveToPosition(vehiclePositionCentre.position, 3);
						showRoomScript.IsSelected = true;
					}
				}
				break;
			case MenuLevel.TrackSelect:
				trackSelectAnim.gameObject.SetActive(true);
				trackSelectAnim.Play("TrackSelectEnter");
				trackBeingViewed = 0;
				trackPreviewImage.sprite = trackPreviews[trackBeingViewed];
				trackLockImage.gameObject.SetActive(!Unlocker.IsTrackUnLocked(trackBeingViewed));
				scriptedInput = true;
				break;
			}
		}
		currentLevel = newLevel;
	}
	
	/// <summary>
	/// Loads a level.
	/// </summary>
	/// <param name="level">Level.</param>
	public static void LoadLevel(int level) { 			
		Application.LoadLevel(level);
	}
}
