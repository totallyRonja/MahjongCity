using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour {
    
    public Image LineSegment;

    List<Image> segments = new List<Image>();

    public void SetLine(Vector2[] path) {
        //setup
        while (path.Length - 1 > segments.Count) {
            segments.Add(Instantiate(LineSegment, transform));
        }
        Clear();
        
        //put segments where we want them
        for (int i = 0; i < path.Length-1; i++) {
            Vector2 from = path[i];
            Vector2 to = path[i+1];
            Vector2 center = (from + to) / 2;
            Vector2 diff = to - from;
            diff = new Vector2(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
            
            segments[i].gameObject.SetActive(true);
            segments[i].rectTransform.position = center;
            segments[i].rectTransform.sizeDelta = new Vector2(diff.x + 20, diff.y + 20);
        }
    }

    public void Clear() {
        foreach (Image segment in segments) {
            segment.gameObject.SetActive(false);
        }
    }
}
