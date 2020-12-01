using System.Collections;
using System.Collections.Generic;
using Scriptbag;
using UnityEngine;
using UnityEngine.EventSystems;

[SelectionBase]
public class LevelButton : MonoBehaviour, IPointerClickHandler {

    public GameObject Unsolved;
    public GameObject Solved;

    public string LevelKey = "unnamed";
    public TextAsset LevelData;
    bool solved;
    
    void Start() {
        SetSolved(SaveSystem.IsSolved(LevelKey));
    }

    void SetSolved(bool isSolved) {
        solved = isSolved;
        Unsolved.SetActive(!isSolved);
        Solved.SetActive(isSolved);
    }

    public void OnPointerClick(PointerEventData eventData) {
        LevelSystem.LoadLevel(LevelData);
    }
}
