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

    public static void ResetScore(object s, EventArgs e) {
        currScore = 0;
    }

    void Awake() {
        GameManager.Instance.SettingUpNewGame += Score.ResetScore;
    }

    public void UpdateScore() {
        guiText.text = "Score: " + currScore.ToString();
    }
}
