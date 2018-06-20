using UnityEngine;
using System.Collections;

// Menu Enums
public enum GameState { BrowsingMenu = 0, PlayingGame = 1 };
public enum MenuLevel { None = -1, MainMenu = 0, VehicleSelect = 1, TrackSelect = 2, Options = 3, PlayerData = 4 };

//PlayerData Enums
public enum RaceType { None = -1, Basic = 0, TimeTrial = 1, Elimination = 2 };

//PathBuilder Enums
public enum CheckPointType { None, SegmentLine, LapLine }

// Dave enums
public enum TimerType { Start = 0, Eliminate = 1}