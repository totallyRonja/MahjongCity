using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel : MonoBehaviour {
    public TextAsset Level;
    
    void Awake() {
        if (LevelSystem.CurrentLevelData == null)
            LevelSystem.CurrentLevelData = Level;
    }
}
