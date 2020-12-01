using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelSystem {
	public static TextAsset CurrentLevelData;

	const string LevelsScene = "Scenes/Levels";

	//todo: consider defining levels via scriptableobjects to allow to associate more data with them
	public static void LoadLevel(TextAsset level) {
		CurrentLevelData = level;
		//todo: put loading screen inbetween here and load async for slower devices?
		SceneManager.LoadScene(LevelsScene);
	}
}
