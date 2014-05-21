using UnityEngine;
using System;
using System.Collections;

public class Score : MonoBehaviour {
    static int currScore = 0;

    public static int CurrentScore {
        get {
            return currScore;
        }
        set {
            currScore = value;
        }
    }

    public static void AddScore(int val) {
        currScore += val;
    }

    public static void ResetScore() {
        currScore = 0;
    }

    public void UpdateScore() {
        guiText.text = "Score: " + currScore.ToString();
    }
}
