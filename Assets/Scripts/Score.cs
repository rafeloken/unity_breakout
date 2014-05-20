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

    // TODO: Change this to not update score every frame regardless of new score!
    void Update() {
        guiText.text = "Score: " + currScore.ToString();
    }
}
